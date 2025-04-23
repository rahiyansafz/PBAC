using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AccessManagementAPI.Core.Services;

public class RoleService(
    IRoleRepository roleRepository,
    IUserRepository userRepository,
    IMemoryCache cache)
    : IRoleService
{
    private const string ROLE_CACHE_KEY_PREFIX = "role_";
    private const string ROLE_PERMISSIONS_CACHE_KEY_PREFIX = "role_permissions_";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await roleRepository.GetAllAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        var cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{roleId}";

        if (!cache.TryGetValue(cacheKey, out Role? role))
        {
            role = await roleRepository.GetByIdAsync(roleId);
            if (role != null) cache.Set(cacheKey, role, CACHE_DURATION);
        }

        return role;
    }

    public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
    {
        return await roleRepository.GetRoleWithPermissionsAsync(roleId);
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        var newRole = await roleRepository.AddAsync(role);
        return newRole;
    }

    public async Task UpdateRoleAsync(Role role)
    {
        await roleRepository.UpdateAsync(role);
        var cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{role.Id}";
        cache.Remove(cacheKey);
    }

    public async Task DeleteRoleAsync(int roleId)
    {
        var role = await roleRepository.GetByIdAsync(roleId);
        if (role != null && !role.IsSystemRole)
        {
            await roleRepository.DeleteAsync(role);
            var cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{roleId}";
            cache.Remove(cacheKey);
        }
        else if (role != null && role.IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be deleted");
        }
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        var cacheKey = $"{ROLE_PERMISSIONS_CACHE_KEY_PREFIX}{roleId}";

        if (!cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
        {
            permissions = await roleRepository.GetRolePermissionsAsync(roleId);
            cache.Set(cacheKey, permissions, CACHE_DURATION);
        }

        return permissions ?? Array.Empty<Permission>();
    }

    public async Task AddPermissionToRoleAsync(int roleId, int permissionId)
    {
        await roleRepository.AddPermissionToRoleAsync(roleId, permissionId);
        var cacheKey = $"{ROLE_PERMISSIONS_CACHE_KEY_PREFIX}{roleId}";
        cache.Remove(cacheKey);
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        await roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
        var cacheKey = $"{ROLE_PERMISSIONS_CACHE_KEY_PREFIX}{roleId}";
        cache.Remove(cacheKey);
    }

    public async Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId)
    {
        // Get users by role
        var users = await userRepository.FindAsync(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
        return users;
    }

    public async Task AddUserToRoleAsync(int userId, int roleId)
    {
        var user = await userRepository.GetByIdAsync(userId);
        var role = await roleRepository.GetByIdAsync(roleId);

        if (user != null && role != null)
        {
            var userRoles = await userRepository.GetUserWithRolesAsync(userId);
            if (userRoles?.UserRoles.Any(ur => ur.RoleId == roleId) != true)
            {
                userRoles?.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                await userRepository.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveUserFromRoleAsync(int userId, int roleId)
    {
        var user = await userRepository.GetUserWithRolesAsync(userId);
        if (user != null)
        {
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole != null)
            {
                user.UserRoles.Remove(userRole);
                await userRepository.SaveChangesAsync();
            }
        }
    }
}
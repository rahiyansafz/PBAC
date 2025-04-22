using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AccessManagementAPI.Core.Services;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IUserRepository _userRepository;
    private readonly IMemoryCache _cache;
    private const string ROLE_CACHE_KEY_PREFIX = "role_";
    private const string ROLE_PERMISSIONS_CACHE_KEY_PREFIX = "role_permissions_";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);

    public RoleService(
        IRoleRepository roleRepository,
        IUserRepository userRepository,
        IMemoryCache cache)
    {
        _roleRepository = roleRepository;
        _userRepository = userRepository;
        _cache = cache;
    }

    public async Task<IEnumerable<Role>> GetAllRolesAsync()
    {
        return await _roleRepository.GetAllAsync();
    }

    public async Task<Role?> GetRoleByIdAsync(int roleId)
    {
        string cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{roleId}";

        if (!_cache.TryGetValue(cacheKey, out Role? role))
        {
            role = await _roleRepository.GetByIdAsync(roleId);
            if (role != null)
            {
                _cache.Set(cacheKey, role, CACHE_DURATION);
            }
        }

        return role;
    }

    public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
    {
        return await _roleRepository.GetRoleWithPermissionsAsync(roleId);
    }

    public async Task<Role> CreateRoleAsync(Role role)
    {
        var newRole = await _roleRepository.AddAsync(role);
        return newRole;
    }

    public async Task UpdateRoleAsync(Role role)
    {
        await _roleRepository.UpdateAsync(role);
        string cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{role.Id}";
        _cache.Remove(cacheKey);
    }

    public async Task DeleteRoleAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId);
        if (role != null && !role.IsSystemRole)
        {
            await _roleRepository.DeleteAsync(role);
            string cacheKey = $"{ROLE_CACHE_KEY_PREFIX}{roleId}";
            _cache.Remove(cacheKey);
        }
        else if (role != null && role.IsSystemRole)
        {
            throw new InvalidOperationException("System roles cannot be deleted");
        }
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        string cacheKey = $"{ROLE_PERMISSIONS_CACHE_KEY_PREFIX}{roleId}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
        {
            permissions = await _roleRepository.GetRolePermissionsAsync(roleId);
            _cache.Set(cacheKey, permissions, CACHE_DURATION);
        }

        return permissions;
    }

    public async Task AddPermissionToRoleAsync(int roleId, int permissionId)
    {
        await _roleRepository.AddPermissionToRoleAsync(roleId, permissionId);
        string cacheKey = $"{ROLE_PERMISSIONS_CACHE_KEY_PREFIX}{roleId}";
        _cache.Remove(cacheKey);
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        await _roleRepository.RemovePermissionFromRoleAsync(roleId, permissionId);
        string cacheKey = $"{ROLE_PERMISSIONS_CACHE_KEY_PREFIX}{roleId}";
        _cache.Remove(cacheKey);
    }

    public async Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId)
    {
        // Get users by role
        var users = await _userRepository.FindAsync(u => u.UserRoles.Any(ur => ur.RoleId == roleId));
        return users;
    }

    public async Task AddUserToRoleAsync(int userId, int roleId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        var role = await _roleRepository.GetByIdAsync(roleId);

        if (user != null && role != null)
        {
            var userRoles = await _userRepository.GetUserWithRolesAsync(userId);
            if (userRoles?.UserRoles.Any(ur => ur.RoleId == roleId) != true)
            {
                userRoles?.UserRoles.Add(new UserRole { UserId = userId, RoleId = roleId });
                await _userRepository.SaveChangesAsync();
            }
        }
    }

    public async Task RemoveUserFromRoleAsync(int userId, int roleId)
    {
        var user = await _userRepository.GetUserWithRolesAsync(userId);
        if (user != null)
        {
            var userRole = user.UserRoles.FirstOrDefault(ur => ur.RoleId == roleId);
            if (userRole != null)
            {
                user.UserRoles.Remove(userRole);
                await _userRepository.SaveChangesAsync();
            }
        }
    }
}
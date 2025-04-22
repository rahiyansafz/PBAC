using AccessManagementAPI.Core.Models;

namespace AccessManagementAPI.Core.Services;

public interface IRoleService
{
    Task<IEnumerable<Role>> GetAllRolesAsync();
    Task<Role?> GetRoleByIdAsync(int roleId);
    Task<Role?> GetRoleWithPermissionsAsync(int roleId);
    Task<Role> CreateRoleAsync(Role role);
    Task UpdateRoleAsync(Role role);
    Task DeleteRoleAsync(int roleId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
    Task AddPermissionToRoleAsync(int roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
    Task<IEnumerable<User>> GetUsersInRoleAsync(int roleId);
    Task AddUserToRoleAsync(int userId, int roleId);
    Task RemoveUserFromRoleAsync(int userId, int roleId);
}
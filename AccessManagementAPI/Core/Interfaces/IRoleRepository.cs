using AccessManagementAPI.Core.Models;

namespace AccessManagementAPI.Core.Interfaces;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetRoleWithPermissionsAsync(int roleId);
    Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId);
    Task AddPermissionToRoleAsync(int roleId, int permissionId);
    Task RemovePermissionFromRoleAsync(int roleId, int permissionId);
    Task AddUserToRoleAsync(int userId, int roleId);
}
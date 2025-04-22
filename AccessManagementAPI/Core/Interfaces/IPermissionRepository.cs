using AccessManagementAPI.Core.Models;

namespace AccessManagementAPI.Core.Interfaces;

public interface IPermissionRepository : IRepository<Permission>
{
    Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category);
    Task<Permission?> GetPermissionBySystemNameAsync(string systemName);
}
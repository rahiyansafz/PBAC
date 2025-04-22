using AccessManagementAPI.Core.Models;

namespace AccessManagementAPI.Core.Services;

public interface IPermissionService
{
    Task<bool> AuthorizeAsync(int userId, string permissionSystemName);
    Task<bool> HasPermissionAsync(int userId, string permissionSystemName);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
    Task<IEnumerable<string>> GetUserPermissionNamesAsync(int userId);
    Task<IEnumerable<MenuItem>> GetAuthorizedMenuItemsAsync(int userId);
}
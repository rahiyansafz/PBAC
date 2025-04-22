using AccessManagementAPI.Core.Models;

namespace AccessManagementAPI.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetUserWithRolesAsync(int userId);
    Task<User?> GetUserByUsernameAsync(string username);
    Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId);
}
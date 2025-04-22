using AccessManagementAPI.Core.Models;

namespace AccessManagementAPI.Core.Interfaces;

public interface IMenuItemRepository : IRepository<MenuItem>
{
    Task<IEnumerable<MenuItem>> GetVisibleMenuItemsForRoleAsync(int roleId);
    Task<IEnumerable<MenuItem>> GetTopLevelMenuItemsAsync();
    Task<IEnumerable<MenuItem>> GetMenuItemsWithParentAsync(int parentId);
}
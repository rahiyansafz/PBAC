using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccessManagementAPI.Core.Repositories;

public class MenuItemRepository : Repository<MenuItem>, IMenuItemRepository
{
    public MenuItemRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<MenuItem>> GetVisibleMenuItemsForRoleAsync(int roleId)
    {
        // Get role permissions
        var permissionSystemNames = await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .Join(_context.Permissions,
                rp => rp.PermissionId,
                p => p.Id,
                (rp, p) => p.SystemName)
            .ToListAsync();

        // Get menu items where RequiredPermissionSystemName is empty or in the user's permissions
        return await _context.MenuItems
            .Where(mi => mi.IsVisible && (
                string.IsNullOrEmpty(mi.RequiredPermissionSystemName) ||
                permissionSystemNames.Contains(mi.RequiredPermissionSystemName)
            ))
            .OrderBy(mi => mi.ParentId)
            .ThenBy(mi => mi.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetTopLevelMenuItemsAsync()
    {
        return await _context.MenuItems
            .Where(mi => mi.ParentId == 0 && mi.IsVisible)
            .OrderBy(mi => mi.DisplayOrder)
            .ToListAsync();
    }

    public async Task<IEnumerable<MenuItem>> GetMenuItemsWithParentAsync(int parentId)
    {
        return await _context.MenuItems
            .Where(mi => mi.ParentId == parentId && mi.IsVisible)
            .OrderBy(mi => mi.DisplayOrder)
            .ToListAsync();
    }
}
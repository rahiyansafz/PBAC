using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccessManagementAPI.Core.Repositories;

public class RoleRepository(ApplicationDbContext context) : Repository<Role>(context), IRoleRepository
{
    public async Task<Role?> GetRoleWithPermissionsAsync(int roleId)
    {
        return await _context.Roles.Include(r => r.RolePermissions).ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == roleId);
    }

    public async Task<IEnumerable<Permission>> GetRolePermissionsAsync(int roleId)
    {
        return await _context.RolePermissions.Where(rp => rp.RoleId == roleId).Select(rp => rp.Permission)
            .ToListAsync();
    }

    public async Task AddPermissionToRoleAsync(int roleId, int permissionId)
    {
        var exists =
            await _context.RolePermissions.AnyAsync(rp => rp.RoleId == roleId && rp.PermissionId == permissionId);
        if (!exists)
        {
            await _context.RolePermissions.AddAsync(new RolePermission
            {
                RoleId = roleId, PermissionId = permissionId
            });
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemovePermissionFromRoleAsync(int roleId, int permissionId)
    {
        var rolePermission =
            await _context.RolePermissions.FirstOrDefaultAsync(rp =>
                rp.RoleId == roleId && rp.PermissionId == permissionId);
        if (rolePermission != null)
        {
            _context.RolePermissions.Remove(rolePermission);
            await _context.SaveChangesAsync();
        }
    }

    public async Task AddUserToRoleAsync(int userId, int roleId)
    {
        var exists = await _context.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId);
        if (!exists)
        {
            await _context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId });
            await _context.SaveChangesAsync();
        }
    }
}
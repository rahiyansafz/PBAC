using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AccessManagementAPI.Core.Repositories;

public class PermissionRepository : Repository<Permission>, IPermissionRepository
{
    public PermissionRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByCategoryAsync(string category)
    {
        return await _context.Permissions
            .Where(p => p.Category == category)
            .ToListAsync();
    }

    public async Task<Permission?> GetPermissionBySystemNameAsync(string systemName)
    {
        return await _context.Permissions
            .FirstOrDefaultAsync(p => p.SystemName == systemName);
    }
}
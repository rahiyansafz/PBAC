using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using Microsoft.Extensions.Caching.Memory;

namespace AccessManagementAPI.Core.Services;

public class PermissionService : IPermissionService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly IMenuItemRepository _menuItemRepository;
    private readonly IMemoryCache _cache;
    private const string PERMISSION_CACHE_KEY_PREFIX = "user_permissions_";
    private const string MENU_CACHE_KEY_PREFIX = "user_menus_";
    private static readonly TimeSpan CACHE_DURATION = TimeSpan.FromMinutes(10);

    public PermissionService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IPermissionRepository permissionRepository,
        IMenuItemRepository menuItemRepository,
        IMemoryCache cache)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _permissionRepository = permissionRepository;
        _menuItemRepository = menuItemRepository;
        _cache = cache;
    }

    public async Task<bool> AuthorizeAsync(int userId, string permissionSystemName)
    {
        // If permission is empty, allow access
        if (string.IsNullOrEmpty(permissionSystemName))
            return true;

        return await HasPermissionAsync(userId, permissionSystemName);
    }

    public async Task<bool> HasPermissionAsync(int userId, string permissionSystemName)
    {
        var permissions = await GetUserPermissionNamesAsync(userId);
        return permissions.Contains(permissionSystemName);
    }

    public async Task<IEnumerable<Permission>> GetUserPermissionsAsync(int userId)
    {
        string cacheKey = $"{PERMISSION_CACHE_KEY_PREFIX}{userId}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<Permission> permissions))
        {
            permissions = await _userRepository.GetUserPermissionsAsync(userId);
            _cache.Set(cacheKey, permissions, CACHE_DURATION);
        }

        return permissions;
    }

    public async Task<IEnumerable<string>> GetUserPermissionNamesAsync(int userId)
    {
        var permissions = await GetUserPermissionsAsync(userId);
        return permissions.Select(p => p.SystemName).Distinct();
    }

    public async Task<IEnumerable<MenuItem>> GetAuthorizedMenuItemsAsync(int userId)
    {
        string cacheKey = $"{MENU_CACHE_KEY_PREFIX}{userId}";

        if (!_cache.TryGetValue(cacheKey, out IEnumerable<MenuItem> menuItems))
        {
            var user = await _userRepository.GetUserWithRolesAsync(userId);
            if (user == null)
                return Enumerable.Empty<MenuItem>();

            var allMenuItems = new List<MenuItem>();

            foreach (var userRole in user.UserRoles)
            {
                var roleMenuItems = await _menuItemRepository.GetVisibleMenuItemsForRoleAsync(userRole.RoleId);
                allMenuItems.AddRange(roleMenuItems);
            }

            // Remove duplicates
            menuItems = allMenuItems.GroupBy(m => m.Id).Select(g => g.First()).ToList();

            // Order by ParentId first, then DisplayOrder
            menuItems = menuItems.OrderBy(m => m.ParentId).ThenBy(m => m.DisplayOrder).ToList();

            _cache.Set(cacheKey, menuItems, CACHE_DURATION);
        }

        return menuItems;
    }
}
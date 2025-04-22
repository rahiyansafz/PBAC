using System.ComponentModel.DataAnnotations;
using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController : ControllerBase
{
    private readonly IMenuItemRepository _menuItemRepository;

    public MenuItemsController(IMenuItemRepository menuItemRepository)
    {
        _menuItemRepository = menuItemRepository;
    }

    // GET: api/menuitems
    [HttpGet]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
    {
        var menuItems = await _menuItemRepository.GetAllAsync();
        return Ok(menuItems);
    }

    // GET: api/menuitems/5
    [HttpGet("{id}")]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        return Ok(menuItem);
    }

    // GET: api/menuitems/toplevel
    [HttpGet("toplevel")]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetTopLevelMenuItems()
    {
        var menuItems = await _menuItemRepository.GetTopLevelMenuItemsAsync();
        return Ok(menuItems);
    }

    // GET: api/menuitems/parent/5
    [HttpGet("parent/{parentId}")]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItemsByParent(int parentId)
    {
        var menuItems = await _menuItemRepository.GetMenuItemsWithParentAsync(parentId);
        return Ok(menuItems);
    }

    // POST: api/menuitems
    [HttpPost]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<MenuItem>> CreateMenuItem([FromBody] MenuItemCreateDto menuItemDto)
    {
        var menuItem = new MenuItem
        {
            Name = menuItemDto.Name,
            DisplayName = menuItemDto.DisplayName,
            Url = menuItemDto.Url,
            Icon = menuItemDto.Icon,
            ParentId = menuItemDto.ParentId,
            DisplayOrder = menuItemDto.DisplayOrder,
            IsVisible = menuItemDto.IsVisible,
            RequiredPermissionSystemName = menuItemDto.RequiredPermissionSystemName
        };

        var createdMenuItem = await _menuItemRepository.AddAsync(menuItem);
        return CreatedAtAction(nameof(GetMenuItem), new { id = createdMenuItem.Id }, createdMenuItem);
    }

    // PUT: api/menuitems/5
    [HttpPut("{id}")]
    [HasPermission("menus.manage")]
    public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItemUpdateDto menuItemDto)
    {
        var existingMenuItem = await _menuItemRepository.GetByIdAsync(id);
        if (existingMenuItem == null)
        {
            return NotFound();
        }

        existingMenuItem.Name = menuItemDto.Name;
        existingMenuItem.DisplayName = menuItemDto.DisplayName;
        existingMenuItem.Url = menuItemDto.Url;
        existingMenuItem.Icon = menuItemDto.Icon;
        existingMenuItem.ParentId = menuItemDto.ParentId;
        existingMenuItem.DisplayOrder = menuItemDto.DisplayOrder;
        existingMenuItem.IsVisible = menuItemDto.IsVisible;
        existingMenuItem.RequiredPermissionSystemName = menuItemDto.RequiredPermissionSystemName;

        await _menuItemRepository.UpdateAsync(existingMenuItem);
        return NoContent();
    }

    // DELETE: api/menuitems/5
    [HttpDelete("{id}")]
    [HasPermission("menus.manage")]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var menuItem = await _menuItemRepository.GetByIdAsync(id);
        if (menuItem == null)
        {
            return NotFound();
        }

        // Check if there are any children of this menu item
        var children = await _menuItemRepository.GetMenuItemsWithParentAsync(id);
        if (children.Any())
        {
            return BadRequest(
                "Cannot delete a menu item with children. Delete children first or reassign them to another parent.");
        }

        await _menuItemRepository.DeleteAsync(menuItem);
        return NoContent();
    }
}

public class MenuItemCreateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string DisplayName { get; set; } = string.Empty;

    [Required] [StringLength(255)] public string Url { get; set; } = string.Empty;

    [StringLength(50)] public string Icon { get; set; } = string.Empty;

    public int ParentId { get; set; } = 0;

    public int DisplayOrder { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    [StringLength(50)] public string RequiredPermissionSystemName { get; set; } = string.Empty;
}

public class MenuItemUpdateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string DisplayName { get; set; } = string.Empty;

    [Required] [StringLength(255)] public string Url { get; set; } = string.Empty;

    [StringLength(50)] public string Icon { get; set; } = string.Empty;

    public int ParentId { get; set; } = 0;

    public int DisplayOrder { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    [StringLength(50)] public string RequiredPermissionSystemName { get; set; } = string.Empty;
}
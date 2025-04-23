using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Dtos.MenuItems;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuItemsController(IMenuItemRepository menuItemRepository) : ControllerBase
{
    // GET: api/menuitems
    [HttpGet]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItems()
    {
        var menuItems = await menuItemRepository.GetAllAsync();
        return Ok(menuItems);
    }

    // GET: api/menuitems/5
    [HttpGet("{id:int}")]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<MenuItem>> GetMenuItem(int id)
    {
        var menuItem = await menuItemRepository.GetByIdAsync(id);
        if (menuItem == null) return NotFound();

        return Ok(menuItem);
    }

    // GET: api/menuitems/toplevel
    [HttpGet("toplevel")]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetTopLevelMenuItems()
    {
        var menuItems = await menuItemRepository.GetTopLevelMenuItemsAsync();
        return Ok(menuItems);
    }

    // GET: api/menuitems/parent/5
    [HttpGet("parent/{parentId:int}")]
    [HasPermission("menus.manage")]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetMenuItemsByParent(int parentId)
    {
        var menuItems = await menuItemRepository.GetMenuItemsWithParentAsync(parentId);
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

        var createdMenuItem = await menuItemRepository.AddAsync(menuItem);
        return CreatedAtAction(nameof(GetMenuItem), new { id = createdMenuItem.Id }, createdMenuItem);
    }

    // PUT: api/menuitems/5
    [HttpPut("{id:int}")]
    [HasPermission("menus.manage")]
    public async Task<IActionResult> UpdateMenuItem(int id, [FromBody] MenuItemUpdateDto menuItemDto)
    {
        var existingMenuItem = await menuItemRepository.GetByIdAsync(id);
        if (existingMenuItem == null) return NotFound();

        existingMenuItem.Name = menuItemDto.Name;
        existingMenuItem.DisplayName = menuItemDto.DisplayName;
        existingMenuItem.Url = menuItemDto.Url;
        existingMenuItem.Icon = menuItemDto.Icon;
        existingMenuItem.ParentId = menuItemDto.ParentId;
        existingMenuItem.DisplayOrder = menuItemDto.DisplayOrder;
        existingMenuItem.IsVisible = menuItemDto.IsVisible;
        existingMenuItem.RequiredPermissionSystemName = menuItemDto.RequiredPermissionSystemName;

        await menuItemRepository.UpdateAsync(existingMenuItem);
        return NoContent();
    }

    // DELETE: api/menuitems/5
    [HttpDelete("{id:int}")]
    [HasPermission("menus.manage")]
    public async Task<IActionResult> DeleteMenuItem(int id)
    {
        var menuItem = await menuItemRepository.GetByIdAsync(id);
        if (menuItem == null) return NotFound();

        // Check if there are any children of this menu item
        var children = await menuItemRepository.GetMenuItemsWithParentAsync(id);
        if (children.Any())
            return BadRequest(
                "Cannot delete a menu item with children. Delete children first or reassign them to another parent.");

        await menuItemRepository.DeleteAsync(menuItem);
        return NoContent();
    }
}
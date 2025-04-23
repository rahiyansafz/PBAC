using System.Security.Claims;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserMenuController(IPermissionService permissionService) : ControllerBase
{
    // GET: api/usermenu
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetUserMenu()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var menuItems = await permissionService.GetAuthorizedMenuItemsAsync(userId);
        return Ok(menuItems);
    }
}
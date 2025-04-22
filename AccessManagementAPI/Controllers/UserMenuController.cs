using System.Security.Claims;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserMenuController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public UserMenuController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    // GET: api/usermenu
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MenuItem>>> GetUserMenu()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized();
        }

        var menuItems = await _permissionService.GetAuthorizedMenuItemsAsync(userId);
        return Ok(menuItems);
    }
}
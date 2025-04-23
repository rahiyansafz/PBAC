using System.Security.Claims;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPermissionsController(IPermissionService permissionService) : ControllerBase
{
    // GET: api/userpermissions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Permission>>> GetUserPermissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var permissions = await permissionService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }

    // GET: api/userpermissions/names
    [HttpGet("names")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserPermissionNames()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var permissionNames = await permissionService.GetUserPermissionNamesAsync(userId);
        return Ok(permissionNames);
    }

    // GET: api/userpermissions/check/results.view
    [HttpGet("check/{permissionName}")]
    public async Task<ActionResult<bool>> CheckPermission(string permissionName)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var hasPermission = await permissionService.HasPermissionAsync(userId, permissionName);
        return Ok(hasPermission);
    }
}
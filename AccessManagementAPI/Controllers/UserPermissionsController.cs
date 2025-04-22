using System.Security.Claims;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserPermissionsController : ControllerBase
{
    private readonly IPermissionService _permissionService;

    public UserPermissionsController(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    // GET: api/userpermissions
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Permission>>> GetUserPermissions()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized();
        }

        var permissions = await _permissionService.GetUserPermissionsAsync(userId);
        return Ok(permissions);
    }

    // GET: api/userpermissions/names
    [HttpGet("names")]
    public async Task<ActionResult<IEnumerable<string>>> GetUserPermissionNames()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized();
        }

        var permissionNames = await _permissionService.GetUserPermissionNamesAsync(userId);
        return Ok(permissionNames);
    }

    // GET: api/userpermissions/check/results.view
    [HttpGet("check/{permissionName}")]
    public async Task<ActionResult<bool>> CheckPermission(string permissionName)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized();
        }

        var hasPermission = await _permissionService.HasPermissionAsync(userId, permissionName);
        return Ok(hasPermission);
    }
}
using System.ComponentModel.DataAnnotations;
using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    // GET: api/roles
    [HttpGet]
    [HasPermission("roles.view")]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        var roles = await _roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    // GET: api/roles/5
    [HttpGet("{id}")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<Role>> GetRole(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        return Ok(role);
    }

    // GET: api/roles/5/permissions
    [HttpGet("{id}/permissions")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetRolePermissions(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        var permissions = await _roleService.GetRolePermissionsAsync(id);
        return Ok(permissions);
    }

    // POST: api/roles
    [HttpPost]
    [HasPermission("roles.create")]
    public async Task<ActionResult<Role>> CreateRole([FromBody] RoleCreateDto roleDto)
    {
        var role = new Role
        {
            Name = roleDto.Name,
            SystemName = roleDto.SystemName,
            Description = roleDto.Description,
            IsSystemRole = false
        };

        var createdRole = await _roleService.CreateRoleAsync(role);
        return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, createdRole);
    }

    // PUT: api/roles/5
    [HttpPut("{id}")]
    [HasPermission("roles.edit")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateDto roleDto)
    {
        var existingRole = await _roleService.GetRoleByIdAsync(id);
        if (existingRole == null)
        {
            return NotFound();
        }

        if (existingRole.IsSystemRole && existingRole.SystemName != roleDto.SystemName)
        {
            return BadRequest("Cannot change the SystemName of a system role");
        }

        existingRole.Name = roleDto.Name;
        existingRole.SystemName = roleDto.SystemName;
        existingRole.Description = roleDto.Description;

        await _roleService.UpdateRoleAsync(existingRole);
        return NoContent();
    }

    // DELETE: api/roles/5
    [HttpDelete("{id}")]
    [HasPermission("roles.delete")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await _roleService.GetRoleByIdAsync(id);
        if (role == null)
        {
            return NotFound();
        }

        if (role.IsSystemRole)
        {
            return BadRequest("Cannot delete a system role");
        }

        await _roleService.DeleteRoleAsync(id);
        return NoContent();
    }

    // POST: api/roles/5/permissions
    [HttpPost("{roleId}/permissions/{permissionId}")]
    [HasPermission("permissions.assign")]
    public async Task<IActionResult> AddPermissionToRole(int roleId, int permissionId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }

        await _roleService.AddPermissionToRoleAsync(roleId, permissionId);
        return NoContent();
    }

    // DELETE: api/roles/5/permissions/3
    [HttpDelete("{roleId}/permissions/{permissionId}")]
    [HasPermission("permissions.assign")]
    public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }

        await _roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        return NoContent();
    }

    // GET: api/roles/5/users
    [HttpGet("{roleId}/users")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersInRole(int roleId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }

        var users = await _roleService.GetUsersInRoleAsync(roleId);
        return Ok(users);
    }

    // POST: api/roles/5/users/3
    [HttpPost("{roleId}/users/{userId}")]
    [HasPermission("roles.edit")]
    public async Task<IActionResult> AddUserToRole(int roleId, int userId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }

        await _roleService.AddUserToRoleAsync(userId, roleId);
        return NoContent();
    }

    // DELETE: api/roles/5/users/3
    [HttpDelete("{roleId}/users/{userId}")]
    [HasPermission("roles.edit")]
    public async Task<IActionResult> RemoveUserFromRole(int roleId, int userId)
    {
        var role = await _roleService.GetRoleByIdAsync(roleId);
        if (role == null)
        {
            return NotFound("Role not found");
        }

        await _roleService.RemoveUserFromRoleAsync(userId, roleId);
        return NoContent();
    }
}

public class RoleCreateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string SystemName { get; set; } = string.Empty;

    [StringLength(255)] public string Description { get; set; } = string.Empty;
}

public class RoleUpdateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string SystemName { get; set; } = string.Empty;

    [StringLength(255)] public string Description { get; set; } = string.Empty;
}
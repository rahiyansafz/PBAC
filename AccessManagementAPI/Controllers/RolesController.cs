using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Services;
using AccessManagementAPI.Dtos.Roles;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController(IRoleService roleService) : ControllerBase
{
    // GET: api/roles
    [HttpGet]
    [HasPermission("roles.view")]
    public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
    {
        var roles = await roleService.GetAllRolesAsync();
        return Ok(roles);
    }

    // GET: api/roles/5
    [HttpGet("{id:int}")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<Role>> GetRole(int id)
    {
        var role = await roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();

        return Ok(role);
    }

    // GET: api/roles/5/permissions
    [HttpGet("{id:int}/permissions")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetRolePermissions(int id)
    {
        var role = await roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();

        var permissions = await roleService.GetRolePermissionsAsync(id);
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

        var createdRole = await roleService.CreateRoleAsync(role);
        return CreatedAtAction(nameof(GetRole), new { id = createdRole.Id }, createdRole);
    }

    // PUT: api/roles/5
    [HttpPut("{id:int}")]
    [HasPermission("roles.edit")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] RoleUpdateDto roleDto)
    {
        var existingRole = await roleService.GetRoleByIdAsync(id);
        if (existingRole == null) return NotFound();

        if (existingRole.IsSystemRole && existingRole.SystemName != roleDto.SystemName)
            return BadRequest("Cannot change the SystemName of a system role");

        existingRole.Name = roleDto.Name;
        existingRole.SystemName = roleDto.SystemName;
        existingRole.Description = roleDto.Description;

        await roleService.UpdateRoleAsync(existingRole);
        return NoContent();
    }

    // DELETE: api/roles/5
    [HttpDelete("{id:int}")]
    [HasPermission("roles.delete")]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var role = await roleService.GetRoleByIdAsync(id);
        if (role == null) return NotFound();

        if (role.IsSystemRole) return BadRequest("Cannot delete a system role");

        await roleService.DeleteRoleAsync(id);
        return NoContent();
    }

    // POST: api/roles/5/permissions
    [HttpPost("{roleId:int}/permissions/{permissionId:int}")]
    [HasPermission("permissions.assign")]
    public async Task<IActionResult> AddPermissionToRole(int roleId, int permissionId)
    {
        var role = await roleService.GetRoleByIdAsync(roleId);
        if (role == null) return NotFound("Role not found");

        await roleService.AddPermissionToRoleAsync(roleId, permissionId);
        return NoContent();
    }

    // DELETE: api/roles/5/permissions/3
    [HttpDelete("{roleId:int}/permissions/{permissionId:int}")]
    [HasPermission("permissions.assign")]
    public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
    {
        var role = await roleService.GetRoleByIdAsync(roleId);
        if (role == null) return NotFound("Role not found");

        await roleService.RemovePermissionFromRoleAsync(roleId, permissionId);
        return NoContent();
    }

    // GET: api/roles/5/users
    [HttpGet("{roleId:int}/users")]
    [HasPermission("roles.view")]
    public async Task<ActionResult<IEnumerable<User>>> GetUsersInRole(int roleId)
    {
        var role = await roleService.GetRoleByIdAsync(roleId);
        if (role == null) return NotFound("Role not found");

        var users = await roleService.GetUsersInRoleAsync(roleId);
        return Ok(users);
    }

    // POST: api/roles/5/users/3
    [HttpPost("{roleId:int}/users/{userId:int}")]
    [HasPermission("roles.edit")]
    public async Task<IActionResult> AddUserToRole(int roleId, int userId)
    {
        var role = await roleService.GetRoleByIdAsync(roleId);
        if (role == null) return NotFound("Role not found");

        await roleService.AddUserToRoleAsync(userId, roleId);
        return NoContent();
    }

    // DELETE: api/roles/5/users/3
    [HttpDelete("{roleId:int}/users/{userId:int}")]
    [HasPermission("roles.edit")]
    public async Task<IActionResult> RemoveUserFromRole(int roleId, int userId)
    {
        var role = await roleService.GetRoleByIdAsync(roleId);
        if (role == null) return NotFound("Role not found");

        await roleService.RemoveUserFromRoleAsync(userId, roleId);
        return NoContent();
    }
}
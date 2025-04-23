using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Dtos.Permissions;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PermissionsController(IPermissionRepository permissionRepository) : ControllerBase
{
    // GET: api/permissions
    [HttpGet]
    [HasPermission("permissions.view")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
    {
        var permissions = await permissionRepository.GetAllAsync();
        return Ok(permissions);
    }

    // GET: api/permissions/5
    [HttpGet("{id:int}")]
    [HasPermission("permissions.view")]
    public async Task<ActionResult<Permission>> GetPermission(int id)
    {
        var permission = await permissionRepository.GetByIdAsync(id);
        if (permission == null) return NotFound();

        return Ok(permission);
    }

    // GET: api/permissions/category/Users
    [HttpGet("category/{category}")]
    [HasPermission("permissions.view")]
    public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByCategory(string category)
    {
        var permissions = await permissionRepository.GetPermissionsByCategoryAsync(category);
        return Ok(permissions);
    }

    // POST: api/permissions
    [HttpPost]
    [HasPermission("permissions.assign")]
    public async Task<ActionResult<Permission>> CreatePermission([FromBody] PermissionCreateDto permissionDto)
    {
        var existingPermission = await permissionRepository.GetPermissionBySystemNameAsync(permissionDto.SystemName);
        if (existingPermission != null) return BadRequest("A permission with this system name already exists");

        var permission = new Permission
        {
            Name = permissionDto.Name,
            SystemName = permissionDto.SystemName,
            Description = permissionDto.Description,
            Category = permissionDto.Category,
            Action = permissionDto.Action,
            Resource = permissionDto.Resource
        };

        var createdPermission = await permissionRepository.AddAsync(permission);
        return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.Id }, createdPermission);
    }

    // PUT: api/permissions/5
    [HttpPut("{id:int}")]
    [HasPermission("permissions.assign")]
    public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionUpdateDto permissionDto)
    {
        var existingPermission = await permissionRepository.GetByIdAsync(id);
        if (existingPermission == null) return NotFound();

        // Check if the new system name is already used by another permission
        if (existingPermission.SystemName != permissionDto.SystemName)
        {
            var permissionWithSameSystemName =
                await permissionRepository.GetPermissionBySystemNameAsync(permissionDto.SystemName);
            if (permissionWithSameSystemName != null && permissionWithSameSystemName.Id != id)
                return BadRequest("A permission with this system name already exists");
        }

        existingPermission.Name = permissionDto.Name;
        existingPermission.SystemName = permissionDto.SystemName;
        existingPermission.Description = permissionDto.Description;
        existingPermission.Category = permissionDto.Category;
        existingPermission.Action = permissionDto.Action;
        existingPermission.Resource = permissionDto.Resource;

        await permissionRepository.UpdateAsync(existingPermission);
        return NoContent();
    }

    // DELETE: api/permissions/5
    [HttpDelete("{id:int}")]
    [HasPermission("permissions.assign")]
    public async Task<IActionResult> DeletePermission(int id)
    {
        var permission = await permissionRepository.GetByIdAsync(id);
        if (permission == null) return NotFound();

        await permissionRepository.DeleteAsync(permission);
        return NoContent();
    }
}
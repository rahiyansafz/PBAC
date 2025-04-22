using System.ComponentModel.DataAnnotations;
using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using Microsoft.AspNetCore.Mvc;

namespace AccessManagementAPI.Controllers;

[ApiController]
    [Route("api/[controller]")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionRepository _permissionRepository;

        public PermissionsController(IPermissionRepository permissionRepository)
        {
            _permissionRepository = permissionRepository;
        }

        // GET: api/permissions
        [HttpGet]
        [HasPermission("permissions.view")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
        {
            var permissions = await _permissionRepository.GetAllAsync();
            return Ok(permissions);
        }

        // GET: api/permissions/5
        [HttpGet("{id}")]
        [HasPermission("permissions.view")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound();
            }
            return Ok(permission);
        }

        // GET: api/permissions/category/Users
        [HttpGet("category/{category}")]
        [HasPermission("permissions.view")]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissionsByCategory(string category)
        {
            var permissions = await _permissionRepository.GetPermissionsByCategoryAsync(category);
            return Ok(permissions);
        }

        // POST: api/permissions
        [HttpPost]
        [HasPermission("permissions.assign")]
        public async Task<ActionResult<Permission>> CreatePermission([FromBody] PermissionCreateDto permissionDto)
        {
            var existingPermission = await _permissionRepository.GetPermissionBySystemNameAsync(permissionDto.SystemName);
            if (existingPermission != null)
            {
                return BadRequest("A permission with this system name already exists");
            }

            var permission = new Permission
            {
                Name = permissionDto.Name,
                SystemName = permissionDto.SystemName,
                Description = permissionDto.Description,
                Category = permissionDto.Category,
                Action = permissionDto.Action,
                Resource = permissionDto.Resource
            };

            var createdPermission = await _permissionRepository.AddAsync(permission);
            return CreatedAtAction(nameof(GetPermission), new { id = createdPermission.Id }, createdPermission);
        }

        // PUT: api/permissions/5
        [HttpPut("{id}")]
        [HasPermission("permissions.assign")]
        public async Task<IActionResult> UpdatePermission(int id, [FromBody] PermissionUpdateDto permissionDto)
        {
            var existingPermission = await _permissionRepository.GetByIdAsync(id);
            if (existingPermission == null)
            {
                return NotFound();
            }

            // Check if the new system name is already used by another permission
            if (existingPermission.SystemName != permissionDto.SystemName)
            {
                var permissionWithSameSystemName = await _permissionRepository.GetPermissionBySystemNameAsync(permissionDto.SystemName);
                if (permissionWithSameSystemName != null && permissionWithSameSystemName.Id != id)
                {
                    return BadRequest("A permission with this system name already exists");
                }
            }

            existingPermission.Name = permissionDto.Name;
            existingPermission.SystemName = permissionDto.SystemName;
            existingPermission.Description = permissionDto.Description;
            existingPermission.Category = permissionDto.Category;
            existingPermission.Action = permissionDto.Action;
            existingPermission.Resource = permissionDto.Resource;

            await _permissionRepository.UpdateAsync(existingPermission);
            return NoContent();
        }

        // DELETE: api/permissions/5
        [HttpDelete("{id}")]
        [HasPermission("permissions.assign")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _permissionRepository.GetByIdAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            await _permissionRepository.DeleteAsync(permission);
            return NoContent();
        }
    }

    public class PermissionCreateDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SystemName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Resource { get; set; } = string.Empty;
    }

    public class PermissionUpdateDto
    {
        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string SystemName { get; set; } = string.Empty;

        [StringLength(255)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Category { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string Resource { get; set; } = string.Empty;
    }
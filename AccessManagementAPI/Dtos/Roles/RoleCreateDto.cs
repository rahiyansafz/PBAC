using System.ComponentModel.DataAnnotations;

namespace AccessManagementAPI.Dtos.Roles;

public class RoleCreateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string SystemName { get; set; } = string.Empty;

    [StringLength(255)] public string Description { get; set; } = string.Empty;
}
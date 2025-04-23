using System.ComponentModel.DataAnnotations;

namespace AccessManagementAPI.Dtos.Permissions;

public class PermissionUpdateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string SystemName { get; set; } = string.Empty;

    [StringLength(255)] public string Description { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string Category { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string Action { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string Resource { get; set; } = string.Empty;
}
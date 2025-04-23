using System.ComponentModel.DataAnnotations;

namespace AccessManagementAPI.Dtos.MenuItems;

public class MenuItemCreateDto
{
    [Required] [StringLength(50)] public string Name { get; set; } = string.Empty;

    [Required] [StringLength(50)] public string DisplayName { get; set; } = string.Empty;

    [Required] [StringLength(255)] public string Url { get; set; } = string.Empty;

    [StringLength(50)] public string Icon { get; set; } = string.Empty;

    public int ParentId { get; set; } = 0;

    public int DisplayOrder { get; set; } = 0;

    public bool IsVisible { get; set; } = true;

    [StringLength(50)] public string RequiredPermissionSystemName { get; set; } = string.Empty;
}
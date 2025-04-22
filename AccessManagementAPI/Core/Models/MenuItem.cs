namespace AccessManagementAPI.Core.Models;

public class MenuItem
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Icon { get; set; } = string.Empty;
    public int ParentId { get; set; } // 0 for top-level items
    public int DisplayOrder { get; set; }
    public bool IsVisible { get; set; } = true;

    public string RequiredPermissionSystemName { get; set; } =
        string.Empty; // Permission required to see this menu item
}
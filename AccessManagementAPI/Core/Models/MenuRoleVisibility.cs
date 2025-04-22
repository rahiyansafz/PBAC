namespace AccessManagementAPI.Core.Models;

public class MenuRoleVisibility
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public int RoleId { get; set; }
    public bool IsVisible { get; set; }

    // Navigation properties
    public virtual MenuItem MenuItem { get; set; } = null!;
    public virtual Role Role { get; set; } = null!;
}
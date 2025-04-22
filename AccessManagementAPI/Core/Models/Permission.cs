namespace AccessManagementAPI.Core.Models;

public class Permission
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string SystemName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // e.g., "Users", "Results", "Courses"
    public string Action { get; set; } = string.Empty; // e.g., "Create", "Read", "Update", "Delete"
    public string Resource { get; set; } = string.Empty; // e.g., "User", "Result", "Course"

    // Navigation properties
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
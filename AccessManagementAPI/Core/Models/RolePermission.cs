﻿namespace AccessManagementAPI.Core.Models;

public class RolePermission
{
    public int Id { get; set; }
    public int RoleId { get; set; }
    public int PermissionId { get; set; }

    // Navigation properties
    public virtual Role Role { get; set; } = null!;
    public virtual Permission Permission { get; set; } = null!;
}
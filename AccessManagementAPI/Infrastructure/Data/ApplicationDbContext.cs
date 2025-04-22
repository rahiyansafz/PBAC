using AccessManagementAPI.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace AccessManagementAPI.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;
    public DbSet<Permission> Permissions { get; set; } = null!;
    public DbSet<RolePermission> RolePermissions { get; set; } = null!;
    public DbSet<UserRole> UserRoles { get; set; } = null!;
    public DbSet<MenuItem> MenuItems { get; set; } = null!;
    public DbSet<MenuRoleVisibility> MenuRoleVisibilities { get; set; } = null!;
    public DbSet<UserClaim> UserClaims { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure relationships
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId);

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.User)
            .WithMany(u => u.UserRoles)
            .HasForeignKey(ur => ur.UserId);

        modelBuilder.Entity<UserRole>()
            .HasOne(ur => ur.Role)
            .WithMany(r => r.UserRoles)
            .HasForeignKey(ur => ur.RoleId);

        modelBuilder.Entity<MenuRoleVisibility>()
            .HasOne(mrv => mrv.MenuItem)
            .WithMany()
            .HasForeignKey(mrv => mrv.MenuItemId);

        modelBuilder.Entity<MenuRoleVisibility>()
            .HasOne(mrv => mrv.Role)
            .WithMany()
            .HasForeignKey(mrv => mrv.RoleId);

        modelBuilder.Entity<UserClaim>()
            .HasOne(uc => uc.User)
            .WithMany()
            .HasForeignKey(uc => uc.UserId);

        // Seed data for default admin role and permissions
        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Create default Admin role
        modelBuilder.Entity<Role>().HasData(new Role
        {
            Id = 1,
            Name = "Administrator",
            SystemName = "Administrator",
            Description = "System administrator with full access to all features",
            IsSystemRole = true
        });

        // Create default Student role
        modelBuilder.Entity<Role>().HasData(new Role
        {
            Id = 2,
            Name = "Student",
            SystemName = "Student",
            Description = "Student role with limited permissions",
            IsSystemRole = true
        });

        // Create basic permissions
        var permissions = new List<Permission>
        {
            // User management permissions
            new Permission
            {
                Id = 1, Name = "View Users", SystemName = "users.view", Description = "Permission to view users",
                Category = "Users", Action = "Read", Resource = "User"
            },
            new Permission
            {
                Id = 2, Name = "Create Users", SystemName = "users.create", Description = "Permission to create users",
                Category = "Users", Action = "Create", Resource = "User"
            },
            new Permission
            {
                Id = 3, Name = "Edit Users", SystemName = "users.edit", Description = "Permission to edit users",
                Category = "Users", Action = "Update", Resource = "User"
            },
            new Permission
            {
                Id = 4, Name = "Delete Users", SystemName = "users.delete", Description = "Permission to delete users",
                Category = "Users", Action = "Delete", Resource = "User"
            },

            // Role management permissions
            new Permission
            {
                Id = 5, Name = "View Roles", SystemName = "roles.view", Description = "Permission to view roles",
                Category = "Roles", Action = "Read", Resource = "Role"
            },
            new Permission
            {
                Id = 6, Name = "Create Roles", SystemName = "roles.create", Description = "Permission to create roles",
                Category = "Roles", Action = "Create", Resource = "Role"
            },
            new Permission
            {
                Id = 7, Name = "Edit Roles", SystemName = "roles.edit", Description = "Permission to edit roles",
                Category = "Roles", Action = "Update", Resource = "Role"
            },
            new Permission
            {
                Id = 8, Name = "Delete Roles", SystemName = "roles.delete", Description = "Permission to delete roles",
                Category = "Roles", Action = "Delete", Resource = "Role"
            },

            // Permission management permissions
            new Permission
            {
                Id = 9, Name = "View Permissions", SystemName = "permissions.view",
                Description = "Permission to view permissions", Category = "Permissions", Action = "Read",
                Resource = "Permission"
            },
            new Permission
            {
                Id = 10, Name = "Assign Permissions", SystemName = "permissions.assign",
                Description = "Permission to assign permissions to roles", Category = "Permissions", Action = "Update",
                Resource = "Permission"
            },

            // Result management permissions
            new Permission
            {
                Id = 11, Name = "View Results", SystemName = "results.view", Description = "Permission to view results",
                Category = "Results", Action = "Read", Resource = "Result"
            },
            new Permission
            {
                Id = 12, Name = "Create Results", SystemName = "results.create",
                Description = "Permission to create results", Category = "Results", Action = "Create",
                Resource = "Result"
            },
            new Permission
            {
                Id = 13, Name = "Edit Results", SystemName = "results.edit", Description = "Permission to edit results",
                Category = "Results", Action = "Update", Resource = "Result"
            },
            new Permission
            {
                Id = 14, Name = "Delete Results", SystemName = "results.delete",
                Description = "Permission to delete results", Category = "Results", Action = "Delete",
                Resource = "Result"
            },

            // Menu management permissions
            new Permission
            {
                Id = 15, Name = "Manage Menus", SystemName = "menus.manage",
                Description = "Permission to manage menu items", Category = "System", Action = "Update",
                Resource = "Menu"
            }
        };

        modelBuilder.Entity<Permission>().HasData(permissions);

        // Assign all permissions to Admin role
        var adminRolePermissions = new List<RolePermission>();
        for (int i = 0; i < permissions.Count; i++)
        {
            adminRolePermissions.Add(new RolePermission
            {
                Id = i + 1,
                RoleId = 1, // Admin role
                PermissionId = i + 1
            });
        }

        modelBuilder.Entity<RolePermission>().HasData(adminRolePermissions);

        // Assign specific permissions to Student role
        modelBuilder.Entity<RolePermission>().HasData(
            new RolePermission
                { Id = adminRolePermissions.Count + 1, RoleId = 2, PermissionId = 11 } // View Results permission
        );

        // Create default menu items
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem
            {
                Id = 1, Name = "Dashboard", DisplayName = "Dashboard", Url = "/dashboard", Icon = "home", ParentId = 0,
                DisplayOrder = 1, IsVisible = true, RequiredPermissionSystemName = ""
            },
            new MenuItem
            {
                Id = 2, Name = "UserManagement", DisplayName = "User Management", Url = "#", Icon = "users",
                ParentId = 0, DisplayOrder = 2, IsVisible = true, RequiredPermissionSystemName = "users.view"
            },
            new MenuItem
            {
                Id = 3, Name = "Users", DisplayName = "Users", Url = "/users", Icon = "user", ParentId = 2,
                DisplayOrder = 1, IsVisible = true, RequiredPermissionSystemName = "users.view"
            },
            new MenuItem
            {
                Id = 4, Name = "Roles", DisplayName = "Roles", Url = "/roles", Icon = "shield", ParentId = 2,
                DisplayOrder = 2, IsVisible = true, RequiredPermissionSystemName = "roles.view"
            },
            new MenuItem
            {
                Id = 5, Name = "Permissions", DisplayName = "Permissions", Url = "/permissions", Icon = "key",
                ParentId = 2, DisplayOrder = 3, IsVisible = true, RequiredPermissionSystemName = "permissions.view"
            },
            new MenuItem
            {
                Id = 6, Name = "MenuManagement", DisplayName = "Menu Management", Url = "/menus", Icon = "list",
                ParentId = 0, DisplayOrder = 3, IsVisible = true, RequiredPermissionSystemName = "menus.manage"
            },
            new MenuItem
            {
                Id = 7, Name = "Results", DisplayName = "Results", Url = "/results", Icon = "file-text", ParentId = 0,
                DisplayOrder = 4, IsVisible = true, RequiredPermissionSystemName = "results.view"
            }
        );

        // Create admin user
        modelBuilder.Entity<User>().HasData(
            new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@example.com",
                // BCrypt hash for "Admin@123"
                PasswordHash = "$2a$11$ysX3ykS8fYQfDmPFKPYj4eQCsJJXBgT3UfQoDQtfF.1c1HnCyjXwm",
                IsActive = true,
                EmailConfirmed = true, // Mark email as confirmed for admin user
                EmailVerificationToken = null,
                EmailVerificationTokenExpiry = null,
                PasswordResetToken = null,
                PasswordResetTokenExpiry = null,
                RefreshToken = null,
                RefreshTokenExpiryTime = null
            }
        );

        // Assign admin role to admin user
        modelBuilder.Entity<UserRole>().HasData(
            new UserRole { Id = 1, UserId = 1, RoleId = 1 }
        );
    }
}
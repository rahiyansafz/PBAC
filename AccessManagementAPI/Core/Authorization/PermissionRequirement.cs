using Microsoft.AspNetCore.Authorization;

namespace AccessManagementAPI.Core.Authorization;

public class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
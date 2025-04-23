using Microsoft.AspNetCore.Authorization;

namespace AccessManagementAPI.Core.Authorization;

public class HasPermissionAttribute(string permission) : AuthorizeAttribute(PERMISSION_POLICY_PREFIX + permission)
{
    private const string PERMISSION_POLICY_PREFIX = "Permission";
}
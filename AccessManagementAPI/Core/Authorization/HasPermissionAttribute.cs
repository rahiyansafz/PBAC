using Microsoft.AspNetCore.Authorization;

namespace AccessManagementAPI.Core.Authorization;

public class HasPermissionAttribute : AuthorizeAttribute
{
    private const string PERMISSION_POLICY_PREFIX = "Permission";

    public HasPermissionAttribute(string permission) 
        : base(PERMISSION_POLICY_PREFIX + permission)
    {
    }
}
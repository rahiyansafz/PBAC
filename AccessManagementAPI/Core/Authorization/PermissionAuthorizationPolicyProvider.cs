using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace AccessManagementAPI.Core.Authorization;

public class PermissionAuthorizationPolicyProvider : DefaultAuthorizationPolicyProvider
{
    private const string PERMISSION_POLICY_PREFIX = "Permission";
        
    public PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options) 
        : base(options)
    {
    }

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // If the policy name doesn't start with the prefix, use the default provider
        if (!policyName.StartsWith(PERMISSION_POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            return await base.GetPolicyAsync(policyName);
        }

        // Extract the permission name from the policy name
        var permissionName = policyName.Substring(PERMISSION_POLICY_PREFIX.Length);
            
        // Create a policy with the permission requirement
        var policy = new AuthorizationPolicyBuilder()
            .AddRequirements(new PermissionRequirement(permissionName))
            .Build();

        return policy;
    }
}
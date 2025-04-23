using System.Security.Claims;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace AccessManagementAPI.Core.Authorization;

public class PermissionAuthorizationHandler(
    IPermissionService permissionService,
    IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!context.User.Identity!.IsAuthenticated) return;

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null) return;

        if (int.TryParse(userIdClaim.Value, out var userId))
            if (await permissionService.HasPermissionAsync(userId, requirement.Permission))
                context.Succeed(requirement);
    }
}
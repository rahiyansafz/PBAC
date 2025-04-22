using System.Security.Claims;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace AccessManagementAPI.Core.Authorization;

public class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IPermissionService _permissionService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public PermissionAuthorizationHandler(
        IPermissionService permissionService,
        IHttpContextAccessor httpContextAccessor)
    {
        _permissionService = permissionService;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User == null || !context.User.Identity!.IsAuthenticated)
        {
            return;
        }

        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null)
        {
            return;
        }

        if (int.TryParse(userIdClaim.Value, out int userId))
        {
            if (await _permissionService.HasPermissionAsync(userId, requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
    }
}
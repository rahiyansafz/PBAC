using AccessManagementAPI.Core.Authorization;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;

namespace AccessManagementAPI.Core.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamicPermissions(this IServiceCollection services)
    {
        // Register services
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddScoped<IRoleService, RoleService>();
        services.AddScoped<IEmailService, EmailService>();

        // Register authorization handler
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

        // Register authorization policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }
}
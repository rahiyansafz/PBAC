using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Services;
using AccessManagementAPI.Dtos.UserInfo;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserInfoController(
    IUserRepository userRepository,
    IPermissionService permissionService,
    IConfiguration configuration)
    : ControllerBase
{
    /// <summary>
    ///     Validates the token and returns user information if token is valid
    /// </summary>
    /// <returns>User information including permissions</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
    {
        // If we reach this point, the token is valid (Authorization attribute ensures this)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
            return Unauthorized("Invalid token");

        var user = await userRepository.GetUserWithRolesAsync(userId);
        if (user == null) return NotFound("User not found");

        // Get user permissions
        var permissions = await permissionService.GetUserPermissionNamesAsync(userId);

        // Get user roles
        var roles = user.UserRoles.Select(ur => ur.Role.SystemName).ToList();

        return Ok(new UserInfoResponse
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            IsActive = user.IsActive,
            EmailConfirmed = user.EmailConfirmed,
            Roles = roles,
            Permissions = permissions.ToList()
        });
    }

    /// <summary>
    ///     Validates a token without requiring it in the Authorization header
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>User information if token is valid</returns>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<ActionResult<UserInfoResponse>> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token)) return BadRequest("Token is required");

        try
        {
            // Manually validate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? string.Empty);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidAudience = configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            // If token is invalid, this will throw an exception
            var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out var securityToken);

            // Get user ID from the token
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId))
                return Unauthorized("Invalid token");

            var user = await userRepository.GetUserWithRolesAsync(userId);
            if (user == null) return NotFound("User not found");

            // Get user permissions
            var permissions = await permissionService.GetUserPermissionNamesAsync(userId);

            // Get user roles
            var roles = user.UserRoles.Select(ur => ur.Role.SystemName).ToList();

            return Ok(new UserInfoResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                IsActive = user.IsActive,
                EmailConfirmed = user.EmailConfirmed,
                Roles = roles,
                Permissions = permissions.ToList()
            });
        }
        catch
        {
            return Unauthorized("Invalid token");
        }
    }
}
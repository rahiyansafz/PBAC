using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserInfoController : ControllerBase
{
    private readonly IUserRepository _userRepository;
    private readonly IPermissionService _permissionService;
    private readonly IConfiguration _configuration;

    public UserInfoController(
        IUserRepository userRepository,
        IPermissionService permissionService,
        IConfiguration configuration)
    {
        _userRepository = userRepository;
        _permissionService = permissionService;
        _configuration = configuration;
    }

    /// <summary>
    /// Validates the token and returns user information if token is valid
    /// </summary>
    /// <returns>User information including permissions</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserInfoResponse>> GetUserInfo()
    {
        // If we reach this point, the token is valid (Authorization attribute ensures this)
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
        {
            return Unauthorized("Invalid token");
        }

        var user = await _userRepository.GetUserWithRolesAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        // Get user permissions
        var permissions = await _permissionService.GetUserPermissionNamesAsync(userId);

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
    /// Validates a token without requiring it in the Authorization header
    /// </summary>
    /// <param name="token">The JWT token to validate</param>
    /// <returns>User information if token is valid</returns>
    [HttpPost("validate-token")]
    [AllowAnonymous]
    public async Task<ActionResult<UserInfoResponse>> ValidateToken([FromBody] ValidateTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.Token))
        {
            return BadRequest("Token is required");
        }

        try
        {
            // Manually validate the token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(key)
            };

            // If token is invalid, this will throw an exception
            var principal = tokenHandler.ValidateToken(request.Token, validationParameters, out var securityToken);

            // Get user ID from the token
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized("Invalid token");
            }

            var user = await _userRepository.GetUserWithRolesAsync(userId);
            if (user == null)
            {
                return NotFound("User not found");
            }

            // Get user permissions
            var permissions = await _permissionService.GetUserPermissionNamesAsync(userId);

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

public class UserInfoResponse
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool EmailConfirmed { get; set; }
    public List<string> Roles { get; set; } = new List<string>();
    public List<string> Permissions { get; set; } = new List<string>();
}

public class ValidateTokenRequest
{
    public string Token { get; set; } = string.Empty;
}
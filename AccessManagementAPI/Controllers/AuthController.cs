using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AccessManagementAPI.Core.Interfaces;
using AccessManagementAPI.Core.Models;
using AccessManagementAPI.Core.Models.Auth;
using AccessManagementAPI.Core.Services;
using AccessManagementAPI.Dtos.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace AccessManagementAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    IUserRepository userRepository,
    IRoleRepository roleRepository,
    IEmailService emailService,
    IConfiguration configuration)
    : ControllerBase
{
    // POST: api/auth/register
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDto>> Register([FromBody] RegisterDto registerDto)
    {
        if (await userRepository.ExistsAsync(u => u.Username == registerDto.Username))
            return BadRequest("Username already exists");

        if (await userRepository.ExistsAsync(u => u.Email == registerDto.Email))
            return BadRequest("Email already exists");

        var user = new User
        {
            Username = registerDto.Username,
            Email = registerDto.Email,
            IsActive = true,
            EmailConfirmed = false, // Email not confirmed yet
            EmailVerificationToken = GenerateRandomToken(),
            EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24),
            // Hash the password properly using BCrypt
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(registerDto.Password) // Token valid for 24 hours
        };

        // Save the user
        await userRepository.AddAsync(user);

        // Assign default "Student" role to the user
        var defaultRole = await roleRepository.FindAsync(r => r.SystemName == "Student");
        IEnumerable<Role> enumerable = defaultRole as Role[] ?? defaultRole.ToArray();
        if (enumerable.Any())
        {
            var studentRole = enumerable.First();
            await roleRepository.AddUserToRoleAsync(user.Id, studentRole.Id);
        }

        // Send verification email
        await emailService.SendVerificationEmailAsync(user.Email, user.Id.ToString(), user.EmailVerificationToken);
        return Ok(new UserDto
        {
            Id = user.Id,
            Username = user.Username,
            Email = user.Email,
            Message = "Registration successful. Please check your email to verify your account."
        });
    }

    // GET: api/auth/verify-email
    [HttpGet("verify-email")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyEmail([FromQuery] string userId, [FromQuery] string token)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            return BadRequest("User ID and token are required");

        if (!int.TryParse(userId, out var id)) return BadRequest("Invalid user ID");

        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound("User not found");

        if (user.EmailConfirmed) return Ok("Email already verified");

        if (user.EmailVerificationToken != token) return BadRequest("Invalid verification token");

        if (user.EmailVerificationTokenExpiry < DateTime.UtcNow) return BadRequest("Verification token has expired");

        // Verify the email
        user.EmailConfirmed = true;
        user.EmailVerificationToken = null;
        user.EmailVerificationTokenExpiry = null;
        await userRepository.UpdateAsync(user);
        return Ok("Email verified successfully. You can now log in.");
    }

    // POST: api/auth/resend-verification
    [HttpPost("resend-verification")]
    [AllowAnonymous]
    public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationDto request)
    {
        var user = await userRepository.FindAsync(u => u.Email == request.Email);
        IEnumerable<User> enumerable = user as User[] ?? user.ToArray();
        if (!enumerable.Any())
            // Don't reveal that the email doesn't exist
            return Ok("If your email exists in our system, a verification email has been sent.");

        var currentUser = enumerable.First();
        if (currentUser.EmailConfirmed) return BadRequest("Email already verified");

        // Generate new verification token
        currentUser.EmailVerificationToken = GenerateRandomToken();
        currentUser.EmailVerificationTokenExpiry = DateTime.UtcNow.AddHours(24);
        await userRepository.UpdateAsync(currentUser);

        // Send verification email
        await emailService.SendVerificationEmailAsync(currentUser.Email, currentUser.Id.ToString(),
            currentUser.EmailVerificationToken);
        return Ok("Verification email has been sent. Please check your inbox.");
    }

    // POST: api/auth/login
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<TokenResponse>> Login([FromBody] LoginDto loginDto)
    {
        var user = await userRepository.GetUserByUsernameAsync(loginDto.Username);
        if (user == null) return Unauthorized("Invalid username or password");

        if (!user.IsActive) return Unauthorized("User account is deactivated");

        // Check if email is verified
        if (!user.EmailConfirmed) return Unauthorized("Please verify your email before logging in");

        // Verify password using BCrypt
        var isValidPassword = BCrypt.Net.BCrypt.Verify(loginDto.Password, user.PasswordHash);
        if (!isValidPassword) return Unauthorized("Invalid username or password");

        // Generate JWT token and refresh token
        var tokenResponse = await GenerateTokens(user);

        // Store refresh token in the database
        user.RefreshToken = tokenResponse.RefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7); // 7 days expiry for refresh token
        await userRepository.UpdateAsync(user);
        return Ok(tokenResponse);
    }

    // POST: api/auth/refresh-token
    [HttpPost("refresh-token")]
    public async Task<ActionResult<TokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        if (string.IsNullOrEmpty(request.RefreshToken)) return BadRequest("Refresh token is required");

        // Find user with this refresh token
        var user = await userRepository.FindAsync(u => u.RefreshToken == request.RefreshToken);
        IEnumerable<User> enumerable = user as User[] ?? user.ToArray();
        if (!enumerable.Any() || enumerable.First().RefreshTokenExpiryTime <= DateTime.UtcNow)
            return Unauthorized("Invalid or expired refresh token");

        var currentUser = enumerable.First();

        // Check if email is verified
        if (!currentUser.EmailConfirmed) return Unauthorized("Please verify your email before using this service");

        // Generate new tokens
        var tokenResponse = await GenerateTokens(currentUser);

        // Update refresh token in the database
        currentUser.RefreshToken = tokenResponse.RefreshToken;
        currentUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await userRepository.UpdateAsync(currentUser);
        return Ok(tokenResponse);
    }

    // POST: api/auth/forgot-password
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.Email)) return BadRequest("Email is required");

        var user = await userRepository.FindAsync(u => u.Email == request.Email);
        IEnumerable<User> enumerable = user as User[] ?? user.ToArray();
        if (!enumerable.Any())
            // Don't reveal that the email doesn't exist
            return Ok("If your email exists in our system, a password reset link has been sent.");

        var currentUser = enumerable.First();

        // Generate password reset token
        currentUser.PasswordResetToken = GenerateRandomToken();
        currentUser.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(24);
        await userRepository.UpdateAsync(currentUser);

        // Send password reset email
        await emailService.SendPasswordResetEmailAsync(currentUser.Email, currentUser.Id.ToString(),
            currentUser.PasswordResetToken);
        return Ok("Password reset link has been sent to your email.");
    }

    // POST: api/auth/reset-password
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrEmpty(request.UserId) || string.IsNullOrEmpty(request.Token) ||
            string.IsNullOrEmpty(request.NewPassword) || string.IsNullOrEmpty(request.ConfirmPassword))
            return BadRequest("All fields are required");

        if (request.NewPassword != request.ConfirmPassword) return BadRequest("Passwords do not match");

        if (!int.TryParse(request.UserId, out var id)) return BadRequest("Invalid user ID");

        var user = await userRepository.GetByIdAsync(id);
        if (user == null) return NotFound("User not found");

        if (user.PasswordResetToken != request.Token) return BadRequest("Invalid reset token");

        if (user.PasswordResetTokenExpiry < DateTime.UtcNow) return BadRequest("Reset token has expired");

        // Reset the password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        await userRepository.UpdateAsync(user);
        return Ok("Password has been reset successfully. You can now log in with your new password.");
    }

    // POST: api/auth/change-password
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound("User not found");

        // Verify current password
        var isValidPassword = BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash);
        if (!isValidPassword) return BadRequest("Current password is incorrect");

        if (request.NewPassword != request.ConfirmPassword) return BadRequest("New passwords do not match");

        // Change the password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        await userRepository.UpdateAsync(user);
        return Ok("Password changed successfully");
    }

    // POST: api/auth/revoke-token
    [HttpPost("revoke-token")]
    [Authorize]
    public async Task<IActionResult> RevokeToken()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out var userId)) return Unauthorized();

        var user = await userRepository.GetByIdAsync(userId);
        if (user == null) return NotFound();

        // Revoke refresh token
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        await userRepository.UpdateAsync(user);
        return NoContent();
    }

    private async Task<TokenResponse> GenerateTokens(User user)
    {
        var userRoles = await userRepository.GetUserWithRolesAsync(user.Id);
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username),
            new(ClaimTypes.Email, user.Email)
        };

        // Add role claims
        if (userRoles != null)
            foreach (var userRole in userRoles.UserRoles)
                claims.Add(new Claim(ClaimTypes.Role, userRole.Role.SystemName));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:Key"] ?? "fallbackKeyForDevelopment12345678901234"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        // Token expires in 15 minutes
        var expiration = DateTime.UtcNow.AddMinutes(15);
        var token = new JwtSecurityToken(configuration["Jwt:Issuer"], configuration["Jwt:Audience"],
            claims, expires: expiration, signingCredentials: creds);

        // Generate refresh token
        var refreshToken = GenerateRandomToken();
        return new TokenResponse
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            RefreshToken = refreshToken,
            Expiration = expiration
        };
    }

    private string GenerateRandomToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
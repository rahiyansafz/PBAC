using System.ComponentModel.DataAnnotations;

namespace AccessManagementAPI.Dtos.Auth;

public class ResendVerificationDto
{
    [Required] [EmailAddress] public string Email { get; set; } = string.Empty;
}
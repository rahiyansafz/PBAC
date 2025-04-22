namespace AccessManagementAPI.Core.Models;

public class UserClaim
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string ClaimType { get; set; } = string.Empty;
    public string ClaimValue { get; set; } = string.Empty;

    // Navigation properties
    public virtual User User { get; set; } = null!;
}
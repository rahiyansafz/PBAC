namespace AccessManagementAPI.Dtos.Auth;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Token { get; set; }
    public string? Message { get; set; }
}
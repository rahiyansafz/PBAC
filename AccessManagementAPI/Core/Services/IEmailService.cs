namespace AccessManagementAPI.Core.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlMessage);
    Task SendVerificationEmailAsync(string to, string userId, string token);
    Task SendPasswordResetEmailAsync(string to, string userId, string token);
}
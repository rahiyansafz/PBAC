using System.Net;
using System.Net.Mail;

namespace AccessManagementAPI.Core.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly string _fromEmail;
    private readonly string _smtpServer;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _baseUrl;

    public EmailService(IConfiguration configuration)
    {
        _configuration = configuration;
        _fromEmail = _configuration["Email:From"];
        _smtpServer = _configuration["Email:SmtpServer"];
        _smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
        _smtpUsername = _configuration["Email:Username"];
        _smtpPassword = _configuration["Email:Password"];
        _baseUrl = _configuration["Application:BaseUrl"];
    }

    public async Task SendEmailAsync(string to, string subject, string htmlMessage)
    {
        var client = new SmtpClient(_smtpServer, _smtpPort)
        {
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = true
        };

        var message = new MailMessage
        {
            From = new MailAddress(_fromEmail),
            Subject = subject,
            Body = htmlMessage,
            IsBodyHtml = true
        };

        message.To.Add(new MailAddress(to));

        await client.SendMailAsync(message);
    }

    public async Task SendVerificationEmailAsync(string to, string userId, string token)
    {
        var verificationLink = $"{_baseUrl}/api/auth/verify-email?userId={userId}&token={WebUtility.UrlEncode(token)}";

        var subject = "Verify your email address";
        var htmlMessage = $@"
                <h1>Email Verification</h1>
                <p>Thank you for registering. Please verify your email address by clicking the link below:</p>
                <p><a href='{verificationLink}'>Verify Email</a></p>
                <p>If you didn't register on our site, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
            ";

        await SendEmailAsync(to, subject, htmlMessage);
    }

    public async Task SendPasswordResetEmailAsync(string to, string userId, string token)
    {
        var resetLink = $"{_baseUrl}/reset-password?userId={userId}&token={WebUtility.UrlEncode(token)}";

        var subject = "Reset your password";
        var htmlMessage = $@"
                <h1>Password Reset</h1>
                <p>You requested a password reset. Please click the link below to reset your password:</p>
                <p><a href='{resetLink}'>Reset Password</a></p>
                <p>If you didn't request a password reset, please ignore this email.</p>
                <p>This link will expire in 24 hours.</p>
            ";

        await SendEmailAsync(to, subject, htmlMessage);
    }
}
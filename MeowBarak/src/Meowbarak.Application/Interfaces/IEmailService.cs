namespace Meowbarak.Application.Interfaces;

/// <summary>
/// سرویس ارسال ایمیل - قابل جایگزینی با هر Provider (SMTP, SendGrid و ...)
/// </summary>
public interface IEmailService
{
    Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default);
}

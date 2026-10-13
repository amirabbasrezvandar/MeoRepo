using MailKit.Net.Smtp;
using MailKit.Security;
using Meowbarak.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace Meowbarak.Infrastructure.Services;

/// <summary>
/// سرویس ارسال ایمیل واقعی با MailKit (پایدار و سازگار با Gmail)
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly IHostEnvironment _env;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, IHostEnvironment env, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _env = env;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"];
        var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
        var smtpUser = _configuration["Email:SmtpUser"];
        var smtpPass = _configuration["Email:SmtpPassword"]?.Replace(" ", ""); // فاصله‌ها را حذف می‌کند
        var fromEmail = _configuration["Email:FromEmail"] ?? smtpUser;
        var fromName = _configuration["Email:FromName"] ?? "Meowbarak";

        // اگر SMTP تنظیم نشده بود → فقط لاگ کن
        if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
        {
            _logger.LogWarning("========== EMAIL (SMTP NOT CONFIGURED - LOGGING ONLY) ==========");
            _logger.LogWarning("To     : {To}", toEmail);
            _logger.LogWarning("Subject: {Subject}", subject);
            _logger.LogWarning("Body   : {Body}", body);
            _logger.LogWarning("برای ارسال واقعی ایمیل، تنظیمات Email را در appsettings.json پر کنید.");
            _logger.LogWarning("================================================================");
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                HtmlBody = body
            };
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            // اتصال امن به Gmail
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(smtpUser, smtpPass, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Email sent successfully to {To}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", toEmail);
            throw new InvalidOperationException($"ارسال ایمیل با خطا مواجه شد: {ex.Message}", ex);
        }
    }
}
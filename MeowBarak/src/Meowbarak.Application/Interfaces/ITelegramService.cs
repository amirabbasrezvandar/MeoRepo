namespace Meowbarak.Application.Interfaces;

/// <summary>
/// سرویس ارسال پیام و بررسی کاربر از طریق Userbot تلگرام
/// </summary>
public interface ITelegramService
{
    /// <summary>
    /// بررسی وجود کاربر با آیدی تلگرام
    /// </summary>
    Task<(bool Exists, string Message, object? Data)> CheckUserExistsAsync(long telegramId, CancellationToken cancellationToken = default);

    /// <summary>
    /// ارسال پیام متنی به یک آیدی تلگرام
    /// </summary>
    Task<(bool Success, string Message)> SendMessageAsync(long telegramId, string text, CancellationToken cancellationToken = default);
}

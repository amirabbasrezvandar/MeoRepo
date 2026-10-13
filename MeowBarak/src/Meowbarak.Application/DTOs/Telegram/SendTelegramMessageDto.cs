namespace Meowbarak.Application.DTOs.Telegram;

/// <summary>
/// درخواست ارسال پیام به کاربر تلگرام
/// </summary>
public class SendTelegramMessageDto
{
    public long TelegramId { get; set; }
    public string Message { get; set; } = string.Empty;
}

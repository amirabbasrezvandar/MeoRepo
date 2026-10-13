namespace Meowbarak.Domain.Entities;

/// <summary>
/// کد یکبار مصرف برای تأیید ایمیل یا سایر عملیات‌های آینده
/// </summary>
public class VerificationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    /// <summary>
    /// کد ۶ رقمی یکبار مصرف
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// نوع کد (Register, ResetPassword و ...)
    /// </summary>
    public string Purpose { get; set; } = "Register";

    /// <summary>
    /// زمان انقضا (UTC)
    /// </summary>
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// آیا کد استفاده شده است؟
    /// </summary>
    public bool IsUsed { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

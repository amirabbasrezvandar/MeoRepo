namespace Meowbarak.Domain.Entities;

/// <summary>
/// موجودیت کاربر - فقط اطلاعات احراز هویت
/// قابل توسعه برای پروفایل، نقش‌ها و ... در آینده
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// ایمیل کاربر (یکتا)
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// هش رمز عبور (BCrypt)
    /// </summary>
    public string? PasswordHash { get; set; }

    /// <summary>
    /// آیا ایمیل تأیید شده است؟
    /// </summary>
    public bool IsEmailVerified { get; set; }

    /// <summary>
    /// آیا کاربر فعال است؟
    /// </summary>
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<VerificationCode> VerificationCodes { get; set; } = new List<VerificationCode>();
}

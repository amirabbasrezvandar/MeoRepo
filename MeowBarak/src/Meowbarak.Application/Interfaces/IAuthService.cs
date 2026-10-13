using Meowbarak.Application.DTOs.Auth;

namespace Meowbarak.Application.Interfaces;

/// <summary>
/// سرویس اصلی احراز هویت
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// درخواست ارسال کد OTP به ایمیل
    /// </summary>
    Task<AuthResponseDto> RequestOtpAsync(RequestOtpDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// تأیید کد OTP
    /// </summary>
    Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// تنظیم رمز عبور پس از تأیید OTP (ثبت‌نام کامل)
    /// </summary>
    Task<AuthResponseDto> SetPasswordAsync(SetPasswordDto request, CancellationToken cancellationToken = default);

    /// <summary>
    /// ورود با ایمیل و رمز عبور
    /// </summary>
    Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default);
}

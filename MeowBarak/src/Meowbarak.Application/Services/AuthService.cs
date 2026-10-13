using FluentValidation;
using Meowbarak.Application.Common;
using Meowbarak.Application.DTOs.Auth;
using Meowbarak.Application.Interfaces;
using Meowbarak.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Meowbarak.Application.Services;

/// <summary>
/// پیاده‌سازی سرویس احراز هویت
/// منطق کسب‌وکار اینجا قرار دارد تا Infrastructure فقط پیاده‌سازی فنی باشد
/// </summary>
public class AuthService : IAuthService
{
    private readonly IApplicationDbContext _db;
    private readonly IEmailService _emailService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<AuthService> _logger;
    private readonly IValidator<RequestOtpDto> _requestOtpValidator;
    private readonly IValidator<VerifyOtpDto> _verifyOtpValidator;
    private readonly IValidator<SetPasswordDto> _setPasswordValidator;
    private readonly IValidator<LoginDto> _loginValidator;

    // مدت اعتبار OTP به دقیقه
    private const int OtpExpiryMinutes = 5;

    public AuthService(
        IApplicationDbContext db,
        IEmailService emailService,
        IJwtService jwtService,
        ILogger<AuthService> logger,
        IValidator<RequestOtpDto> requestOtpValidator,
        IValidator<VerifyOtpDto> verifyOtpValidator,
        IValidator<SetPasswordDto> setPasswordValidator,
        IValidator<LoginDto> loginValidator)
    {
        _db = db;
        _emailService = emailService;
        _jwtService = jwtService;
        _logger = logger;
        _requestOtpValidator = requestOtpValidator;
        _verifyOtpValidator = verifyOtpValidator;
        _setPasswordValidator = setPasswordValidator;
        _loginValidator = loginValidator;
    }

    public async Task<AuthResponseDto> RequestOtpAsync(RequestOtpDto request, CancellationToken cancellationToken = default)
    {
        await _requestOtpValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();

        // پیدا کردن یا ساختن کاربر
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Email = email,
                IsEmailVerified = false
            };
            _db.Users.Add(user);
            await _db.SaveChangesAsync(cancellationToken);
        }
        else if (user.PasswordHash is not null && user.IsEmailVerified)
        {
            // کاربر قبلاً ثبت‌نام کامل کرده
            throw new ApiException("این ایمیل قبلاً ثبت‌نام شده است. لطفاً وارد شوید.", 400);
        }

        // باطل کردن کدهای قبلی استفاده نشده
        var oldCodes = await _db.VerificationCodes
            .Where(v => v.UserId == user.Id && !v.IsUsed && v.Purpose == "Register")
            .ToListAsync(cancellationToken);

        foreach (var old in oldCodes)
            old.IsUsed = true;

        // تولید کد ۶ رقمی
        var code = Random.Shared.Next(100000, 999999).ToString();

        var verification = new VerificationCode
        {
            UserId = user.Id,
            Code = code,
            Purpose = "Register",
            ExpiresAt = DateTime.UtcNow.AddMinutes(OtpExpiryMinutes),
            IsUsed = false
        };

        _db.VerificationCodes.Add(verification);
        await _db.SaveChangesAsync(cancellationToken);

        // ارسال ایمیل
        var subject = "کد تأیید Meowbarak";
        var body = $@"
            <div style='font-family:Tahoma,Arial;direction:rtl;text-align:right;'>
                <h2>کد تأیید شما</h2>
                <p>کد یکبار مصرف شما: <strong style='font-size:24px;letter-spacing:4px;'>{code}</strong></p>
                <p>این کد تا {OtpExpiryMinutes} دقیقه معتبر است.</p>
                <p>اگر شما این درخواست را نداده‌اید، این ایمیل را نادیده بگیرید.</p>
            </div>";

        await _emailService.SendEmailAsync(email, subject, body, cancellationToken);

        _logger.LogInformation("OTP sent to {Email}. Code: {Code} (expires in {Minutes} min)", email, code, OtpExpiryMinutes);

        return AuthResponseDto.Ok("کد تأیید به ایمیل شما ارسال شد.", data: new { expiresInMinutes = OtpExpiryMinutes });
    }

    public async Task<AuthResponseDto> VerifyOtpAsync(VerifyOtpDto request, CancellationToken cancellationToken = default)
    {
        await _verifyOtpValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new ApiException("کاربری با این ایمیل یافت نشد.", 404);

        var verification = await _db.VerificationCodes
            .Where(v => v.UserId == user.Id
                        && v.Code == request.Code
                        && v.Purpose == "Register"
                        && !v.IsUsed
                        && v.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ApiException("کد تأیید نامعتبر یا منقضی شده است.", 400);

        // فقط تأیید می‌کنیم که کد درست است. استفاده نهایی در SetPassword انجام می‌شود
        // تا کاربر نتواند بدون تنظیم رمز، کد را مصرف کند.

        return AuthResponseDto.Ok("کد تأیید صحیح است. اکنون می‌توانید رمز عبور خود را تنظیم کنید.");
    }

    public async Task<AuthResponseDto> SetPasswordAsync(SetPasswordDto request, CancellationToken cancellationToken = default)
    {
        await _setPasswordValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new ApiException("کاربری با این ایمیل یافت نشد.", 404);

        if (user.PasswordHash is not null && user.IsEmailVerified)
            throw new ApiException("این کاربر قبلاً ثبت‌نام کامل شده است.", 400);

        var verification = await _db.VerificationCodes
            .Where(v => v.UserId == user.Id
                        && v.Code == request.Code
                        && v.Purpose == "Register"
                        && !v.IsUsed
                        && v.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(v => v.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ApiException("کد تأیید نامعتبر یا منقضی شده است.", 400);

        // علامت‌گذاری کد به عنوان استفاده شده
        verification.IsUsed = true;

        // هش کردن رمز عبور با BCrypt
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
        user.IsEmailVerified = true;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync(cancellationToken);

        // تولید JWT
        var token = _jwtService.GenerateToken(user.Id, user.Email);

        _logger.LogInformation("User {Email} registered successfully", email);

        return AuthResponseDto.Ok("ثبت‌نام با موفقیت انجام شد.", token, new
        {
            userId = user.Id,
            email = user.Email
        });
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto request, CancellationToken cancellationToken = default)
    {
        await _loginValidator.ValidateAndThrowAsync(request, cancellationToken);

        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken)
            ?? throw new ApiException("ایمیل یا رمز عبور اشتباه است.", 401);

        if (!user.IsActive)
            throw new ApiException("حساب کاربری غیرفعال است.", 403);

        if (string.IsNullOrEmpty(user.PasswordHash) || !user.IsEmailVerified)
            throw new ApiException("ثبت‌نام شما کامل نشده است. لطفاً ابتدا کد تأیید و رمز عبور را تنظیم کنید.", 400);

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new ApiException("ایمیل یا رمز عبور اشتباه است.", 401);

        var token = _jwtService.GenerateToken(user.Id, user.Email);

        _logger.LogInformation("User {Email} logged in", email);

        return AuthResponseDto.Ok("ورود موفقیت‌آمیز بود.", token, new
        {
            userId = user.Id,
            email = user.Email
        });
    }
}

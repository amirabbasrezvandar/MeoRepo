using Meowbarak.Application.DTOs.Auth;
using Meowbarak.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Meowbarak.API.Controllers;

/// <summary>
/// کنترلر احراز هویت - ثبت‌نام با OTP و ورود با JWT
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// درخواست ارسال کد یکبار مصرف به ایمیل
    /// </summary>
    /// <remarks>
    /// نمونه درخواست:
    /// 
    ///     POST /api/auth/request-otp
    ///     {
    ///         "email": "user@example.com"
    ///     }
    /// 
    /// در حالت Development کد OTP در لاگ کنسول چاپ می‌شود.
    /// </remarks>
    [HttpPost("request-otp")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RequestOtp([FromBody] RequestOtpDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.RequestOtpAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// تأیید کد یکبار مصرف
    /// </summary>
    [HttpPost("verify-otp")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.VerifyOtpAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// تنظیم رمز عبور و تکمیل ثبت‌نام (پس از تأیید OTP)
    /// </summary>
    /// <remarks>
    /// پس از موفقیت، JWT Token برمی‌گرداند.
    /// </remarks>
    [HttpPost("set-password")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> SetPassword([FromBody] SetPasswordDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.SetPasswordAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// ورود با ایمیل و رمز عبور
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto request, CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return Ok(result);
    }
}

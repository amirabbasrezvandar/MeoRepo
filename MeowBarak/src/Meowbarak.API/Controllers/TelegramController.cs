using FluentValidation;
using Meowbarak.Application.DTOs.Auth;
using Meowbarak.Application.DTOs.Telegram;
using Meowbarak.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meowbarak.API.Controllers;

/// <summary>
/// کنترلر مربوط به عملیات تلگرام (Userbot)
/// نیاز به احراز هویت JWT دارد
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramService _telegramService;
    private readonly IValidator<CheckTelegramUserDto> _checkValidator;
    private readonly IValidator<SendTelegramMessageDto> _sendValidator;

    public TelegramController(
        ITelegramService telegramService,
        IValidator<CheckTelegramUserDto> checkValidator,
        IValidator<SendTelegramMessageDto> sendValidator)
    {
        _telegramService = telegramService;
        _checkValidator = checkValidator;
        _sendValidator = sendValidator;
    }

    /// <summary>
    /// بررسی وجود کاربر با آیدی تلگرام
    /// </summary>
    /// <remarks>
    /// نمونه درخواست:
    /// 
    ///     POST /api/telegram/check-user
    ///     {
    ///         "telegramId": 123456789
    ///     }
    /// 
    /// نیاز به JWT دارد (Bearer token).
    /// </remarks>
    [HttpPost("check-user")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CheckUser([FromBody] CheckTelegramUserDto request, CancellationToken cancellationToken)
    {
        var validation = await _checkValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(AuthResponseDto.Fail(string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage))));
        }

        var (exists, message, data) = await _telegramService.CheckUserExistsAsync(request.TelegramId, cancellationToken);

        if (exists)
            return Ok(AuthResponseDto.Ok(message, data: data));

        return Ok(AuthResponseDto.Fail(message));
    }

    /// <summary>
    /// ارسال پیام متنی به یک آیدی تلگرام از طریق Userbot
    /// </summary>
    /// <remarks>
    /// نمونه درخواست:
    /// 
    ///     POST /api/telegram/send-message
    ///     {
    ///         "telegramId": 123456789,
    ///         "message": "سلام! این پیام از MeowBarak است."
    ///     }
    /// 
    /// نیاز به JWT دارد (Bearer token).
    /// </remarks>
    [HttpPost("send-message")]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthResponseDto), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> SendMessage([FromBody] SendTelegramMessageDto request, CancellationToken cancellationToken)
    {
        var validation = await _sendValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
        {
            return BadRequest(AuthResponseDto.Fail(string.Join(" | ", validation.Errors.Select(e => e.ErrorMessage))));
        }

        var (success, message) = await _telegramService.SendMessageAsync(request.TelegramId, request.Message, cancellationToken);

        if (success)
            return Ok(AuthResponseDto.Ok(message));

        return Ok(AuthResponseDto.Fail(message));
    }
}

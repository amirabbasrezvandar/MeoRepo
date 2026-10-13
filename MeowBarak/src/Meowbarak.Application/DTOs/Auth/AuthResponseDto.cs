namespace Meowbarak.Application.DTOs.Auth;

/// <summary>
/// پاسخ استاندارد تمام endpointهای احراز هویت
/// </summary>
public class AuthResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Token { get; set; }
    public object? Data { get; set; }

    public static AuthResponseDto Ok(string message, string? token = null, object? data = null)
        => new() { Success = true, Message = message, Token = token, Data = data };

    public static AuthResponseDto Fail(string message)
        => new() { Success = false, Message = message };
}

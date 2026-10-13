namespace Meowbarak.Application.DTOs.Auth;

public class VerifyOtpDto
{
    public string Email { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
}

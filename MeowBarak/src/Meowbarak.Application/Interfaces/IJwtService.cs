using System.Security.Claims;

namespace Meowbarak.Application.Interfaces;

/// <summary>
/// سرویس تولید و اعتبارسنجی JWT
/// </summary>
public interface IJwtService
{
    string GenerateToken(Guid userId, string email);
    ClaimsPrincipal? ValidateToken(string token);
}

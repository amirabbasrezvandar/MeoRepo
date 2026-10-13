namespace Meowbarak.Application.Common;

/// <summary>
/// Exception سفارشی برای کنترل خطاهای تجاری
/// </summary>
public class ApiException : Exception
{
    public int StatusCode { get; }

    public ApiException(string message, int statusCode = 400) : base(message)
    {
        StatusCode = statusCode;
    }
}

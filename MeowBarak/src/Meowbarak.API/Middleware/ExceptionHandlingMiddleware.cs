using System.Net;
using System.Text.Json;
using FluentValidation;
using Meowbarak.Application.Common;
using Meowbarak.Application.DTOs.Auth;

namespace Meowbarak.API.Middleware;

/// <summary>
/// مدیریت سراسری Exceptionها برای پاسخ یکنواخت API
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = new AuthResponseDto { Success = false };

        switch (exception)
        {
            case ApiException apiEx:
                context.Response.StatusCode = apiEx.StatusCode;
                response.Message = apiEx.Message;
                break;

            case ValidationException validationEx:
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                response.Message = "خطای اعتبارسنجی";
                response.Data = validationEx.Errors.Select(e => new { e.PropertyName, e.ErrorMessage });
                break;

            default:
                _logger.LogError(exception, "Unhandled exception");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                response.Message = "خطای داخلی سرور رخ داد.";
                break;
        }

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

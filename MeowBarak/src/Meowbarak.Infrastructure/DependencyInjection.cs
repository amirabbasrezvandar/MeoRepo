using Meowbarak.Application.Interfaces;
using Meowbarak.Application.Services;
using Meowbarak.Infrastructure.Data;
using Meowbarak.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Meowbarak.Infrastructure;

/// <summary>
/// ثبت سرویس‌های Infrastructure در DI Container
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // DbContext
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        // Services
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAuthService, AuthService>();

        // Telegram Userbot - Singleton because Client should live for the lifetime of the app
        services.AddSingleton<ITelegramService, TelegramService>();

        return services;
    }
}

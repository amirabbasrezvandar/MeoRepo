using Meowbarak.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using TL;
using WTelegram;

namespace Meowbarak.Infrastructure.Services;

/// <summary>
/// پیاده‌سازی سرویس تلگرام با استفاده از Userbot (WTelegramClient)
/// </summary>
public class TelegramService : ITelegramService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private Client? _client;
    private bool _disposed;

    public TelegramService(IConfiguration configuration, ILogger<TelegramService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<Client> GetClientAsync()
    {
        if (_client != null)
            return _client;

        await _lock.WaitAsync();
        try
        {
            if (_client != null)
                return _client;

            var apiId = _configuration["Telegram:ApiId"] ?? throw new InvalidOperationException("Telegram:ApiId is missing in configuration.");
            var apiHash = _configuration["Telegram:ApiHash"] ?? throw new InvalidOperationException("Telegram:ApiHash is missing in configuration.");
            var sessionFileName = _configuration["Telegram:SessionPath"] ?? "WTelegram.session";

            // مسیرهای ممکن برای پیدا کردن فایل Session
            var possiblePaths = new List<string>
            {
                Path.Combine(AppContext.BaseDirectory, sessionFileName),
                Path.Combine(Directory.GetCurrentDirectory(), sessionFileName),
                Path.Combine(Directory.GetCurrentDirectory(), "src", "Meowbarak.API", sessionFileName),
                Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", sessionFileName)), // از bin به پوشه پروژه
                sessionFileName
            };

            string? sessionPath = null;
            foreach (var path in possiblePaths)
            {
                try
                {
                    var full = Path.GetFullPath(path);
                    if (File.Exists(full))
                    {
                        sessionPath = full;
                        break;
                    }
                }
                catch { /* ignore invalid paths */ }
            }

            if (sessionPath == null)
            {
                var searched = string.Join("\n - ", possiblePaths.Select(p =>
                {
                    try { return Path.GetFullPath(p); }
                    catch { return p; }
                }));
                throw new FileNotFoundException(
                    $"فایل Session تلگرام پیدا نشد.\nمسیرهای جستجو شده:\n - {searched}\n\n" +
                    "لطفاً فایل WTelegram.session را داخل پوشه bin\\Debug\\net8.0 کپی کنید.");
            }

            var fileInfo = new FileInfo(sessionPath);
            _logger.LogInformation(
                "Loading Telegram session from: {SessionPath} (Size: {Size} bytes, LastWrite: {LastWrite})",
                sessionPath, fileInfo.Length, fileInfo.LastWriteTime);

            if (fileInfo.Length < 1000)
            {
                throw new InvalidOperationException(
                    $"فایل Session خیلی کوچک است ({fileInfo.Length} بایت). احتمالاً خراب است. دوباره Session بسازید.");
            }

            string? requestedConfig = null;

            _client = new Client(what =>
            {
                switch (what)
                {
                    case "api_id": return apiId;
                    case "api_hash": return apiHash;
                    case "session_pathname": return sessionPath;
                    case "phone_number":
                    case "verification_code":
                    case "password":
                        requestedConfig = what;
                        // برمی‌گردونیم null تا بعداً خطا بدیم (نمی‌ذاریم لاگین تعاملی انجام بشه)
                        return null;
                    default:
                        return null;
                }
            });

            try
            {
                var user = await _client.LoginUserIfNeeded();
                _logger.LogInformation("Telegram Userbot logged in as {FirstName} (ID: {UserId})", user.first_name, user.id);
                return _client;
            }
            catch (Exception ex) when (requestedConfig != null || ex.Message.Contains("phone_number") || ex.Message.Contains("verification_code"))
            {
                _client?.Dispose();
                _client = null;

                throw new InvalidOperationException(
                    $"Session فایل معتبر نیست یا منقضی شده (درخواست {requestedConfig ?? "phone_number"}).\n" +
                    $"مسیر Session: {sessionPath}\n" +
                    $"اندازه فایل: {fileInfo.Length} بایت\n" +
                    "راه حل: دوباره با برنامه Session Generator یک Session جدید بساز و فایل WTelegram.session را جایگزین کن.",
                    ex);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<(bool Exists, string Message, object? Data)> CheckUserExistsAsync(long telegramId, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();

            var inputUser = new InputUser(telegramId, 0);
            var users = await client.Users_GetUsers(inputUser);

            if (users == null || users.Length == 0 || users[0] is not User user)
            {
                return (false, "یوزری به این آیدی وجود ندارد.", null);
            }

            var data = new
            {
                Id = user.id,
                FirstName = user.first_name,
                LastName = user.last_name,
                Username = user.username,
                IsBot = user.flags.HasFlag(User.Flags.bot),
                IsPremium = user.flags.HasFlag(User.Flags.premium)
            };

            return (true, "کاربر پیدا شد.", data);
        }
        catch (RpcException ex) when (ex.Code == 400 || ex.Message.Contains("USER_ID_INVALID") || ex.Message.Contains("PEER_ID_INVALID"))
        {
            _logger.LogWarning(ex, "Telegram user {TelegramId} not found or invalid", telegramId);
            return (false, "یوزری به این آیدی وجود ندارد.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking Telegram user {TelegramId}", telegramId);
            return (false, $"خطا در بررسی کاربر: {ex.Message}", null);
        }
    }

    public async Task<(bool Success, string Message)> SendMessageAsync(long telegramId, string text, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = await GetClientAsync();

            InputPeer peer;

            try
            {
                var inputUser = new InputUser(telegramId, 0);
                var users = await client.Users_GetUsers(inputUser);

                if (users != null && users.Length > 0 && users[0] is User user)
                {
                    peer = new InputPeerUser(user.id, user.access_hash);
                }
                else
                {
                    peer = new InputPeerUser(telegramId, 0);
                }
            }
            catch
            {
                peer = new InputPeerUser(telegramId, 0);
            }

            await client.SendMessageAsync(peer, text);

            _logger.LogInformation("Message sent successfully to Telegram ID {TelegramId}", telegramId);
            return (true, "پیام با موفقیت ارسال شد.");
        }
        catch (RpcException ex) when (
            ex.Message.Contains("USER_ID_INVALID") ||
            ex.Message.Contains("PEER_ID_INVALID") ||
            ex.Message.Contains("USER_PRIVACY_RESTRICTED") ||
            ex.Message.Contains("USER_IS_BLOCKED") ||
            ex.Code == 400)
        {
            _logger.LogWarning(ex, "Cannot send message to Telegram ID {TelegramId}: {Error}", telegramId, ex.Message);
            return (false, "یوزری به این آیدی وجود ندارد یا امکان ارسال پیام به او وجود ندارد.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending message to Telegram ID {TelegramId}", telegramId);
            return (false, $"خطا در ارسال پیام: {ex.Message}");
        }
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _client?.Dispose();
        _lock.Dispose();
    }
}

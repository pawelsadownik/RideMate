using Microsoft.Extensions.Configuration;
using RideMate.Application.Interfaces;
using RideMate.Application.Interfaces;
using System.Net.Http.Json;

namespace RideMate.Infrastructure.Services;

public sealed class NotificationService : IPushNotificationService
{
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;
    private readonly bool _isDevelopment;

    public NotificationService(IHttpClientFactory http, IConfiguration config)
    {
        _http = http;
        _config = config;
        _isDevelopment = config["App__DevMode"] == "true";
    }

    public async Task SendMatchNotificationAsync(Guid toUserId, string fromNickname, CancellationToken ct = default)
    {
        if (_isDevelopment)
        {
            // W trybie deweloperskim tylko logujemy — rzeczywiste push aktywowane po konfiguracji kont Expo/FCM
            System.Diagnostics.Debug.WriteLine($"[DEV PUSH] Użytkownik {toUserId}: nowe dopasowanie z {fromNickname}");
            return;
        }

        await SendAsync(toUserId, "To jest dopasowanie!", $"Ty i {fromNickname} macie się nawzajem.", ct);
    }

    public async Task SendMessageNotificationAsync(Guid toUserId, string fromNickname, string preview, CancellationToken ct = default)
    {
        if (_isDevelopment)
        {
            System.Diagnostics.Debug.WriteLine($"[DEV PUSH] Użytkownik {toUserId}: nowa wiadomość od {fromNickname}: {preview}");
            return;
        }

        await SendAsync(toUserId, $"Nowa wiadomość od {fromNickname}", preview, ct);
    }

    private async Task SendAsync(Guid toUserId, string title, string body, CancellationToken ct)
    {
        // Pobierz tokeny użytkownika z bazy (wymaga access do DbContext — uproszczone: tu tylko logika wysyłki Expo)
        // W pełnej implementacji injected RideMateDbContext i pobieramy PushTokeny.
        var expoUrl = "https://exp.host/--/api/v2/push/send";
        var client = _http.CreateClient();

        var messages = new[]
        {
            new
            {
                to = $"ExponentPushToken[{toUserId}]", // placeholder — w produkcji pobierz z bazy
                title,
                body,
                sound = "default",
                data = new { type = title.Contains("dopasowanie") ? "match" : "message" }
            }
        };

        try
        {
            // Placeholder: realna wysyłka przez Expo Push API / FCM wymaga pobrania tokenów z bazy
            // i konfiguracji kont deweloperskich — aktywowana w produkcji po wdrożeniu.
            await Task.CompletedTask;
        }
        catch
        {
            // Push nie powinien blokować głównego przepływu
        }
    }
}

using System.Net.Http.Json;

namespace AuthApi.Services;

public class LogClient
{
    private readonly HttpClient _http;
    private readonly ILogger<LogClient> _logger;

    public LogClient(HttpClient http, ILogger<LogClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task LogAsync(string level, string message)
    {
        try
        {
            var eintrag = new
            {
                level = level,
                service = "AuthApi",
                message = message
            };

            using var response = await _http.PostAsJsonAsync(
                "api/logs", eintrag);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Logging fehlgeschlagen: HTTP {StatusCode}",
                    (int)response.StatusCode);
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Logging-Service nicht erreichbar.");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogWarning(ex, "Logging-Anfrage abgebrochen.");
        }
    }
}
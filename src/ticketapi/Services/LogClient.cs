namespace ticketapi.Services
{
    public class LogClient
    {
        private readonly HttpClient _http;
        private readonly ILogger<LogClient> _logger;

        public LogClient(HttpClient http, ILogger<LogClient> logger)
        {
            _http = http;
            _logger = logger;
        }

        public async Task LogAsync(string nachricht)
        {
            try
            {
                var eintrag = new { level = "Info", service = "Ticketlog", message = nachricht };
                await _http.PostAsJsonAsync("api/logs", eintrag);
            }
            catch (Exception ex)
            {
                // Logging darf nie dazu führen, dass ein Ticket nicht gespeichert wird
                _logger.LogWarning(ex, "Logging-Service nicht erreichbar.");
            }
        }
    }
}
using logging_service.Models;
using logging_service.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace logging_service.Controllers;

[ApiController]
[Route("api/logs")]
public class LoggingController : ControllerBase
{
    private static readonly string[] AllowedLevels = ["Info", "Warning", "Error"];

    private readonly LogService _logService;
    private readonly ILogger<LoggingController> _logger;

    public LoggingController(LogService logService, ILogger<LoggingController> logger)
    {
        _logService = logService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Write([FromBody] LogRequest request, CancellationToken ct)
    {
        var level = AllowedLevels.FirstOrDefault(l =>
            l.Equals(request.Level, StringComparison.OrdinalIgnoreCase));

        if (level is null)
        {
            return BadRequest(new { error = "Ungültiger level. Erlaubt: Info, Warning, Error." });
        }

        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = level,
            Service = request.Service.Trim(),
            Message = request.Message.Trim()
        };

        try
        {
            await _logService.AddAsync(entry, ct);
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException)
        {
            _logger.LogError(ex, "Log-Eintrag konnte nicht gespeichert werden.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }

        return StatusCode(StatusCodes.Status201Created);
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int limit = 100, CancellationToken ct = default)
    {
        limit = Math.Clamp(limit, 1, 1000);

        try
        {
            return Ok(await _logService.GetLatestAsync(limit, ct));
        }
        catch (Exception ex) when (ex is MongoException or TimeoutException)
        {
            _logger.LogError(ex, "Log-Einträge konnten nicht gelesen werden.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable);
        }
    }
}
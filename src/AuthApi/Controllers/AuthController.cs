using AuthApi.Data;
using AuthApi.Models;
using AuthApi.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly LogClient _log;

    public AuthController(
        AuthDbContext db,
        IPasswordHasher<User> passwordHasher,
        LogClient log)
    {
        _db = db;
        _passwordHasher = passwordHasher;
        _log = log;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Benutzer in der Datenbank suchen
        var user = await _db.Users
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        // Prüfen, ob der Benutzer existiert und aktiv ist
        if (user == null || !user.IsActive)
        {
            await _log.LogAsync("Warning", "Login fehlgeschlagen");

            return Unauthorized("Benutzername oder Passwort falsch.");
        }

        // Passwort mit dem gespeicherten Hash vergleichen
        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            await _log.LogAsync("Warning", "Login fehlgeschlagen");

            return Unauthorized("Benutzername oder Passwort falsch.");
        }

        // Erfolgreichen Login protokollieren
        await _log.LogAsync("Info", $"Login erfolgreich: UserId={user.Id}");

        // Erfolgreicher Login
        return Ok(new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString(),
            user.SupportLevel
        });
    }
}
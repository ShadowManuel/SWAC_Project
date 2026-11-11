
using AuthApi.Data;
using AuthApi.Models;
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

    public AuthController(
        AuthDbContext db,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Benutzer in der Datenbank suchen
        var user = await _db.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == request.Username);

        // Prüfen, ob der Benutzer existiert und aktiv ist
        if (user == null || !user.IsActive)
        {
            return Unauthorized("Benutzername oder Passwort falsch.");
        }

        // Passwort mit dem gespeicherten Hash vergleichen
        var result = _passwordHasher.VerifyHashedPassword(
            user, user.PasswordHash, request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            return Unauthorized("Benutzername oder Passwort falsch.");
        }

        // Erfolgreicher Login
        return Ok(new
        {
            user.Id,
            user.Username,
            user.Email,
            Role = user.Role.Name
        });
    }
}

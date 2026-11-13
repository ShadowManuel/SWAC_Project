using AuthApi.Data;
using AuthApi.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthApi.Controllers;

[ApiController]
[Route("api/users")]
public class UsersController : ControllerBase
{
    private readonly AuthDbContext _db;
    private readonly IPasswordHasher<User> _passwordHasher;

    public UsersController(
        AuthDbContext db,
        IPasswordHasher<User> passwordHasher)
    {
        _db = db;
        _passwordHasher = passwordHasher;
    }

    [HttpPost]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        // Prüfen, ob der Benutzername bereits existiert
        if (await _db.Users.AnyAsync(
            u => u.Username == request.Username))
        {
            return Conflict("Benutzername bereits vergeben.");
        }

        // Neuen Benutzer erstellen
        var user = new User
        {
            Username = request.Username,
            Role = UserRole.User,
            SupportLevel = 1
        };

        // Passwort hashen
        user.PasswordHash = _passwordHasher.HashPassword(
            user, request.Password);

        // Benutzer in SQL speichern
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        return StatusCode(201, new
        {
            user.Id,
            user.Username,
            Role = user.Role.ToString(),
            user.SupportLevel
        });
    }

    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        var users = await _db.Users
            .Select(u => new
            {
                u.Id,
                u.Username,
                u.Role,
                u.SupportLevel,
                u.IsActive
            })
            .ToListAsync();

        return Ok(users.Select(u => new
        {
            u.Id,
            u.Username,
            Role = u.Role.ToString(),
            u.SupportLevel,
            u.IsActive
        }));
    }
}
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
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // Prüfen, ob Benutzername oder E-Mail bereits existiert
        if (await _db.Users.AnyAsync(u =>
            u.Username == request.Username || u.Email == request.Email))
        {
            return Conflict("Benutzername oder E-Mail bereits vergeben.");
        }

        // Prüfen, ob die Supportrolle existiert
        if (!await _db.Roles.AnyAsync(r => r.Id == request.RoleId))
        {
            return BadRequest("Diese Supportrolle existiert nicht.");
        }

        // Neuen Benutzer erstellen
        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            RoleId = request.RoleId
        };

        // Passwort sicher hashen
        user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

        // Benutzer in SQL speichern
        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // Erfolgreiche Registrierung
        return StatusCode(201, new
        {
            user.Id,
            user.Username,
            user.Email,
            user.RoleId
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
                u.Email,
                u.IsActive,
                Role = u.Role.Name
            })
            .ToListAsync();

        return Ok(users);
    }
}
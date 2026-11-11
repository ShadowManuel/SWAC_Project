# Neuer SIMS-AuthApi-Code: Ziel, bestätigte Dateien und vorgeschlagene Erweiterungen

## Ziel-Ordnerstruktur im Team-Repository
```text
SWAC_Project/
├── sims.slnx
├── docker-compose.yml              # bereits im Repo; Inhalt unbekannt
├── README.md
└── src/
    ├── simsapi/                     # bereits vorhanden, Kollege / Ticket-Core
    └── AuthApi/                     # NEU, Nutzerin hat Projekt angelegt
        ├── Controllers/
        │   └── WeatherForecastController.cs  # Template, später entfernen
        ├── Models/
        │   ├── User.cs               # laut Nutzerin angelegt
        │   └── Role.cs               # laut Nutzerin angelegt
        ├── Data/
        │   └── AuthDbContext.cs      # laut Nutzerin angelegt
        ├── Program.cs               # aktueller Inhalt noch nicht gesendet
        ├── AuthApi.csproj            # Versionsdetails noch nicht gesendet
        ├── appsettings.json
        └── ...
```

## `Models/User.cs` – zuletzt gemeinsam erstellter Vorschlag, Erstellung bestätigt
```csharp
namespace AuthApi.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = "";
    public string Email { get; set; } = "";
    public string PasswordHash { get; set; } = "";

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public bool IsActive { get; set; } = true;
}
```

## `Models/Role.cs` – zuletzt gemeinsam erstellter Vorschlag, Erstellung bestätigt
```csharp
namespace AuthApi.Models;

public class Role
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Level { get; set; }

    public List<User> Users { get; set; } = new();
}
```

## `Data/AuthDbContext.cs` – vorgeschlagener Code, angeblich eingefügt/fehlerfrei
```csharp
using Microsoft.EntityFrameworkCore;
using AuthApi.Models;

namespace AuthApi.Data;

public class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>()
            .HasOne(u => u.Role)
            .WithMany(r => r.Users)
            .HasForeignKey(u => u.RoleId);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}
```

**Vorsicht:** `User`/`Role` sind bisher selbst erstellte POCOs und NICHT automatisch IdentityUser/IdentityRole. Falls volle Identity gewählt wird, Modellentwurf/DbContext ggf. umbauen BEVOR Migrationen erstellt werden. Alternativ eigene Modelle mit Framework-Passwort-Hasher verwenden und Registrierung/Autorisierung sorgfältig selbst umsetzen.

## Letzte erklärte (aber NICHT bestätigte) Ergänzung in `AuthApi/Program.cs`
```csharp
using AuthApi.Data;
using AuthApi.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

// Nach 'var builder = WebApplication.CreateBuilder(args);'
var connectionString =
    builder.Configuration.GetConnectionString("AuthDb")
    ?? throw new InvalidOperationException("ConnectionString AuthDb fehlt.");

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
```
Die `using`s und die Services gehören in den richtigen Stellen der **`AuthApi/Program.cs`**, nicht in `simsapi/Program.cs`. Die Nutzerin hat die Umsetzung dieser Anweisung bisher nicht bestätigt.

## Letzte erklärte (aber nicht bestätigte) lokale User-Secrets-Konfiguration
```json
{
  "ConnectionStrings": {
    "AuthDb": "Server=localhost,1433;Database=SIMS_Auth;User Id=sa;Password=DEIN_PASSWORT;Encrypt=True;TrustServerCertificate=True"
  }
}
```
- `DEIN_PASSWORT` ist ausschließlich ein Platzhalter.
- Host-PC mit SQL-Container veröffentlichter Port 1433: `localhost,1433` kann passen, muss anhand Compose geprüft werden.
- AuthApi als Container: statt `localhost` den passenden SQL-Service-Namen im Docker-Netzwerk verwenden.
- Nicht in Git committen; für Produktion Container-Secrets/geeignete Secret Stores verwenden.

## DB-Schema / Beziehung
`Users.Id` PK (int), `Users.RoleId` FK → `Roles.Id`, `Roles.Id` PK (int); ein Role-Eintrag hat viele User.
- 1 = First Support
- 2 = Second Support
- 3 = Third Support
Diese ID-Zuordnung ist **ein Vorschlag**, noch keine tatsächlich gesäte Datenbank.

Zu besprechen: Braucht SIMS zusätzlich Administratoren außerhalb der drei Supportlevel? Professor verlangt exemplarisch Administratoren und Benutzer. Möglich: Berechtigungsrollen (`Admin`, `Support`) getrennt von fachlichen Supportleveln (`First/Second/Third`). Vor Migrations/Seed entscheiden.

## API-Ziele (geplant, noch nicht implementiert)
- `POST /api/auth/login`: Username/Passwort prüfen; Token oder Session ausgeben.
- `GET /api/auth/me`: angemeldeten User + Supportlevel anzeigen.
- `POST /api/users`: nur befugten Admins User mit Supportlevel anlegen.
- `GET /api/users`: berechtigte Benutzerliste.
- `GET /api/users?level=2`: Mitarbeiter eines Levels für Ticketzuweisung (berechtigte Aufrufer).
- `PATCH /api/users/{id}/disable`: Benutzer deaktivieren.

Die API soll DTOs benutzen, niemals PasswortHash ausgeben. `IsActive` muss beim Login und bei relevanten API-Zugriffen beachtet werden. Logout/Token-Ablauf, Rollenänderung/Revocation und JWT-Validierung zwischen Services planen.

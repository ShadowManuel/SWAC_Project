# Was ist umgesetzt, was erst besprochen?

## Verifiziert durch Chat-Rückmeldungen und Screenshots
- Team-GitHub-Repo: `https://github.com/ShadowManuel/SWAC_Project`, Branch `main`. Im Online-Screenshot: `src/simsapi/`, `sims.slnx`, `docker-compose.yml`, `.dockerignore`, `.env.example`, `.gitignore`, `README.md`, `sims-projektplan.html`.
- Vorheriges EIGENES Repo: `https://github.com/Babsi011096/SAC`, Pfad `C:\Users\Babsi\source\repos\SAC`, mit 3 uncommitteten Änderungen an `signs.slnx`, `simsapi/Program.cs`, `simsapi/simsapi.csproj`. **Das ist nicht dasselbe Repo wie das des Teams.** Nur im Teamrepo weiterarbeiten.
- Nutzerin hat neues Teamrepo in Visual Studio geöffnet, `AuthApi` als zweites Web-API-Projekt unter `src/AuthApi` neu angelegt. Screenshot: Projektmappe `sims (2 von 2 Projekten)` mit `AuthApi` und `simsapi`. `AuthApi` zeigt typisches Web-API-Template, `Controllers/WeatherForecastController.cs`, `Program.cs`, Appsettings, Properties.
- Nutzerin bestätigte: `AuthApi/Models/User.cs` und `Role.cs` erstellt; `Data/AuthDbContext.cs` erstellt; EF-Core-Pakete installiert; DbContext ohne rote Fehler.

## Noch nicht bestätigt / unbedingt überprüfen
- Ob genau das Team-Repo als `origin` des **neuen** lokalen Verzeichnisses gesetzt ist (`git remote -v`).
- Ob `AuthApi` inzwischen gepusht wurde; Git Status und `sims.slnx` kontrollieren. Keine blinden Commits/Pushes, bevor Nutzerin bereit ist.
- Exakte Versionen installierter EF-Pakete in `AuthApi.csproj` und der endgültige Code in `Models/`, `Data/`, `Program.cs`.
- Ob Anweisung aus letzter Antwort schon ausgeführt ist: `AddDbContext<AuthDbContext>()`, `IPasswordHasher<User>`, User Secrets ConnectionString. Keine Rückmeldung dazu.
- `docker-compose.yml` wurde noch nicht vollständig in Chat gezeigt; SQL-Service, Netz/Port, Umgebungsvariablen, Volumes sind unbekannt.
- `dotnet ef`-Tool, SQL-Server-Container, Migration, Tabellen, Seed-Rollen und Login-Endpunkte sind noch nicht nachgewiesen.
- Identity-Ansatz noch nicht abschließend gewählt. Nicht mit Migration/Schema beginnen, bevor das feststeht.

## Vorhandene `src/simsapi/simsapi.csproj` aus altem Chat-Ausschnitt
```xml
<Project Sdk="Microsoft.NET.Sdk.Web">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.14" />
    <PackageReference Include="Scalar.AspNetCore" Version="2.18.7" />
  </ItemGroup>
</Project>
```
**Dies betrifft das ursprüngliche `simsapi`, NICHT zwingend `AuthApi`.**

## Ursprüngliche `simsapi/Program.cs` aus Nutzerbeitrag (nicht AuthApi!)
```csharp
namespace simsapi;
using Scalar.AspNetCore;
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Services.AddControllers();
        builder.Services.AddOpenApi();
        var app = builder.Build();
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
            app.MapOpenApi();
        }
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}
```

## Prüfung der Arbeitsumgebung (sanft, ohne Dateien zu verändern)
Im Terminal am Teamrepo:
```powershell
git remote -v
git status
git branch -a
docker compose config --services
docker compose ps
```
In Visual Studio: tatsächliche `src/AuthApi/AuthApi.csproj`, `Program.cs`, `Models/User.cs`, `Models/Role.cs`, `Data/AuthDbContext.cs` einsehen.

## Nächster Implementierungsablauf
1. Teambasis & Docker Compose bestätigen, keine Konfiguration des Kollegen überschreiben.
2. Auth-Design fixieren: volle Identity-Lösung ODER simple Modelle + PasswordHasher. Rollen/Administrator-Rechte klären.
3. EF Core ConnectionString per User Secrets für lokal, Container-Umgebungsvariablen für Docker. Unterschied `localhost:1433` vom Host vs. Docker-Service-Hostname IN Containern beachten.
4. Sql-Server-Laufzeit starten, sichere Secrets setzen; Migration `InitialAuth` erstellen und durchführen; `Roles` seeden.
5. Implementieren der Login-API + adminbeschränkter User-Anlage und Benutzerabfrage. DTOs verwenden, nie PasswordHash im Response.
6. Auth testen (gültige/ungültige Passwörter, deaktivierter User, unzulässige Level, unauthorisierte User-Abfrage).
7. JWT/Cookie und Integration mit Ticket-API abstimmen; Dockerfile für AuthApi und Compose ergänzen.
8. Git in eigenem Feature-Branch oder nach Teamvereinbarung; README und Semgrep ergänzen.

**Achtung:** `git pull origin main` von früher bezog sich auf `Babsi011096/SAC`, nicht auf `ShadowManuel/SWAC_Project`. Deshalb nicht aus dem damaligen „Already up to date“ auf den Stand des neuen Teamrepos schließen.

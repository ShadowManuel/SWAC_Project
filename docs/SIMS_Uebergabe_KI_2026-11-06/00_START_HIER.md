# SIMS – Übergabe an die FH-KI (Stand: 06.11.2026)

> **Bitte zuerst lesen.** Dies ist die zusammengefasste Übergabe einer laufenden Projektberatung. Die Dateien in diesem Paket enthalten die Professorenangabe, den aktuellen Stand, Kontext aus dem älteren EasyList-Projekt sowie den geplanten Auth-Microservice. **Unbedingt zwischen umgesetzt, vorgeschlagen und ungeprüft unterscheiden.**

## Ziel und Zuständigkeit

SIMS = Security Incident Management System: Sicherheitsvorfälle/Tickets verwalten, bearbeiten und eskalieren. Ein Team arbeitet an getrennten C#/.NET-Microservices. Die Nutzerin ist für **AuthApi** verantwortlich: Support-Mitarbeiter registrieren/anlegen, Login, Benutzerverwaltung, Zuordnung zu **First / Second / Third Support**, SQL-Datenhaltung und sichere Authentifizierung. Ein Kollege baut Ticketfunktionen und Zuweisung/Eskalation. Später Frontend, Logging (ggf. Redis), Docker und Integration. Tickets kommen als Vorfälle von außerhalb; die manuelle Vorfallerfassung ist laut Professoren-PDF trotzdem eine Projektanforderung und muss vom Team insgesamt erfüllt werden.

## Tatsächlich bestätigter Ist-Stand

- Das Team-Repository aus Screenshots: **https://github.com/ShadowManuel/SWAC_Project**, Branch `main`, ursprüngliche Struktur `src/simsapi/`, `sims.slnx`, `docker-compose.yml`, `README.md`, `.env.example`, `.dockerignore` usw. Ein anderes lokales Repository `https://github.com/Babsi011096/SAC` existierte ebenfalls; **nicht vermischen**. Die Nutzerin möchte im Team-Repository arbeiten und hat angegeben, dieses geklont zu haben. Aktuelle `git remote -v`-Ausgabe des NEUEN lokalen Klons liegt nicht vor.
- In Visual Studio zeigt die Projektmappe **`sims` zwei Projekte: `AuthApi` und `simsapi`**. Die Nutzerin bestätigte, dass **`AuthApi` unter `SWAC_Project/src/AuthApi`** liegt, neben `src/simsapi`.
- `AuthApi` wurde als ASP.NET Core Web API angelegt, mit `Controllers`, `Program.cs`, `appsettings.json`, `AuthApi.csproj`, WeatherForecast-Beispiel.
- In `AuthApi/Models` wurden **`User.cs` und `Role.cs`** nach den vorgeschlagenen einfachen Modellen angelegt; die Nutzerin bestätigte das.
- EF-Core-Pakete `Microsoft.EntityFrameworkCore.SqlServer` und `Microsoft.EntityFrameworkCore.Design` (10.0.x empfohlen) sowie `Data/AuthDbContext.cs` wurden in der Beratung eingerichtet; die Nutzerin bestätigte **keine roten Fehler**. Die exakten Versionen und der lokale Dateistand wurden **nicht** durch einen neuen Upload verifiziert.
- Der zuletzt erklärte Schritt war die **Konfiguration des `AuthDbContext` und des `IPasswordHasher<User>` in `AuthApi/Program.cs` sowie einer Connection String in User Secrets**. **Es gibt noch KEINE Bestätigung**, dass dies umgesetzt ist. Docker Compose ist ebenfalls noch nicht geprüft, SQL-Verbindung/Tabellen/Migration/Login sind nicht als funktionsfähig bestätigt.
- Ein vorhandenes Teamprojekt `src/simsapi` wurde aus einem Screenshot/Code gezeigt: `TargetFramework net10.0`, `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`, Controller-basiert und `Program.cs` mit `MapOpenApi`, `MapScalarApiReference`, `MapControllers`. Das ist nicht automatisch der `AuthApi`-Ist-Code.

## Architekturentscheidung im Gespräch

- **Empfohlen:** Ein eigenständiges `AuthApi`-Projekt neben `simsapi` (Ticket-/Core-API). Getrennt lauffähige APIs; nicht `AuthApi` in `simsapi` verschieben.
- Datenbanktechnik für SIMS: **SQL Server** (im Docker-Container, wie Professor verlangt) + Entity Framework Core. NoSQL Redis für Logs oder Sessions im Gesamtprojekt.
- Vorgeschlagene Auth-Datenmodelle: `User { Id, Username, Email, PasswordHash, RoleId, Role, IsActive }`, `Role { Id, Name, Level, Users }`; `User.RoleId` ist FK → `Role.Id`. Rolle/Supportlevel ausschließlich serverseitig festlegen.
- **Noch nicht final entschieden:** einfache `User`/`Role`-Klassen + `PasswordHasher<User>` (letzte konkrete Empfehlung), **oder** vollständiges ASP.NET Core Identity (`IdentityUser`, `IdentityRole`). NICHT beides unkoordiniert vermischen; Entscheidung **vor erster Migration** treffen. Full Identity wurde zuvor als Option diskutiert, die konkreten Dateien sind derzeit aber einfache eigene Modelle.
- Keine direkte SQL-FK zwischen getrennten Services nötig: Ticket-Service speichert z.B. `AssignedUserId` und fragt bei Bedarf die Auth-API nach berechtigten Benutzerinformationen. Externe IDs/API-Vertrag gemeinsam vereinbaren.

## Wichtigste Anforderungen aus Professoren-PDF `Referenzen/SIMS_Original.pdf`

- Mind. **4 Docker-Container** in sinnvollen Netzen: App, APIs, Redis, SQL Server; lokal vollständig in Docker lauffähig.
- RESTful API(s); SQL-Speicherung mit Architekturpattern oder EF, ER-Diagramm, Schutz vor SQL-Injection; NoSQL/Redis einsetzen; Semgrep/SAST; README mit Security-Maßnahmen, Features, Diagrammen, Version/Lizenz/Team/Roadmap, SAST-Ergebnissen; funktionierende Demo; Git.
- Fachlich: Vorfälle erfassen, ändern, schließen, eskalieren; Benutzer anmelden, anlegen/deaktivieren; Benutzer- und Vorfalllisten; Logging.
- Punkte: 1–3 für sinnvolle Microservices, 3 für Datenhaltung, je 1 SAST/Qualität, Docker, Dokumentation, Demo (insgesamt 8–10), optional Zusatzpunkte.
- `docker-compose.yml` wird im vorhandenen Repo erwähnt; seinen exakten Inhalt hat die Nutzerin noch **nicht geschickt**.

## Referenz aus altem EasyList/ProjektWeb

- Älteres Projekt: **https://github.com/c2310475117/ProjektWeb_EasyList**; Node.js/Express/Sequelize/SQLite statt C#/.NET/EF/SQL Server.
- `index.html`: `POST http://localhost:3000/user/register` und `POST .../user/login` mit JSON (Username, Email/Password); auf Erfolg JWT/ID gespeichert, Weiterleitung zu `todo.html`.
- `userRoutes.js`: `User.findOne` in Sequelize; `bcrypt.hash(password, 10)` zur Registrierung; `bcrypt.compare(password, user.password_hash)` beim Login; `generateAccessToken(user.user_id)`.
- `auth.js`: `jsonwebtoken` signiert Token mit `{ userId }` und `expiresIn: '1h'`; Middleware prüft `Authorization: Bearer ...`; Zugriff auf eigene Listen wird serverseitig geprüft.
- `userModel.js`: Sequelize `users` mit `user_id` als autoincrement PK, eindeutigen `user_name` und `email`, `password_hash`, `created_at`, `updated_at`.
- **Nur Konzepte übernehmen**, nicht den JS-Code oder die damaligen Sicherheitsprobleme. Siehe `03_EasyList_Referenz.md`.

## Nächste sinnvolle Schritte – exakt in dieser Reihenfolge

1. **Repository/Pfade bestätigen:** Terminal im Teamprojekt: `git remote -v`, Projektmappe, tatsächliche Dateipfade; Änderungen des Kollegen nicht überschreiben.
2. **Ist-Code der neuen AuthApi anfordern/prüfen:** `src/AuthApi/AuthApi.csproj`, `Program.cs`, `Models/User.cs`, `Models/Role.cs`, `Data/AuthDbContext.cs`. Keine unbestätigten Vorschläge als bereits implementiert behandeln.
3. **Vorhandenes Docker Compose prüfen:** `docker-compose.yml` + `.env.example` (ohne echte Geheimnisse); `docker compose config --services` und `docker compose ps`. Prüfen, ob SQL Server bereits konfiguriert ist und unter welchem Servicenamen/Port.
4. **Entscheidung Security/Identity und Datenmodell:** simple User/Role+`PasswordHasher` ODER vollständiges ASP.NET Core Identity, dazu Administrator-Berechtigung für Registrierung und sichere Rollenvergabe. Noch keine Migration vor dieser Entscheidung.
5. **EF Core konfigurieren und Migration erzeugen** (`AuthDbContext`, Connection Strings per User Secrets/Umgebungsvariablen, EF Tools, SQL Server Start). SQL-Tabellen und 3 Rollen anlegen und testen.
6. **Auth-Endpunkte bauen:** Anmeldung; kontrolliertes Anlegen/Deaktivieren; `GET /api/users?level=...` nur mit Berechtigungsprüfung; `/api/auth/me`. JWT oder sichere Cookie-Sitzung vereinbaren; Ticket-Service soll Identität serverseitig prüfen können.
7. **Integration** mit Ticket-Service (ID, Rollen, Token-Validierung, Auth-Daten-Lifecycle); Dockerfile/Compose; Tests und Semgrep.

## Sicherheitsaussagen, die die nächste KI berücksichtigen muss

- Kein Passwort im Klartext in SQL oder Logs. `PasswordHasher<User>` bzw. ASP.NET Core Identity und Passwortprüfung nutzen.
- Keine fest codierten JWT-Secrets. Alte EasyList-Fallback-Zeile `process.env.JWT_SECRET || 'geheimnis'` ist **unsicher**.
- Rollen nicht aus untrusted Client-Input übernehmen; Supportrollen/Zuweisung nur berechtigt verwalten.
- JWT nie in Query-Strings speichern; `localStorage`-Tokens/`innerHTML` aus altem Projekt nicht unkritisch kopieren.
- API muss Zugriffe serverseitig autorisieren; User-ID aus Frontend/URL ist allein **kein** Berechtigungsnachweis (IDOR).
- Lokale Verbindungsdaten sind nicht bestätigt; keine realen Passwörter in `appsettings.json` oder Git hinterlegen.

## Zusammenarbeit / Antwortstil

Die Nutzerin lernt durch Anwenden und will **kleine, konkrete Schritte** in Visual Studio/C#, kurze Erklärung was/weshalb und eine unmittelbare Prüfmöglichkeit. **Sie möchte den Backend-Code selbst verstehen und schreiben, mit KI-Unterstützung.** Laut Vorlesungsmitschrift wird der Professor keine Merge-Konflikte und keinen KI-generierten Code für die Studierenden reparieren; deshalb Code nicht blind übernehmen und Änderungen nachvollziehbar halten. Keine vollständige Neuschreibung des Team-Repos, keine unkontrollierten Änderungen an Projekt des Kollegen. Bei Fehlern nach Ausgabe fragen und diagnostisch vorgehen. Die Nutzerin ist verantwortlich für den Auth-Teil, nicht Ticketlog oder Escalation.

## Dateien in diesem Übergabepaket

- `01_Professor_und_System.md` – Anforderungen und Verantwortlichkeiten.
- `02_Iststand_und_naechste_Schritte.md` – Status mit sicheren Prüfkommandos.
- `03_EasyList_Referenz.md` – konkrete alte Login-/Model-Logik und Probleme.
- `04_Code_und_Datenmodell.md` – bisheriger C#-Stand bzw. vorgeschlagene Codebausteine.
- `05_Schnittstellen_und_Sicherheit.md` – Entscheidungspunkte der Integration.
- `Referenzen/SIMS_Original.pdf` und `Referenzen/Team_Architektur_Skizze.jpg` – Originalquellen.

**Achtung:** Dieses Paket ist eine Übergabe, kein aktueller Repository-Export. Angaben über lokale Dateien beruhen auf der letzten Rückmeldung; erst am tatsächlichen Code bestätigen.

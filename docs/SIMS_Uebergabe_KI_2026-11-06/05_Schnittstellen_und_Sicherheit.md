# Schnittstellen zum Ticket-Service und Sicherheitsentscheidungen

## Verantwortlichkeiten
**AuthApi (Nutzerin):** User-Daten, sichere Passwortverarbeitung, Login/Token oder sichere Sitzung, Supportlevel First/Second/Third, berechtigte Benutzerverwaltung, User-Lookups für Ticket-Zuweisung. **Ticket-Service (Kollege):** Tickets speichern, zuweisen, Status/Eskalation, später Ticketlogs. **Gemeinsam:** API-Verträge, Docker, Redis, Doku, SAST, Demo.

## Vorläufiger API-Vertrag, vor Implementierung mit Team abstimmen

`POST /api/auth/login`
```json
{"username":"testuser","password":"<nur-im-Request>"}
```
Response-Beispiel nur konzeptionell (bei Tokenlösung): `{"accessToken":"<token>","expiresIn":3600}`. Keine Passwörter/Hashes im Response.

`GET /api/auth/me` (Authentifizierung erforderlich)
```json
{"id":42,"username":"testuser","supportLevel":2,"isActive":true}
```

`GET /api/users?level=2` (autorisiert)
```json
[{"id":42,"username":"testuser","supportLevel":2}]
```

Der Ticket-Service speichert bei einem Ticket beispielsweise `AssignedUserId=42` und kontrolliert über die AuthApi, ob die ID existiert und dem gewünschten Level angehört. Die Daten müssen nicht per SQL Foreign Key zwischen Services gekoppelt sein. Kein Zugriff des Ticket-Service auf Passwortdaten.

## Kritische Entscheidungen, die noch offen sind
1. Auth-Implementierung: volle ASP.NET Core Identity (robuster, aber anderer Entity-Typ/Schema) vs. einfache eigene `User`/`Role`-Modelle mit ASP.NET Core `PasswordHasher<User>` und eigener JWT-Auth (überschaubares Schema, höherer Eigenaufwand). **Erst entscheiden, dann migrieren.**
2. UI-/Tokenstrategie: Bearer-JWT zwischen verschiedenen Services, oder Cookie-Sitzung; bei JWT Aussteller, Zielgruppe, Signatur, Gültigkeit, Schlüsselrotation und Rollen-/Statusänderungen berücksichtigen. Kein Token in URL. JWT nicht bloß dekodieren, vollständig validieren.
3. Rollenmodell: Supportlevel als `Role` oder fachliches `SupportLevel`, getrennt von privilegierter `Admin`-Rolle. Öffentliche Selbstregistrierung oder nur Admin-Anlage? Sicherste Standardannahme: kein freier Rollenwechsel/keine Admin-Selbstregistrierung.
4. Benutzeridentifikation/ID-Typ und Service-Kommunikation: `int` wie Modelle oder `Guid`; stabile API-Verträge. Tickets haben ggf. Bearbeiter-ID sowie Melder außerhalb der drei Supportgruppen – Modell gemeinsam festlegen.
5. User-Deaktivierung: blockiert neue Logins; Umgang mit bestehenden Tokens/Sessions vereinbaren.
6. SQL Server in Compose: Service-Name, Container-Port/Host-Port, Connection Strings, Volumes, Init-/Migration-Verfahren.
7. Fester Zielumfang/Level-Berechtigungen: Welche Tickets darf First vs. Second vs. Third sehen oder eskalieren?

## Security-Checkliste
- Passwörter niemals loggen oder im Klartext speichern; Framework-Passwort-Hasher verwenden, Hash ausschließlich in Datenbank.
- Generische Fehlermeldung bei ungültigem Username/Passwort; Rate-Limiting und Lockout-Konzept.
- TLS/HTTPS für Browser/API; Docker-Entwicklung mit klarer lokaler Konfiguration.
- Geheimnisse in User Secrets (lokal) oder Container-Secrets/Env (Deployment), nicht in Git.
- Autorisierung gegen serverseitig validierte Identität, nicht nur Client-UserId oder Supportlevel aus Request.
- Rollen/Zuweisungen nur an berechtigte Benutzer; keine privilege escalation.
- Eigene Berechtigungsprüfung für jeden User-/Ticket-Endpunkt gegen IDOR.
- EF Core parametrisierte Queries/keine String-Konkatenation für SQL; XSS-Ausgabe sicher rendern.
- Tests: Login gültig/falsch, nicht existierender User, deaktivierter User, falscher Supportlevel, unauthorisierte Listen/Änderungen; SAST-Semgrep und README-Dokumentation.

## Empfehlung für unmittelbares weiteres KI-Gespräch
> Wir sind bei `src/AuthApi` in einem C#-/.NET-10-Teamprojekt. `Models/User.cs`, `Models/Role.cs` und `Data/AuthDbContext.cs` sind schon angelegt. Bitte sieh dir zuerst meine aktuelle `AuthApi/Program.cs`, `AuthApi.csproj` und die vorhandene `docker-compose.yml` an, kläre mit mir die Wahl zwischen vollständigem ASP.NET Core Identity und einfachen Modellen + PasswordHasher und leite mich dann in kleinen Schritten durch SQL-Verbindung, Migrationen und Login-API. Bitte keine großen Dateien blind überschreiben und keinen Code für den Ticket-Service meines Kollegen ohne Absprache ändern.

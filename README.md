# SIMS - Security Incident Management System

SIMS ist ein Helpdesk- und Ticketsystem für Sicherheitsvorfälle. Nutzer melden Tickets, Supporter übernehmen sie, eskalieren sie bei Bedarf auf die nächste Stufe und schließen sie ab. Alle Aktionen werden zentral protokolliert.

Das Projekt besteht aus mehreren Microservices (.NET 10), zwei SQL-Server-Datenbanken, einer MongoDB für Logs und einem Web-Frontend. Alles läuft mit Docker.

## Inhalt

- [Projekt starten](#projekt-starten)
- [Anmelden und Testuser](#anmelden-und-testuser)
- [Architektur](#architektur)
- [Frontend](#frontend)
- [API-Überblick](#api-überblick)
- [Projektstruktur](#projektstruktur)
- [Zusammenarbeit im Team](#zusammenarbeit-im-team)
- [Fehlersuche](#fehlersuche)
- [Bekannte Lücken](#bekannte-lücken)

## Projekt starten

Voraussetzungen: Docker Desktop mit mindestens 6 GB Arbeitsspeicher (zwei SQL-Server-Container), unter Windows mit WSL 2. Für die Entwicklung zusätzlich das .NET 10 SDK.

1. Repo klonen
2. `.env.example` nach `.env` kopieren und alle Werte ausfüllen (siehe [Anmelden und Testuser](#anmelden-und-testuser))
   ```
   copy .env.example .env
   ```
3. Alles bauen und starten:
   ```
   docker compose up --build
   ```
   Der erste Start dauert einige Minuten. Mit `docker compose up -d --build` läuft alles im Hintergrund.
4. Prüfen, ob alle Container laufen (acht Stück, alle "Up"):
   ```
   docker compose ps
   ```
5. Frontend öffnen: http://localhost:8080

| Service | Adresse |
|---|---|
| Frontend | http://localhost:8080 |
| Auth-API | http://localhost:5001/scalar |
| Ticket-API | http://localhost:5002/scalar |
| Logging-API | http://localhost:5003/scalar |
| Unterrichtsbeispiel | http://localhost:5000/scalar |

Stoppen mit `docker compose down`. Mit `docker compose down -v` werden zusätzlich alle Daten gelöscht.

Nach einer Änderung an einem Service diesen neu bauen, zum Beispiel:
```
docker compose up -d --build auth-api
```

## Anmelden und Testuser

Die Datei `.env` wird **nie** committet. Sie enthält die Passwörter für Datenbanken und Testuser. Das Team stimmt die Werte untereinander ab.

| Variable | Zweck |
|---|---|
| `AUTH_DB_PASSWORD`, `TICKET_DB_PASSWORD` | SQL Server verlangt mindestens 8 Zeichen mit Groß- und Kleinbuchstaben, Ziffer und Sonderzeichen |
| `SEED_TESTUSER1_PASSWORD` bis `SEED_TESTUSER3_PASSWORD` | Passwörter der drei Testuser |

Beim Start legt die Auth-API automatisch diese Testuser an, aber nur, wenn das zugehörige Passwort in der `.env` gesetzt ist:

| Benutzername | Support-Level | Rolle |
|---|---|---|
| `testuser1` | 1 | User |
| `testuser2` | 2 | User |
| `testuser3` | 3 | User |

Nach dem Eintragen oder Ändern der Passwörter die Auth-API neu starten: `docker compose up -d --build auth-api`. Bestehende Testuser behalten ihr altes Passwort. Soll es sich ändern, mit `docker compose down -v` die Datenbanken zurücksetzen.

Neue Nutzer können sich außerdem im Frontend selbst registrieren. Sie bekommen immer die Rolle User.

## Architektur

```
Browser
   |
   v
 web (nginx, Port 8080)  -- liefert das Frontend aus und leitet API-Aufrufe weiter
   |-- /auth-api/     --> auth-api     --> auth-db   (SQL Server)
   |-- /ticket-api/   --> ticketapi    --> ticket-db (SQL Server)
   '-- /logging-api/  --> logging-api  --> log-db    (MongoDB)

auth-api und ticketapi schreiben ihre Ereignisse zusätzlich an die logging-api.
```

| Service | Aufgabe | Daten |
|---|---|---|
| `web` | Frontend (HTML, CSS, JavaScript) und Reverse Proxy | keine |
| `auth-api` | Registrierung, Login, Nutzerliste | SQL Server (`auth-db`) |
| `ticketapi` | Tickets anlegen, übernehmen, eskalieren, erledigen | SQL Server (`ticket-db`) |
| `logging-api` | Zentrales Log, schreiben und lesen | MongoDB (`log-db`) |
| `simsapi` | Beispiel aus dem Unterricht, nur als Anhaltspunkt | keine |

Docker-Netze trennen die Datenbanken von den APIs: Jede Datenbank ist nur für ihren eigenen Service erreichbar. Der Browser spricht ausschließlich mit `web`. Dadurch braucht es kein CORS, und die APIs müssen nichts über das Frontend wissen.

## Frontend

Das Frontend liegt in `src/web` und kommt ohne Build-Schritt aus: eine `index.html`, eine `nginx.conf` und ein `Dockerfile`. Es lädt alle Daten live aus den APIs, es gibt keine eingebauten Testdaten.

| Bereich | Inhalt |
|---|---|
| Login | Anmelden, Registrieren |
| Startseite | Kennzahlen und eigene Tickets in Bearbeitung |
| Tickets | Tabelle mit Suche und Filtern, Detailansicht, Übernehmen, Eskalieren, Erledigen, neues Ticket |
| Nutzer | Nutzerliste, neuen Nutzer anlegen |
| Logs | Logeinträge mit Level-Filter |
| Diagnose | Prüft die Services und führt einen kompletten Demo-Durchlauf aus |

Die Diagnose-Seite ist der schnellste Weg, nach einer Änderung am Backend zu prüfen, ob alles noch zusammenpasst. Der Demo-Durchlauf legt einen Test-Nutzer und ein Test-Ticket an, bearbeitet das Ticket bis zum Abschluss und kontrolliert, ob auth-api und ticketapi ins Logging schreiben.

Das Design folgt dem Wireframe in `docs/wireframe`.

## API-Überblick

Alle Pfade gelten direkt am Service (zum Beispiel `http://localhost:5002/api/ticket`). Über das Frontend laufen sie mit Präfix, etwa `/ticket-api/api/ticket`. Die genauen Schemas stehen in der Scalar-Oberfläche jedes Services.

**auth-api**

| Methode | Pfad | Zweck |
|---|---|---|
| POST | `/api/auth/login` | Anmelden. Antwort: `id`, `username`, `role`, `supportLevel`. 401 bei falschen Daten |
| POST | `/api/users` | Nutzer anlegen (Rolle immer `User`). 409, wenn der Name vergeben ist |
| GET | `/api/users` | Nutzerliste |

**ticketapi**

| Methode | Pfad | Zweck |
|---|---|---|
| GET | `/api/ticket?level=` | Offene Tickets, optional nach Level gefiltert |
| POST | `/api/ticket` | Ticket anlegen (`titel`, `beschreibung`, `kategorie`, `ersteller`) |
| POST | `/api/ticket/{id}/uebernehmen` | Ticket übernehmen (`bearbeiter`). Status wird `InBearbeitung` |
| POST | `/api/ticket/{id}/eskalieren` | Ein Level höher, Status wieder `Offen`, Bearbeiter leer |
| POST | `/api/ticket/{id}/erledigen` | Ticket abschließen. Status wird `Erledigt` |
| GET | `/api/ticket/meine?bearbeiter=` | Tickets, die ein Nutzer in Bearbeitung hat |

Kategorien: `Phishing`, `Logging`, `Malware`, `Connecting`, `Other`. Tickets der Kategorien Phishing und Malware starten auf Level 2, alle anderen auf Level 1. Das höchste Level ist 3. Ein Status, der nicht zur Aktion passt, liefert 409.

**logging-api**

| Methode | Pfad | Zweck |
|---|---|---|
| POST | `/api/logs` | Eintrag schreiben (`level`: Info, Warning oder Error, `service`, `message`) |
| GET | `/api/logs?limit=` | Neueste Einträge lesen |

## Projektstruktur

| Ordner | Inhalt |
|---|---|
| `src/AuthApi` | Auth-Service mit EF-Core-Migrationen |
| `src/ticketapi` | Ticket-Service mit EF-Core-Migrationen |
| `src/logging-service` | Logging-Service |
| `src/web` | Frontend und nginx-Konfiguration |
| `src/simsapi` | Beispiel aus dem Unterricht, nur als Anhaltspunkt |
| `docs/` | Wireframe, Übergabe-Dokumente und Referenzen |
| `sims-projektplan.html` | Projektplan: Architektur, Schnittstellen, Datenmodell (im Browser öffnen) |
| `docker-compose.yml` | Startet alle Container |

## Zusammenarbeit im Team

- Nie direkt auf `main` pushen. Für jede Aufgabe einen eigenen Branch anlegen (Task-Kürzel im Namen) und über einen Pull Request mergen.
- Kein Force-Push.
- `main` muss jederzeit mit `docker compose up --build` starten.
- Ändert sich das Datenmodell eines Services, gehört eine neue EF-Core-Migration dazu (`dotnet ef migrations add <Name>` im Ordner des Services). Ohne Migration startet der Service nicht.
- `.env` und Passwörter nie committen.

## Fehlersuche

| Problem | Lösung |
|---|---|
| `docker` wird nicht erkannt | Docker Desktop installieren und die PowerShell danach neu öffnen |
| "Cannot connect to the Docker daemon" | Docker Desktop starten und warten, bis es läuft. Sonst WSL 2 prüfen (`wsl --update`) |
| Port 8080 belegt | In `docker-compose.yml` beim `web`-Service `"8080:80"` auf einen freien Port ändern |
| Container fehlt oder beendet sich | `docker compose ps -a` zeigt den Status, `docker compose logs <service> --tail 40` den Grund |
| Frontend zeigt 502 oder "nicht erreichbar" | Der Service startet noch oder ist abgestürzt. Eine Minute warten, dann Logs prüfen |
| Login mit Testuser schlägt fehl | Passwort in `.env` gesetzt? Danach `docker compose up -d --build auth-api` |
| `auth-api` startet nicht (`PendingModelChangesWarning`) | Das Modell wurde geändert, aber es fehlt eine Migration (siehe Teamregeln) |
| Logging-API meldet 503 | MongoDB (`log-db`) ist nicht erreichbar: `docker compose logs log-db` |
| `ticket-db` beendet sich | `TICKET_DB_PASSWORD` fehlt oder ist zu schwach. `.env` prüfen, danach `docker compose down -v` |
| Frontend zeigt noch die alte Version | Im Browser `Strg+F5` drücken. Nach Änderungen `docker compose up -d --build web` |

## Bekannte Lücken

Diese Funktionen aus dem Wireframe fehlen im Backend noch:

- Es gibt keine Liste aller Tickets. Das Frontend setzt die Anzeige aus offenen Tickets und den Tickets in Bearbeitung zusammen. Erledigte Tickets sind nicht abrufbar.
- Ein einzelnes Ticket lässt sich nicht per Id abfragen.
- Nutzer lassen sich nicht sperren, und es gibt keine Rollenverwaltung. Es wird kein Admin-Account angelegt.
- Es gibt keine Priorität, keine Kommentare und keine Zuweisung an einen bestimmten Supporter. Einsender und Bearbeiter sind einfache Namen.

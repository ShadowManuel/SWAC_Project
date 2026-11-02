# SIMS – Security Incident Management System

## Projekt starten

Voraussetzungen: Docker Desktop mit mindestens 6 GB Arbeitsspeicher (zwei SQL-Server-Container), .NET 10 SDK für die Entwicklung.

1. Repo klonen
2. `.env.example` nach `.env` kopieren und die Passwörter ersetzen
3. Alles bauen und starten:
   ```
   docker compose up --build
   ```
4. Prüfen, ob alle Container laufen:
   ```
   docker compose ps
   ```

| Service | Adresse |
|---|---|
| Frontend | http://localhost:8080 |
| Auth-API | http://localhost:5001/scalar |
| Ticket-API | http://localhost:5002/scalar |
| Logging-API | http://localhost:5003/scalar |
| Unterrichtsbeispiel | http://localhost:5000/scalar |

Stoppen mit `docker compose down`. Mit `docker compose down -v` werden zusätzlich alle Daten gelöscht.

## Projektstruktur

| Ordner | Inhalt |
|---|---|
| `src/` | Die Services und das Frontend |
| `src/simsapi` | Beispiel aus dem Unterricht, nur als Anhaltspunkt; wird nicht deployt |
| `TASKS.md` | Aufgaben, Arbeitsregeln und Vorlagen für das Team |
| `sims-projektplan.html` | Projektplan: Architektur, Schnittstellen, Datenmodell (im Browser öffnen) |

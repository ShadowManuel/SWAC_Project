# SIMS – Professorenangabe, Architektur und Arbeitsaufteilung

## Quelle
Original: `Referenzen/SIMS_Original.pdf` (3 Seiten). Gruppen-Skizze: `Referenzen/Team_Architektur_Skizze.jpg`.

## Projekt
Security Incident Management System: manuelle Erfassung sicherheitsrelevanter Vorfälle mit Bearbeiter, Melder, Schweregrad, Status, CVE, betroffenem System, Beschreibung und Zeitstempel. Eskalation auf Benutzer/Supportlevel; Benutzerverwaltung (z. B. Administrator, Benutzer), Logging (SQL oder Redis: Id, Timestamp, Loglevel, Message), Anmeldung, Vorfall add/edit/close, Escalation, User add/disable sowie Incident- und Userlisten.

## Technische Pflichten aus der Angabe
- Mindestens vier sinnvoll vernetzte Docker-Container: Anwendung, API(s), Redis, SQL Server. Vollständig lokal per Docker lauffähig, Dockerfiles erstellen. `docker-compose.yml` optional bei Abgabe, aber im Team-Repo bereits vorhanden.
- Git-Repository als gemeinsame Quellcodeverwaltung.
- Objektorientierte, fertig lauffähige Anwendung mit persistenter Datenverarbeitung.
- Für relationale Speicherung Datenzugriffspattern (Vorlesung `05-DB-AppDev.pdf`: Table/Row Data Gateway, Active Record, Data Mapper) **oder** Entity Framework. SQL Injection vermeiden.
- RESTful APIs, Redis NoSQL z. B. Logs/Sessions, SQL-Datenbank, ER-Diagramm.
- Frontend (Web mit LLM zulässig), mindestens Konsolenclient.
- Statische Codeanalyse mit Semgrep oder Alternative und allgemeine Codequalität; Dokumentation von Sicherheitsüberlegungen.
- README.md: Features/Funktionalität, Version, Lizenz, Mitwirkende, fiktive Roadmap, Systemvoraussetzungen, Repo-Link, ER-Diagramm, ggf. Klassendiagramm, SAST-Ergebnisse; Aufgabenstellung selbst laut Angabe nicht als Aufgabe dokumentieren; Markdown-Badges.
- Abgabe: Repo-Link, SQL-Skript falls ohne EF, Dockerfiles, README, Demo.

## Punkte
- 1–3 Punkte für 1–3 sinnvolle Microservices (max. 3 Punkte)
- 3 Punkte Datenhaltung (Patterns oder EF, SQL, NoSQL)
- 1 Punkt SAST und Qualität
- 1 Punkt Docker
- 1 Punkt Dokumentation
- 1 Punkt lauffähige Demo
- Optional: SBOM/DependencyTrack, CI(+CD), externe Systeme z. B. Jira.

## Team-Skizze (nicht mit finaler Laufzeitarchitektur verwechseln)
Auth. → User Problem → Ticket → 2nd; Ticket führt zu Ticketlog mit First/Second/Third und Logging-Datenbank; Auth. bezieht eine Benutzer-Datenbank. Einige Pfeile/Verbindungen sind konzeptionell und in der Skizze noch nicht vollständig geklärt.

**Mündliche Erklärung der Nutzerin:** Drei Mitarbeiterarten First, Second, Third Support loggen sich ein. Tickets kommen durch externe Nutzer bzw. werden von außen erstellt; die Nutzerin baut nicht die externe Vorfallerstellung. Tickets werden durch den Kollegen den Levels/Benutzern zugeordnet und bearbeitet. Zusätzliche Ticket-Logging/Eskalations-Funktionen kommen später. Nutzerin baut jetzt ausschließlich Auth-API inkl. SQL-Benutzerverwaltung. Das Team muss die restlichen Professorenanforderungen gemeinsam erfüllen.

## Empfohlene logische Containeraufteilung
1. App/Frontend
2. AuthApi (eigenständiger .NET-Service)
3. TicketApi oder bestehendes `simsapi` (Kollege)
4. SQL Server
5. Redis

**Keine Zusicherung, dass diese 5 Container schon existieren.** Es handelt sich um die abgestimmte Zielarchitektur; tatsächlichen Inhalt von Compose noch prüfen. Auth und Ticket können denselben SQL-Server-Container nutzen, mit getrennten Datenbanken/Schemata und ohne harte serviceübergreifende Foreign Keys.

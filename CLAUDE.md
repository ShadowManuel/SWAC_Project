# SIMS – Kontext für Claude

Uni-Projekt an der FH St. Pölten: **Security Incident Management System**. Abgabe nach etwa 2 Wochen. Das Projekt ist die Grundlage für ein Cloud-Beispiel in einem späteren Semester. Die Angabe liegt bei Lucas lokal unter `../SIMS/SIMS.pdf` (nicht im Repo).

## Was das Team weiß

Lucas hat der Gruppe angekündigt:
- Er arbeitet genauer aus, **wer was wie übernimmt**, damit jeder etwas zu tun hat, und legt das Dokument ins Repo.
- Er definiert vorab **alle Schnittstellen**, damit jeder sofort mit seinem Teil anfangen kann.

Das Ergebnis davon ist:
- `sims-projektplan.html`: Umfang, Architektur, Rollen, Zeitplan, Schnittstellen, Datenmodell, Arbeitsregeln, Abgabe-Checkliste. Veröffentlicht unter https://claude.ai/artifact/KJriQpcryF4AM3pAkwCypW (privat, Lucas teilt den Link).
- GitHub-Issues #1–#33, eins pro Task, mit Beschreibung, Akzeptanzkriterien, Testfällen und Definition of Done. Board „SIMS“: https://github.com/users/ShadowManuel/projects/3
- README mit Anleitung „Projekt starten“.

Was im Plan und in den Issues steht, ist die Absprache mit dem Team. Wer dort etwas ändert, ändert eine Zusage an das Team; größere Änderungen erst mit Lucas abstimmen.

## Team

- **Lucas** (GitHub `ShadowManuel`): Lead, wenig Zeit, als Einziger gute Programmierkenntnisse. Übernimmt Repo, Compose-Gerüst, Reviews, Integration, Demo (Issues L1–L4).
- **Dev A** Auth-Service (A1–A6), **Dev B** Ticket-Service (B1–B7), **Dev C** Logging-Service und README (C1–C8), **Dev D** Frontend und Semgrep (D1–D8).
- Collaborators: `SalemKo`, `Top-Mehmet`, `grepelias`, `Zakaria39-coding`; Einladung an `Babsi011096` offen. Welche Person welche Rolle hat, ist noch nicht festgelegt, die Issues sind noch niemandem zugewiesen.
- Wenig Programmiererfahrung im Team: Aufgaben klein und konkret halten, Hinweise statt fertiger Lösungen.

## Entscheidungen

- **3 Microservices** in ASP.NET Core (.NET 10, `sims.slnx`): auth-api, ticket-api, logging-api, dazu ein Web-Frontend (LLM-generiert, Technologie frei).
- **Jede Datenbank in eigenem Container und eigenem Netz:** auth-db und ticket-db (SQL Server, EF Code-First, Migrations beim Start mit Retry), redis für Logs (Redis Stream `logs`, AOF).
- **Netze:** `app` = APIs reden miteinander (aktuell nur Logs an logging-api); `auth-db`, `ticket-db`, `log-db` = je ein Service mit seiner DB. Das Frontend braucht kein Netz, der Browser ruft die APIs über die Ports 5001–5003 auf. Datenbanken haben keine `ports:`.
- **Kein JWT, keine Sessions:** Login gibt Id, Name, Rolle zurück; Admin-Funktionen blendet nur das Frontend aus. Bekannte Einschränkung für den Security-Abschnitt, JWT steht in der Roadmap.
- **Incidents:** manuelle Erfassung mit Melder (`ReporterId`, `ReporterName`) und Bearbeiter (`AssigneeId`, `AssigneeName`). Kein Foreign Key über Container-Grenzen, deshalb Id plus Namenskopie. Eskalation = Zähler 1–3. Eigene Endpoints für `/escalate` und `/close`.
- **API-Doku mit Scalar** (`AddOpenApi()`, `/scalar`).
- **Semgrep** wie in der Angabe per `docker run`, keine CI-Pipeline (Lucas schaut sich CI selbst an).
- **Nur Stufe 1:** keine Erweiterungen bauen, Ideen kommen in die Roadmap der README.
- `src/simsapi` ist das Unterrichtsbeispiel. Bleibt als Anhaltspunkt für das Team im Repo und läuft als Container auf Port 5000 mit; gehört nicht zum SIMS-System.
- Keine `TASKS.md` im Repo. Tasks leben nur als GitHub-Issues.
- Cloud-tauglich bleiben: Konfiguration über Umgebungsvariablen, kein `localhost` im Backend-Code, Services ohne Zustand.

## Arbeitsweise

- **Trunk-based:** `main` muss immer mit `docker compose up --build` starten. Kurze Branches pro Issue (z.B. `A3-login`), PR mit `Closes #n`, Review durch eine andere Person.
- Commit-Stil: `chore: ...`, `feature: ...`.
- Sprache im Projekt und mit Lucas: Deutsch.
- Vor Commits, Pushes und Änderungen an Issues oder Board nachfragen.

## Offene Punkte

- PR #34 (L1, L2) ist noch nicht gemergt.
- Vorschlag offen: Netz `app` in `api` umbenennen und den Kommentar in der Compose-Datei präzisieren.
- Vorschlag offen: Health-Endpoint `/health` in A1, B1, C1 aufnehmen.
- Auf dem Board fehlt noch die Prüfung der Workflows (Item closed / PR merged → Done).

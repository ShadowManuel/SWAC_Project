## SAST (Semgrep)

### Durchführung

Die statische Code-Analyse läuft mit [Semgrep](https://semgrep.dev) als Docker-Container, wie in der Angabe vorgesehen:

```powershell
.\sast.ps1
```

Das Skript führt `semgrep scan --config auto` über das gesamte Repository aus (ohne den Ordner `docs`) und speichert das Ergebnis in `sast/semgrep-ergebnis.txt` und `sast/semgrep-ergebnis.json`.

Zusätzlich läuft Semgrep bei jedem Push und Pull Request automatisch als GitHub Action (`.github/workflows/sast.yml`). Das Ergebnis liegt im Actions-Tab beim jeweiligen Lauf unter „Artifacts“.

### Ergebnisse

<!-- Zahlen nach dem finalen Lauf mit .\sast.ps1 aktualisieren -->

| Regel | Schweregrad | Anzahl | Betroffen |
|---|---|---|---|
| `dockerfile-source-not-pinned` | Info | 9 | alle Dockerfiles (`FROM`-Zeilen) |
| `insecure-document-method` | Warning | 1 | `src/web/index.html` |

### Bewertung und Umgang

**`dockerfile-source-not-pinned`**: Die Basis-Images werden über einen Tag (`sdk:10.0`, `aspnet:10.0`, `nginx:1.27-alpine`) statt über einen festen Hash (`@sha256:…`) eingebunden. Ein Tag kann sich ändern; im schlimmsten Fall würde ein manipuliertes Image unbemerkt übernommen (Supply-Chain-Risiko).
*Entscheidung:* bewusst akzeptiert. Die offiziellen Microsoft- und nginx-Images erhalten über den Tag automatisch Sicherheitsupdates, was für dieses Projekt wichtiger ist als reproduzierbare Builds. Für einen produktiven Einsatz würden die Images per Hash fixiert (siehe Roadmap).

**`insecure-document-method`**: Das Frontend setzt HTML über `innerHTML`. Wird dabei ungeprüfte Benutzereingabe eingefügt (z. B. ein Ticket-Titel), wäre Cross-Site-Scripting (XSS) möglich.
*Entscheidung:* geprüft, Risiko abgeschwächt. Alle Werte aus der API laufen vor dem Einfügen durch die Funktion `esc()`, die `& < > " '` in HTML-Entities umwandelt. Ein Titel wie `<script>alert(1)</script>` wird dadurch als Text angezeigt und nicht ausgeführt. Bei neuen Stellen mit `innerHTML` muss `esc()` immer verwendet werden.

### Was Semgrep nicht findet (manuelle Prüfung)

- **Passwort im Klartext in `docker-compose.yml`** (`auth-db`, `MSSQL_SA_PASSWORD`): wird nicht als Fund gemeldet, ist aber öffentlich im Repository sichtbar. Sollte wie beim Ticket-Service über die `.env`-Datei gesetzt werden.
- **Datenbank-Ports nach außen** (`auth-db`, `log-db`): sind an `127.0.0.1` gebunden und damit nur lokal erreichbar, widersprechen aber der Vorgabe, dass Datenbanken nur im eigenen Docker-Netz erreichbar sind.

# SIMS – Tasks

Leitfaden für das Team. Jeder Task wird ein GitHub-Issue. Was gebaut werden soll, steht im Projektplan (Abschnitte „Schnittstellen“ und „Datenmodell“). Wie ihr es umsetzt, entscheidet ihr selbst. Die Hinweise bei den Tasks sind Tipps, keine Pflicht.

Jeder Task hat:
- **Beschreibung:** worum es geht und warum
- **Akzeptanzkriterien:** was der Task konkret können muss
- **Testen:** welche Fälle ihr vor dem PR durchspielt
- **Hinweise:** optionale Tipps

Zusätzlich gilt für **jeden** Task die Definition of Done.

## Definition of Done

Ein Task ist erst fertig, wenn alles davon stimmt:

- [ ] Alle Akzeptanzkriterien sind erfüllt
- [ ] Alle Testfälle des Tasks sind durchgespielt (in Scalar oder im Browser), das Ergebnis steht kurz im PR
- [ ] `main` ist hereingeholt und `docker compose up --build` startet alle Container ohne Fehler
- [ ] Keine Passwörter oder andere Secrets im Code, nur in `.env`
- [ ] Der Code ist verständlich: sprechende Namen, keine auskommentierten Reste, keine kopierte Logik
- [ ] Neue Security-Maßnahmen stehen als eine Zeile im Security-Abschnitt der README
- [ ] Der PR ist reviewed und gemergt, das Issue ist geschlossen

## So arbeiten wir

### Wichtigste Regel: `main` ist immer lauffähig

Wer `main` holt und `docker compose up --build` ausführt, bekommt **alle Container ohne Fehler gestartet**. Jeden Tag holen alle `main` in ihren Branch. Ein kaputtes `main` blockiert also das ganze Team, auch Leute, die gar nichts falsch gemacht haben.

**Vor jedem Merge** führt die Person, die den PR erstellt hat, auf ihrem Branch aus:

1. `main` in den eigenen Branch holen (`git pull origin main`)
2. `docker compose up --build`
3. Prüfen:
   - [ ] Alle Container laufen, keiner ist mit einem Fehler beendet (`docker compose ps`)
   - [ ] Der eigene Service funktioniert in Scalar
   - [ ] Die anderen Services starten weiterhin

**Halbfertig ist okay, kaputt nicht.** Fehlt z.B. noch ein Endpoint, ist das kein Problem. Stürzt ein Service beim Start ab oder kompiliert etwas nicht, darf es nicht auf `main`.

**Wenn `main` doch kaputt ist:** Das Reparieren hat Vorrang vor allem anderen. Entweder sofort beheben oder den letzten Merge mit `git revert` rückgängig machen. Kurz im Teamchat Bescheid geben.

### Branches und Pull Requests

- **Ein Branch pro Task**, z.B. `A3-login`. Ein Branch lebt höchstens 1–2 Tage. Dauert ein Task länger, aufteilen.
- **Jeden Tag `main` in den eigenen Branch holen**, damit Konflikte klein bleiben.
- **Pull Request** mit Titel `A3: Login-Endpoint` und `Closes #<Issue-Nummer>` in der Beschreibung.
- **Review:** Eine andere Person aus dem Team schaut drüber und mergt. Ziel: am selben Tag gemergt. Lucas reviewt PRs, die Compose-Datei, Datenmodell oder Schnittstellen ändern.
- **Blocker** sofort im Teamchat melden.

### Niemand muss auf andere warten

- **Backend:** Jeder Service lässt sich allein über Scalar testen.
- **Logging:** Das Interface `ILogClient` kommt an Tag 2 (C2). Ab dann können Auth und Tickets ihre Log-Aufrufe schreiben. Bis der echte Client fertig ist (C5), schreibt ein Platzhalter die Einträge nur in die Konsole.
- **Frontend:** Dev D baut die Seiten zuerst mit **Mock-Daten**, also fest eingebauten Antworten wie in den JSON-Beispielen im Plan. Sobald die echte API fertig ist, wird nur die Datenquelle umgestellt.

## Zeitplan

| Tage | Phase |
|---|---|
| 1–2 | Projekte, Container, Datenbanken |
| 3–5 | Kernfunktionen |
| 6–8 | Restliche Funktionen, Logging-Aufrufe, Frontend an echte APIs |
| 9–10 | Feature-Freeze: nur Bugfixes, README, Demo-Probe |

---

## Lucas (Lead)

### L1: Repo und Solution anlegen
Tag 1 · Voraussetzung: keine

**Beschreibung:** Grundlage für alle anderen Tasks. Ohne Repo und Solution kann niemand sein Projekt anlegen, deshalb am ersten Vormittag erledigen.

**Akzeptanzkriterien:**
- [ ] GitHub-Repo mit `.gitignore` für .NET (`dotnet new gitignore`)
- [ ] Solution `sims.slnx` im Repo-Root, Ordner `src/` für die Services, Unterrichtsbeispiel unter `src/simsapi`
- [ ] Alle Teammitglieder haben Schreibrechte
- [ ] Project-Board mit den Spalten Todo, In Arbeit, Review, Fertig; alle Tasks als Issues angelegt

**Testen:**
- [ ] Ein Teammitglied klont das Repo und öffnet die Solution ohne Fehler

### L2: Compose-Grundgerüst
Tag 1 · Voraussetzung: L1

**Beschreibung:** Die Compose-Datei legt fest, welche Container es gibt und wer mit wem reden darf. L2 legt nur Netze und Volumes an, die Services tragen die Devs in ihren Setup-Tasks selbst ein.

**Akzeptanzkriterien:**
- [ ] `docker-compose.yml` im Repo-Root mit den Netzen `app`, `auth-db`, `ticket-db`, `log-db` und den Volumes (siehe Vorlage unten)
- [ ] `.env.example` mit allen Variablen, Passwörter als Platzhalter; `.env` steht in `.gitignore`
- [ ] Kurze Anleitung „Projekt starten“ in der README

**Testen:**
- [ ] `docker compose config` meldet keine Fehler

### L3: Reviews und Unterstützung
Tag 3–8

**Beschreibung:** Damit die Teile zusammenpassen, schaut Lucas sich die Änderungen an, die alle betreffen.

**Akzeptanzkriterien:**
- [ ] PRs zu Compose, Datenmodell und Schnittstellen reviewt
- [ ] Blocker im Teamchat beantwortet

### L4: Integration und Demo
Tag 9–10 · Voraussetzung: alle Features gemergt

**Beschreibung:** Am Ende muss alles auf einem fremden Rechner laufen und die Demo sitzen.

**Akzeptanzkriterien:**
- [ ] Demo-Ablauf geschrieben: Login → Incident anlegen → eskalieren → schließen → User anlegen und deaktivieren → Logs ansehen
- [ ] Demo einmal mit dem Team geprobt

**Testen:**
- [ ] Repo frisch klonen, `.env` aus `.env.example` anlegen, `docker compose up --build`, kompletten Demo-Ablauf durchklicken
- [ ] Container neu starten: Users, Incidents und Logs sind noch da

---

## Dev A: Auth-Service

### A1: Projekt und Container anlegen
Tag 1–2 · Voraussetzung: L1, L2

**Beschreibung:** Der Auth-Service bekommt sein eigenes Projekt, seinen Container und seine eigene SQL-Server-Datenbank in einem eigenen Netz.

**Akzeptanzkriterien:**
- [ ] Web-API-Projekt `src/Sims.AuthService` mit Controllern, zur Solution hinzugefügt
- [ ] Pakete: `Microsoft.EntityFrameworkCore.SqlServer`, `Microsoft.EntityFrameworkCore.Design`, `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`
- [ ] Scalar erreichbar unter `http://localhost:5001/scalar`
- [ ] Dockerfile (Vorlage unten)
- [ ] `auth-api` und `auth-db` in der `docker-compose.yml`; `auth-db` nur im Netz `auth-db`, ohne `ports:`
- [ ] CORS für `http://localhost:8080` freigegeben

**Testen:**
- [ ] `docker compose up --build`: `auth-api` und `auth-db` laufen
- [ ] `http://localhost:5001/scalar` öffnet sich

**Hinweise:**
```
dotnet new webapi -n Sims.AuthService -o src/Sims.AuthService --use-controllers
dotnet sln add src/Sims.AuthService
```

### A2: User-Modell und Datenbank
Tag 2–3 · Voraussetzung: A1

**Beschreibung:** Die User-Tabelle mit EF Code-First. Die Datenbank soll sich beim Start selbst anlegen, damit niemand manuell SQL ausführen muss.

**Akzeptanzkriterien:**
- [ ] Klasse `User` laut Datenmodell, `Role` als Enum
- [ ] `AuthDbContext`, `Username` ist eindeutig (Unique Index)
- [ ] Erste Migration angelegt
- [ ] Migration wird beim Start automatisch angewendet, mit Retry, weil SQL Server beim Hochfahren 10–30 Sekunden braucht
- [ ] Seed: ein Admin-User mit gehashtem Passwort, Zugangsdaten in `.env.example` dokumentiert

**Testen:**
- [ ] Mit leerem Volume starten (`docker compose down -v`, dann `up`): Service startet, auch wenn die DB langsam hochfährt
- [ ] Zweiter Start: kein Fehler, kein doppelter Admin

**Hinweise:** `options.UseSqlServer(connectionString, sql => sql.EnableRetryOnFailure())` und `db.Database.Migrate()` beim Start. Für das Hashing bietet sich `PasswordHasher<User>` aus `Microsoft.AspNetCore.Identity` an (Paket `Microsoft.Extensions.Identity.Core`).

### A3: Login
Tag 3–4 · Voraussetzung: A2

**Beschreibung:** Das Frontend schickt Benutzername und Passwort und bekommt Id, Name und Rolle zurück. Die Fehlermeldung darf nicht verraten, ob es einen Usernamen gibt.

**Akzeptanzkriterien:**
- [ ] `POST /api/auth/login` wie im Plan
- [ ] Passwort wird mit dem gespeicherten Hash verglichen
- [ ] Unbekannter User, deaktivierter User und falsches Passwort geben dieselbe `401`-Meldung
- [ ] Leere Felder geben `400`
- [ ] Der Passwort-Hash steht in keiner Antwort

**Testen:**
- [ ] Admin mit richtigem Passwort → `200` mit Id, Name, Rolle
- [ ] Admin mit falschem Passwort → `401`
- [ ] Unbekannter Username → `401`, gleiche Meldung wie oben
- [ ] Leerer Username oder leeres Passwort → `400`

### A4: User verwalten
Tag 4–5 · Voraussetzung: A2

**Beschreibung:** Admins legen User an, sehen alle User und können sie deaktivieren. Gelöscht wird nie, damit alte Incidents nachvollziehbar bleiben.

**Akzeptanzkriterien:**
- [ ] `GET /api/users`
- [ ] `POST /api/users` mit Validierung (Passwort mindestens 8 Zeichen, gültige Rolle), `409` wenn der Username vergeben ist
- [ ] `PATCH /api/users/{id}/disable`, `404` wenn der User nicht existiert
- [ ] Datenzugriff über ein Repository oder einen Service, nicht direkt im Controller

**Testen:**
- [ ] User anlegen → `201`, erscheint in `GET /api/users`, Antwort ohne Hash
- [ ] Gleichen Username nochmal → `409`
- [ ] Passwort mit 5 Zeichen → `400`; Rolle `"Chef"` → `400`
- [ ] User deaktivieren → `204`; Login mit diesem User → `401`
- [ ] Unbekannte Id deaktivieren → `404`

### A5: Ereignisse loggen
Tag 6–7 · Voraussetzung: C2 (Interface), für echtes Senden C5

**Beschreibung:** Sicherheitsrelevante Ereignisse sollen im Log landen, damit Admins nachvollziehen können, wer sich wann angemeldet hat.

**Akzeptanzkriterien:**
- [ ] Login erfolgreich (`Info`), Login fehlgeschlagen (`Warning`, nur Username, nie das Passwort), User angelegt und deaktiviert (`Info`)
- [ ] Wenn die logging-api nicht erreichbar ist, läuft der Auth-Service trotzdem weiter

**Testen:**
- [ ] Erfolgreicher und fehlgeschlagener Login → beide Einträge in `GET /api/logs`
- [ ] `docker compose stop logging-api`, dann einloggen → Login funktioniert weiterhin

### A6: README-Abschnitt Auth
Tag 8–9

**Beschreibung:** Jede Person dokumentiert ihren eigenen Teil, Dev C fügt alles zusammen.

**Akzeptanzkriterien:**
- [ ] Features und Endpoints des Auth-Service
- [ ] Security-Maßnahmen: Hashing, einheitliche Login-Meldung, keine Hashes in Antworten, eigenes DB-Netz
- [ ] Bekannte Einschränkung: keine Token, Admin-Prüfung nur im Frontend

---

## Dev B: Ticket-Service

### B1: Projekt und Container anlegen
Tag 1–2 · Voraussetzung: L1, L2

**Beschreibung:** Der Ticket-Service bekommt sein eigenes Projekt, seinen Container und seine eigene SQL-Server-Datenbank in einem eigenen Netz.

**Akzeptanzkriterien:**
- [ ] Web-API-Projekt `src/Sims.TicketService` mit Controllern, zur Solution hinzugefügt
- [ ] Gleiche Pakete wie A1
- [ ] Scalar unter `http://localhost:5002/scalar`
- [ ] Dockerfile (Vorlage unten)
- [ ] `ticket-api` und `ticket-db` in der `docker-compose.yml`; `ticket-db` nur im Netz `ticket-db`, ohne `ports:`
- [ ] CORS für `http://localhost:8080` freigegeben

**Testen:**
- [ ] `docker compose up --build`: `ticket-api` und `ticket-db` laufen
- [ ] `http://localhost:5002/scalar` öffnet sich

### B2: Incident-Modell und Datenbank
Tag 2–3 · Voraussetzung: B1

**Beschreibung:** Die Incident-Tabelle mit EF Code-First. Melder und Bearbeiter werden als Id und Name gespeichert, weil die User in einer anderen Datenbank liegen.

**Akzeptanzkriterien:**
- [ ] Klasse `Incident` laut Datenmodell, `Severity` und `Status` als Enums
- [ ] Enums werden als Text gespeichert und als Text im JSON ausgegeben (`"High"`, nicht `2`)
- [ ] `TicketDbContext`, erste Migration, beim Start automatisch angewendet (mit Retry wie in A2)
- [ ] Seed: 3–5 realistische Beispiel-Incidents für die Demo

**Testen:**
- [ ] Mit leerem Volume starten: Tabelle wird angelegt, Seed-Daten sind da
- [ ] Zweiter Start: kein Fehler, keine doppelten Seed-Daten

**Hinweise:** Enums als Text im JSON mit `JsonStringEnumConverter` in `AddControllers().AddJsonOptions(...)`, in der DB mit `.HasConversion<string>()`.

### B3: Incidents lesen
Tag 3 · Voraussetzung: B2

**Beschreibung:** Die Liste und die Detailansicht im Frontend brauchen diese beiden Endpoints.

**Akzeptanzkriterien:**
- [ ] `GET /api/incidents`, neueste zuerst
- [ ] `GET /api/incidents/{id}`, `404` wenn nicht vorhanden

**Testen:**
- [ ] Liste liefert die Seed-Daten, neueste zuerst, Enums als Text
- [ ] Vorhandene Id → `200`; Id 9999 → `404`

### B4: Incident anlegen und bearbeiten
Tag 4–5 · Voraussetzung: B3

**Beschreibung:** Das Kern-Feature laut Angabe: Vorfälle manuell erfassen und bearbeiten.

**Akzeptanzkriterien:**
- [ ] `POST /api/incidents`: Status startet mit `Open`, Eskalationsstufe mit 1, `createdAt` setzt der Server
- [ ] `PUT /api/incidents/{id}`: Status nur `Open` oder `InProgress`; `409` wenn der Incident geschlossen ist
- [ ] Validierung laut Plan: Pflichtfelder, CVE-Format `CVE-JJJJ-NNNN`
- [ ] Eigene Request-Klassen (DTOs) statt der Entity direkt im Controller

**Testen:**
- [ ] Gültiger Incident → `201`, Status `Open`, Stufe 1
- [ ] Ohne Titel → `400`; CVE `"abc"` → `400`; ohne CVE → `201`
- [ ] Bearbeiten mit Status `InProgress` und Bearbeiter → `200`, Änderung in `GET` sichtbar
- [ ] Bearbeiten mit Status `Closed` → `400`
- [ ] Geschlossenen Incident bearbeiten → `409`

### B5: Eskalieren und schließen
Tag 5–6 · Voraussetzung: B3

**Beschreibung:** Eskalation und Schließen sind eigene Aktionen mit eigenen Regeln, deshalb eigene Endpoints.

**Akzeptanzkriterien:**
- [ ] `POST /api/incidents/{id}/escalate`: Stufe +1, `409` bei geschlossenem Incident oder Stufe 3
- [ ] `POST /api/incidents/{id}/close`: Status `Closed`, `closedAt` setzt der Server, `409` wenn schon geschlossen

**Testen:**
- [ ] Neuen Incident zweimal eskalieren → Stufe 3; dritter Versuch → `409`
- [ ] Schließen → `200`, Status `Closed`, `closedAt` gesetzt
- [ ] Nochmal schließen → `409`; geschlossenen eskalieren → `409`
- [ ] Unbekannte Id → `404`

### B6: Ereignisse loggen
Tag 6–7 · Voraussetzung: C2 (Interface), für echtes Senden C5

**Beschreibung:** Jede Änderung an einem Incident soll im Log nachvollziehbar sein.

**Akzeptanzkriterien:**
- [ ] Incident angelegt, bearbeitet, geschlossen (`Info`), eskaliert (`Warning`)
- [ ] Wenn die logging-api nicht erreichbar ist, läuft der Ticket-Service trotzdem weiter

**Testen:**
- [ ] Anlegen, eskalieren, schließen → drei passende Einträge in `GET /api/logs`
- [ ] `docker compose stop logging-api`, dann Incident anlegen → funktioniert weiterhin

### B7: README-Abschnitt Tickets
Tag 8–9

**Beschreibung:** Jede Person dokumentiert ihren eigenen Teil, Dev C fügt alles zusammen.

**Akzeptanzkriterien:**
- [ ] Features und Endpoints des Ticket-Service, Eskalationsregeln
- [ ] Erklärung, warum Melder und Bearbeiter nur als Id und Name gespeichert werden (kein Foreign Key über Container-Grenzen)
- [ ] Security-Maßnahmen: EF statt SQL-Strings, Validierung, eigenes DB-Netz

---

## Dev C: Logging-Service und README

### C1: Projekt und Container anlegen
Tag 1–2 · Voraussetzung: L1, L2

**Beschreibung:** Der Logging-Service bekommt sein eigenes Projekt und speichert in Redis, das nur er erreichen kann.

**Akzeptanzkriterien:**
- [ ] Web-API-Projekt `src/Sims.LoggingService` mit Controllern, zur Solution hinzugefügt
- [ ] Pakete: `StackExchange.Redis`, `Microsoft.AspNetCore.OpenApi`, `Scalar.AspNetCore`
- [ ] Scalar unter `http://localhost:5003/scalar`
- [ ] Dockerfile (Vorlage unten)
- [ ] `logging-api` und `redis` in der `docker-compose.yml`; Redis mit AOF, Passwort und Volume, nur im Netz `log-db`, ohne `ports:`
- [ ] CORS für `http://localhost:8080` freigegeben

**Testen:**
- [ ] `docker compose up --build`: `logging-api` und `redis` laufen
- [ ] `http://localhost:5003/scalar` öffnet sich

### C2: Interface für den Log-Client
Tag 2 · Voraussetzung: L1

**Beschreibung:** Auth und Tickets sollen ihre Log-Aufrufe schreiben können, bevor der Logging-Service fertig ist. Dafür gibt es früh ein gemeinsames Interface.

**Akzeptanzkriterien:**
- [ ] Klassenbibliothek `src/Sims.Shared`, zur Solution hinzugefügt
- [ ] Interface `ILogClient` mit einer Methode zum Senden (Level, Service, Message)
- [ ] Platzhalter `ConsoleLogClient`, der die Einträge nur in die Konsole schreibt
- [ ] Kurze Anleitung für Dev A und B: Projekt-Referenz hinzufügen, registrieren, aufrufen

**Testen:**
- [ ] Ein Service mit Referenz auf `Sims.Shared` baut weiterhin im Docker-Container (Build-Kontext ist der Repo-Root)

### C3: Log-Speicher in Redis
Tag 3 · Voraussetzung: C1

**Beschreibung:** Logs werden in einem Redis Stream gespeichert. Der Stream vergibt Ids mit Zeitstempel und kann von hinten gelesen werden.

**Akzeptanzkriterien:**
- [ ] Klasse `LogEntry` (Id, Timestamp, Level, Service, Message)
- [ ] `LogRepository` schreibt in den Stream `logs` und liest die neuesten Einträge
- [ ] Stream auf etwa 10.000 Einträge begrenzt
- [ ] Redis-Verbindung als Singleton registriert

**Testen:**
- [ ] Eintrag schreiben und wieder lesen
- [ ] `docker compose restart redis` → Einträge sind noch da (AOF)

### C4: Log-Endpoints
Tag 4 · Voraussetzung: C3

**Beschreibung:** Die anderen Services schreiben über die API, das Frontend liest darüber.

**Akzeptanzkriterien:**
- [ ] `POST /api/logs`: Id und Timestamp vergibt der Service, `400` bei leerem Feld oder ungültigem Level
- [ ] `GET /api/logs?limit=100`: neueste zuerst, Standard 100

**Testen:**
- [ ] Drei Einträge schreiben → `GET` liefert sie in umgekehrter Reihenfolge
- [ ] Level `"Debug"` → `400`; leere Message → `400`
- [ ] `limit=2` → genau zwei Einträge

### C5: Echter Log-Client
Tag 5 · Voraussetzung: C2, C4

**Beschreibung:** Ersetzt den Platzhalter aus C2 durch einen Client, der wirklich an die logging-api sendet.

**Akzeptanzkriterien:**
- [ ] `HttpLogClient` in `Sims.Shared` sendet per HTTP an die Adresse aus der Konfiguration (`Logging__Url`)
- [ ] Fehler beim Senden werden abgefangen; der aufrufende Service stürzt nie wegen des Loggings ab
- [ ] Auth und Tickets nutzen den neuen Client (nur Registrierung ändern)

**Testen:**
- [ ] Aus auth-api oder ticket-api geloggte Einträge erscheinen in `GET /api/logs`
- [ ] `docker compose stop logging-api` → der aufrufende Service läuft weiter

### C6: README-Gerüst
Tag 1–3

**Beschreibung:** Die README zählt einen eigenen Punkt. Mit einem frühen Gerüst wissen alle, wo ihr Abschnitt hinkommt.

**Akzeptanzkriterien:**
- [ ] Alle Pflichtabschnitte als Überschriften: Beschreibung, Features, Architektur, Installation, Systemvoraussetzungen (OS, .NET 10, Docker), Version, Lizenz, Mitwirkende, Roadmap, ER-Diagramm, Klassendiagramm, Security, SAST-Ergebnisse, Link zum Repo
- [ ] Badges von badgen.net (z.B. Version, Lizenz, .NET)
- [ ] Die Projektaufgabe selbst wird nicht dokumentiert

### C7: Diagramme und Roadmap
Tag 6–8

**Beschreibung:** Die Angabe verlangt ER-Diagramm, Klassendiagramm (wenn sinnvoll) und eine fiktive Roadmap.

**Akzeptanzkriterien:**
- [ ] ER-Diagramm für beide SQL-Datenbanken (z.B. Mermaid direkt in der README)
- [ ] Beschreibung der Redis-Datenstruktur (Key, Typ, Felder, Begrenzung, Persistenz)
- [ ] Architekturdiagramm mit Containern und Netzen
- [ ] Roadmap: JWT, automatische Incidents von Geräten, Jira-Anbindung, Eskalationshistorie, HTTPS

**Testen:**
- [ ] Diagramme werden auf GitHub richtig angezeigt

### C8: README fertigstellen
Tag 9–10 · Voraussetzung: A6, B7, D8

**Beschreibung:** Alle Abschnitte zu einer einheitlichen README zusammenführen.

**Akzeptanzkriterien:**
- [ ] Abschnitte von A, B und D eingesammelt und vereinheitlicht
- [ ] Logging-Abschnitt mit Security-Maßnahmen (Redis nur im eigenen Netz, Passwort, keine Passwörter in Logs)
- [ ] Einmal komplett gegengelesen

**Testen:**
- [ ] Jemand, der nicht im Team ist, kann das Projekt nur mit der README starten

---

## Dev D: Frontend und SAST

### D1: Semgrep einrichten
Tag 1–2

**Beschreibung:** Semgrep ist Pflicht laut Angabe. Den Befehl früh ausprobieren, damit er am Ende sofort funktioniert und jeder ihn vor einem PR laufen lassen kann.

**Akzeptanzkriterien:**
- [ ] Befehl aus der Angabe im Repo-Ordner ausgeführt:
  `docker run --rm -v "${PWD}:/src" returntocorp/semgrep semgrep --config auto`
- [ ] Kurze Anleitung im Teamchat

### D2: Frontend-Projekt und Container
Tag 2–3 · Voraussetzung: L2

**Beschreibung:** Das Frontend läuft als eigener Container. Die Technologie ist frei, die Angabe erlaubt LLM-generierten Code.

**Akzeptanzkriterien:**
- [ ] Frontend-Projekt in `src/web`, z.B. HTML/JavaScript oder React mit Vite
- [ ] Dockerfile, das die Seite über nginx ausliefert
- [ ] `web` in der `docker-compose.yml`, erreichbar unter `http://localhost:8080`
- [ ] API-Adressen an einer Stelle konfiguriert (5001, 5002, 5003)
- [ ] Mock-Daten wie in den JSON-Beispielen im Plan, an einer Stelle umschaltbar auf die echten APIs

**Testen:**
- [ ] `http://localhost:8080` zeigt die Startseite

### D3: Login
Tag 3–4 · Echte API: A3

**Beschreibung:** Nach dem Login merkt sich das Frontend den User und zeigt Admin-Funktionen nur Admins.

**Akzeptanzkriterien:**
- [ ] Login-Formular, Fehlermeldung bei `401`
- [ ] Nach dem Login werden Id, Name und Rolle gemerkt (z.B. `sessionStorage`)
- [ ] Logout-Button
- [ ] Admin-Menüpunkte nur für die Rolle `Admin` sichtbar

**Testen:**
- [ ] Login als Admin → Admin-Menü sichtbar; als User → nicht sichtbar
- [ ] Falsches Passwort → Fehlermeldung, kein Login
- [ ] Logout → zurück zum Login, Seiten ohne Login nicht erreichbar

### D4: Incidents anzeigen
Tag 4–5 · Echte API: B3

**Beschreibung:** Die Hauptseite des Systems: alle Vorfälle auf einen Blick.

**Akzeptanzkriterien:**
- [ ] Liste mit Titel, Schweregrad, Status, Stufe, Bearbeiter, Datum
- [ ] Schweregrad und Status farblich erkennbar
- [ ] Detailansicht eines Incidents

**Testen:**
- [ ] Liste zeigt alle Incidents, neueste oben
- [ ] Klick auf einen Incident öffnet die Details

### D5: Incidents bearbeiten
Tag 5–7 · Echte API: B4, B5, A4

**Beschreibung:** Anlegen, Bearbeiten, Eskalieren und Schließen über die Oberfläche.

**Akzeptanzkriterien:**
- [ ] Formular zum Anlegen; Melder ist automatisch der angemeldete User
- [ ] Formular zum Bearbeiten; Bearbeiter aus einer Auswahlliste der User (`GET /api/users`)
- [ ] Buttons „Eskalieren“ und „Schließen“, deaktiviert wenn nicht erlaubt
- [ ] Fehlermeldungen der API (`{ "error": ... }`) werden angezeigt

**Testen:**
- [ ] Incident anlegen → erscheint in der Liste mit dem eigenen Namen als Melder
- [ ] Ohne Titel absenden → Fehlermeldung
- [ ] Eskalieren bis Stufe 3 → Button wird deaktiviert
- [ ] Schließen → Status ändert sich, Bearbeiten nicht mehr möglich

### D6: User-Verwaltung
Tag 6–7 · Echte API: A4

**Beschreibung:** Admin-Seite zum Anlegen und Deaktivieren von Usern.

**Akzeptanzkriterien:**
- [ ] Liste der User mit Rolle und Status (aktiv oder deaktiviert)
- [ ] Formular zum Anlegen
- [ ] Button zum Deaktivieren

**Testen:**
- [ ] User anlegen → erscheint in der Liste; Login mit ihm funktioniert
- [ ] Vergebenen Username anlegen → Fehlermeldung
- [ ] User deaktivieren → Status ändert sich; Login mit ihm schlägt fehl

### D7: Log-Ansicht
Tag 7–8 · Echte API: C4

**Beschreibung:** Admin-Seite, die zeigt, was im System passiert ist.

**Akzeptanzkriterien:**
- [ ] Tabelle mit Zeit, Level, Service, Nachricht, neueste zuerst
- [ ] Level farblich erkennbar

**Testen:**
- [ ] Einloggen, Incident anlegen, Log-Seite öffnen → beide Ereignisse oben in der Liste

### D8: Semgrep-Lauf und Auswertung
Tag 9 · Voraussetzung: alle Features gemergt

**Beschreibung:** Der SAST-Punkt verlangt den Lauf und dokumentierte Ergebnisse.

**Akzeptanzkriterien:**
- [ ] Semgrep über den fertigen Code laufen lassen
- [ ] Findings gesichtet: einfache selbst behoben, den Rest an die zuständige Person gegeben
- [ ] README-Abschnitt „SAST-Ergebnisse“: Anzahl, wichtigste Findings, was damit passiert ist
- [ ] README-Abschnitt Frontend: Seiten und Bedienung, gern mit Screenshots

**Testen:**
- [ ] Zweiter Semgrep-Lauf nach den Fixes: keine neuen Findings

---

## Vorlagen

### Dockerfile für einen .NET-Service

Liegt im Projektordner, gebaut wird aber vom Repo-Root aus (wegen `Sims.Shared`). Für die anderen Services `AuthService` entsprechend ersetzen.

```dockerfile
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Sims.AuthService/Sims.AuthService.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "Sims.AuthService.dll"]
```

### docker-compose.yml

L2 legt die Netze und Volumes an. Die Services tragen die Devs in ihren Setup-Tasks ein.

```yaml
services:
  auth-api:                          # A1
    build:
      context: .
      dockerfile: src/Sims.AuthService/Dockerfile
    ports: ["5001:8080"]
    environment:
      ConnectionStrings__Default: "Server=auth-db,1433;Database=SimsAuth;User Id=sa;Password=${AUTH_DB_PASSWORD};TrustServerCertificate=True"
      Logging__Url: "http://logging-api:8080"
    networks: [app, auth-db]
    depends_on: [auth-db]

  auth-db:                           # A1
    image: mcr.microsoft.com/mssql/server:2022-latest
    environment:
      ACCEPT_EULA: "Y"
      MSSQL_SA_PASSWORD: ${AUTH_DB_PASSWORD}
      MSSQL_MEMORY_LIMIT_MB: "1024"
    volumes: [auth-db-data:/var/opt/mssql]
    networks: [auth-db]

  # ticket-api, ticket-db           (B1, analog zu Auth, Port 5002)
  # logging-api, redis              (C1, Port 5003)
  # web                             (D2, Port 8080)

networks:
  app:
  auth-db:
  ticket-db:
  log-db:

volumes:
  auth-db-data:
  ticket-db-data:
  redis-data:
```

Die Datenbanken bekommen keinen `ports:`-Eintrag. Sie sind nur im eigenen Netz erreichbar.

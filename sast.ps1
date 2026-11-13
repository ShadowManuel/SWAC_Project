# SAST mit Semgrep (laut Angabe als Docker-Container)
# Aufruf im Repo-Hauptordner:   .\sast.ps1
# Ergebnis: sast\semgrep-ergebnis.txt (lesbar) und sast\semgrep-ergebnis.json (maschinenlesbar)

$ErrorActionPreference = "Stop"
$repo = (Get-Location).Path

if (-not (Test-Path "$repo\docker-compose.yml")) {
    Write-Host "Bitte im Hauptordner des Repos ausführen (dort, wo die docker-compose.yml liegt)." -ForegroundColor Red
    exit 1
}

New-Item -ItemType Directory -Force -Path "$repo\sast" | Out-Null

Write-Host "Semgrep läuft, das dauert beim ersten Mal etwas länger ..." -ForegroundColor Cyan

docker run --rm -v "${repo}:/src" semgrep/semgrep semgrep scan `
    --config auto `
    --exclude docs `
    --text --output /src/sast/semgrep-ergebnis.txt `
    --json-output=/src/sast/semgrep-ergebnis.json

Write-Host ""
Write-Host "Fertig. Ergebnis: sast\semgrep-ergebnis.txt" -ForegroundColor Green
Get-Content "$repo\sast\semgrep-ergebnis.txt" | Select-Object -Last 15

# Pana — Ship to Production
# Usage: .\ship.ps1
# Runs build → test → deploy to VPS

$ErrorActionPreference = "Stop"

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  PANA — Ship to Production" -ForegroundColor Cyan
Write-Host "========================================`n" -ForegroundColor Cyan

# ── Gate 1: Build ──────────────────────────────────────
Write-Host "[1/3] Building..." -ForegroundColor Yellow
dotnet build src/Pana.sln
if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ BUILD FAILED. Fix errors before shipping.`n" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Build passed`n" -ForegroundColor Green

# ── Gate 2: Tests ──────────────────────────────────────
Write-Host "[2/3] Running tests..." -ForegroundColor Yellow
dotnet test src/Pana.Tests
if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ TESTS FAILED. Fix failing tests before shipping.`n" -ForegroundColor Red
    exit 1
}
Write-Host "✅ Tests passed`n" -ForegroundColor Green

# ── Gate 3: Deploy ─────────────────────────────────────
Write-Host "[3/3] Deploying to production..." -ForegroundColor Yellow
ssh root@198.58.111.94 "cd /home/pana/app && git pull && docker compose -f deploy/docker-compose.prod.yml up -d --build api"
if ($LASTEXITCODE -ne 0) {
    Write-Host "`n❌ DEPLOY FAILED. Check SSH connection and Docker logs.`n" -ForegroundColor Red
    exit 1
}

Write-Host "`n========================================" -ForegroundColor Green
Write-Host "  ✅ SHIPPED SUCCESSFULLY" -ForegroundColor Green
Write-Host "========================================`n" -ForegroundColor Green
Write-Host "App running at: http://198.58.111.94`n" -ForegroundColor Cyan

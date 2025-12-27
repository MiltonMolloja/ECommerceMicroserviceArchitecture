# Script maestro para migración completa
# 1. Trunca tablas
# 2. Aplica fixes de schema
# 3. Importa datos

$PgHost = "72.61.128.126"
$PgPort = "5433"
$PgDatabase = "postgres"
$PgUsername = "postgres"
$PgPassword = "3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2"

Write-Host "============================================" -ForegroundColor Magenta
Write-Host "PostgreSQL Full Migration" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host ""

# Step 1: Truncate tables
Write-Host "Step 1: Truncating all tables..." -ForegroundColor Yellow
powershell -ExecutionPolicy Bypass -Command {
    param($h, $p, $d, $u, $pw)
    $tempDir = "$env:TEMP\pg-truncate-$(Get-Random)"
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    Push-Location $tempDir
    dotnet new console -n PgTruncate -f net9.0 2>&1 | Out-Null
    Set-Location PgTruncate
    dotnet add package Npgsql 2>&1 | Out-Null
    
    $code = @"
using System;
using System.IO;
using Npgsql;

var connectionString = "Host=$h;Port=$p;Database=$d;Username=$u;Password=$pw";
var sqlScript = File.ReadAllText(@"C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\00a-truncate-all-tables.sql");

try {
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    using var cmd = new NpgsqlCommand(sqlScript, conn);
    cmd.CommandTimeout = 300;
    cmd.ExecuteNonQuery();
    Console.WriteLine("SUCCESS");
} catch (Exception ex) {
    Console.WriteLine($"ERROR: {ex.Message}");
    Environment.Exit(1);
}
"@
    $code | Out-File -FilePath "Program.cs" -Encoding UTF8
    $output = dotnet run --verbosity quiet 2>&1
    Pop-Location
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
    if ($output -match "SUCCESS") { Write-Host "  ✅ Tables truncated" -ForegroundColor Green } else { Write-Host "  ❌ Error: $output" -ForegroundColor Red; exit 1 }
} -ArgumentList $PgHost, $PgPort, $PgDatabase, $PgUsername, $PgPassword

# Step 2: Apply schema fixes
Write-Host "Step 2: Applying schema fixes..." -ForegroundColor Yellow
& "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\01b-fix-schema.ps1"
if ($LASTEXITCODE -ne 0) { exit 1 }

# Step 3: Import data
Write-Host "Step 3: Importing data..." -ForegroundColor Yellow
& "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\02-import-data.ps1"

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "Migration Complete!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Magenta

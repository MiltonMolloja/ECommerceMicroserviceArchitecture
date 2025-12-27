# Script para aplicar fixes al schema de PostgreSQL
# Ajusta el schema para que coincida con los datos exportados de SQL Server

$PgHost = "72.61.128.126"
$PgPort = "5433"
$PgDatabase = "postgres"
$PgUsername = "postgres"
$PgPassword = "3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2"
$ScriptPath = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\00-fix-schema-for-migration.sql"

Write-Host "============================================" -ForegroundColor Magenta
Write-Host "PostgreSQL Schema Fixes" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host ""
Write-Host "Target: $PgHost`:$PgPort" -ForegroundColor Cyan
Write-Host "Database: $PgDatabase" -ForegroundColor Cyan
Write-Host "Script: $ScriptPath" -ForegroundColor Cyan
Write-Host ""

# Verificar que el archivo existe
if (-not (Test-Path $ScriptPath)) {
    Write-Host "❌ Error: Script file not found at $ScriptPath" -ForegroundColor Red
    exit 1
}

# Crear un proyecto temporal de .NET para ejecutar el script
$tempDir = "$env:TEMP\pg-fix-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

Push-Location $tempDir

try {
    Write-Host "Creating temporary .NET project..." -ForegroundColor Yellow
    
    # Crear proyecto de consola
    dotnet new console -n PgFix -f net9.0 | Out-Null
    Set-Location PgFix
    
    # Agregar Npgsql
    dotnet add package Npgsql | Out-Null
    
    # Crear el código que lee el archivo SQL
    $code = @"
using System;
using System.IO;
using Npgsql;

var connectionString = "Host=$PgHost;Port=$PgPort;Database=$PgDatabase;Username=$PgUsername;Password=$PgPassword";
var sqlScriptPath = @"$ScriptPath";

try
{
    if (!File.Exists(sqlScriptPath))
    {
        Console.WriteLine($"❌ Error: SQL script file not found at {sqlScriptPath}");
        Environment.Exit(1);
    }
    
    var sqlScript = File.ReadAllText(sqlScriptPath);
    
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    
    Console.WriteLine("✅ Connected to PostgreSQL successfully!");
    Console.WriteLine();
    Console.WriteLine("Applying schema fixes...");
    Console.WriteLine();
    
    using var cmd = new NpgsqlCommand(sqlScript, conn);
    cmd.CommandTimeout = 300; // 5 minutes timeout
    
    var rowsAffected = cmd.ExecuteNonQuery();
    
    Console.WriteLine();
    Console.WriteLine("✅ Schema fixes applied successfully!");
    Console.WriteLine();
    Console.WriteLine("============================================");
    Console.WriteLine("Schema is now ready for data import! ✅");
    Console.WriteLine("============================================");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine();
    Console.WriteLine("Stack trace:");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}
"@

    $code | Out-File -FilePath "Program.cs" -Encoding UTF8
    
    # Ejecutar
    Write-Host "Executing schema fixes..." -ForegroundColor Yellow
    Write-Host ""
    dotnet run --verbosity quiet
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Schema fixes completed successfully!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Schema fixes failed!" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
    exit 1
}
finally {
    Pop-Location
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}

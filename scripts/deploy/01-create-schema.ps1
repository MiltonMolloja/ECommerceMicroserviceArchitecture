# Script para crear el schema completo en PostgreSQL
# Ejecuta el script 00-create-all-tables-postgres.sql

$PgHost = "72.61.128.126"
$PgPort = "5433"
$PgDatabase = "postgres"
$PgUsername = "postgres"
$PgPassword = "3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2"
$ScriptPath = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\00-create-all-tables-postgres.sql"

Write-Host "============================================" -ForegroundColor Magenta
Write-Host "PostgreSQL Schema Creation" -ForegroundColor Magenta
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
$tempDir = "$env:TEMP\pg-schema-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

Push-Location $tempDir

try {
    Write-Host "Creating temporary .NET project..." -ForegroundColor Yellow
    
    # Crear proyecto de consola
    dotnet new console -n PgSchema -f net9.0 | Out-Null
    Set-Location PgSchema
    
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
    Console.WriteLine("Executing schema creation script...");
    Console.WriteLine();
    
    using var cmd = new NpgsqlCommand(sqlScript, conn);
    cmd.CommandTimeout = 300; // 5 minutes timeout
    
    var rowsAffected = cmd.ExecuteNonQuery();
    
    Console.WriteLine();
    Console.WriteLine("✅ Schema creation completed successfully!");
    Console.WriteLine();
    
    // Verify schemas created
    Console.WriteLine("Verifying schemas...");
    using var verifyCmd = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast') ORDER BY schema_name", conn);
    using var reader = verifyCmd.ExecuteReader();
    
    Console.WriteLine();
    Console.WriteLine("Schemas created:");
    while (reader.Read())
    {
        Console.WriteLine($"  ✓ {reader.GetString(0)}");
    }
    reader.Close();
    
    // Count tables
    using var countCmd = new NpgsqlCommand("SELECT COUNT(*) FROM information_schema.tables WHERE table_schema NOT IN ('pg_catalog', 'information_schema')", conn);
    var tableCount = countCmd.ExecuteScalar();
    
    Console.WriteLine();
    Console.WriteLine($"Total tables created: {tableCount}");
    Console.WriteLine();
    Console.WriteLine("============================================");
    Console.WriteLine("Schema creation complete! ✅");
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
    Write-Host "Executing schema creation..." -ForegroundColor Yellow
    Write-Host ""
    dotnet run --verbosity quiet
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host ""
        Write-Host "✅ Schema creation completed successfully!" -ForegroundColor Green
    } else {
        Write-Host ""
        Write-Host "❌ Schema creation failed!" -ForegroundColor Red
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

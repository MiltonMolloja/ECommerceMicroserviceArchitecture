# Script simple para verificar PostgreSQL usando .NET Core
# No requiere Npgsql instalado globalmente

$PgHost = "72.61.128.126"
$PgPort = "5433"
$PgDatabase = "postgres"
$PgUsername = "postgres"
$PgPassword = "3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "PostgreSQL Database Status Check" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""
Write-Host "Connecting to: $PgHost`:$PgPort" -ForegroundColor Cyan
Write-Host "Database: $PgDatabase" -ForegroundColor Cyan
Write-Host ""

# Crear un proyecto temporal de .NET para usar Npgsql
$tempDir = "$env:TEMP\pg-check-$(Get-Random)"
New-Item -ItemType Directory -Path $tempDir -Force | Out-Null

Push-Location $tempDir

try {
    # Crear proyecto de consola
    dotnet new console -n PgCheck -f net9.0 | Out-Null
    Set-Location PgCheck
    
    # Agregar Npgsql
    dotnet add package Npgsql | Out-Null
    
    # Crear el código
    $code = @"
using System;
using Npgsql;

var connectionString = "Host=$PgHost;Port=$PgPort;Database=$PgDatabase;Username=$PgUsername;Password=$PgPassword";

try
{
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    
    Console.WriteLine("✅ Connected successfully!");
    Console.WriteLine();
    
    // List schemas
    Console.WriteLine("=== SCHEMAS ===");
    using (var cmd = new NpgsqlCommand("SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast') ORDER BY schema_name", conn))
    using (var reader = cmd.ExecuteReader())
    {
        var schemas = new System.Collections.Generic.List<string>();
        while (reader.Read())
        {
            var schema = reader.GetString(0);
            schemas.Add(schema);
            Console.WriteLine($"  - {schema}");
        }
        
        if (schemas.Count == 0)
        {
            Console.WriteLine("  No custom schemas found");
        }
        
        Console.WriteLine();
        
        // For each schema, list tables
        reader.Close();
        
        Console.WriteLine("=== TABLES BY SCHEMA ===");
        foreach (var schema in schemas)
        {
            Console.WriteLine($"\n[{schema}] Schema:");
            
            using var tableCmd = new NpgsqlCommand($"SELECT table_name FROM information_schema.tables WHERE table_schema = '{schema}' AND table_type = 'BASE TABLE' ORDER BY table_name", conn);
            using var tableReader = tableCmd.ExecuteReader();
            
            var tables = new System.Collections.Generic.List<string>();
            while (tableReader.Read())
            {
                tables.Add(tableReader.GetString(0));
            }
            tableReader.Close();
            
            if (tables.Count == 0)
            {
                Console.WriteLine("  No tables found");
            }
            else
            {
                foreach (var table in tables)
                {
                    using var countCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM \"{schema}\".\"{table}\"", conn);
                    var count = countCmd.ExecuteScalar();
                    Console.WriteLine($"  - {table} ({count} rows)");
                }
            }
        }
    }
    
    // Check public schema
    Console.WriteLine("\n[public] Schema:");
    using (var cmd = new NpgsqlCommand("SELECT table_name FROM information_schema.tables WHERE table_schema = 'public' AND table_type = 'BASE TABLE' ORDER BY table_name", conn))
    using (var reader = cmd.ExecuteReader())
    {
        var tables = new System.Collections.Generic.List<string>();
        while (reader.Read())
        {
            tables.Add(reader.GetString(0));
        }
        reader.Close();
        
        if (tables.Count == 0)
        {
            Console.WriteLine("  No tables found");
        }
        else
        {
            foreach (var table in tables)
            {
                using var countCmd = new NpgsqlCommand($"SELECT COUNT(*) FROM public.\"{table}\"", conn);
                var count = countCmd.ExecuteScalar();
                Console.WriteLine($"  - {table} ({count} rows)");
            }
        }
    }
    
    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("Check completed successfully!");
    Console.WriteLine("========================================");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Error: {ex.Message}");
    Console.WriteLine(ex.StackTrace);
    Environment.Exit(1);
}
"@

    $code | Out-File -FilePath "Program.cs" -Encoding UTF8
    
    # Ejecutar
    Write-Host "Building and running check..." -ForegroundColor Yellow
    dotnet run --verbosity quiet
}
catch {
    Write-Host "❌ Error: $_" -ForegroundColor Red
}
finally {
    Pop-Location
    Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
}

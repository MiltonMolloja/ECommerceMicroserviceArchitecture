# Script para verificar el estado de PostgreSQL
# Verifica schemas, tablas y cantidad de registros

$ConnectionString = "Host=72.61.128.126;Port=5433;Database=postgres;Username=postgres;Password=3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2"

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "PostgreSQL Database Status Check" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

try {
    # Cargar el driver de PostgreSQL
    Add-Type -Path "C:\Program Files\PackageManagement\NuGet\Packages\Npgsql.8.0.5\lib\net8.0\Npgsql.dll" -ErrorAction SilentlyContinue
    
    $conn = New-Object Npgsql.NpgsqlConnection($ConnectionString)
    $conn.Open()
    
    Write-Host "✅ Connected to PostgreSQL successfully!" -ForegroundColor Green
    Write-Host ""
    
    # 1. List all schemas
    Write-Host "=== SCHEMAS ===" -ForegroundColor Yellow
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = "SELECT schema_name FROM information_schema.schemata WHERE schema_name NOT IN ('pg_catalog', 'information_schema', 'pg_toast') ORDER BY schema_name"
    $reader = $cmd.ExecuteReader()
    
    $schemas = @()
    while ($reader.Read()) {
        $schemaName = $reader.GetString(0)
        $schemas += $schemaName
        Write-Host "  - $schemaName" -ForegroundColor Cyan
    }
    $reader.Close()
    
    if ($schemas.Count -eq 0) {
        Write-Host "  No custom schemas found" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    # 2. List tables in each schema
    Write-Host "=== TABLES BY SCHEMA ===" -ForegroundColor Yellow
    
    foreach ($schema in $schemas) {
        Write-Host "`n[$schema] Schema:" -ForegroundColor Cyan
        
        $cmd = $conn.CreateCommand()
        $cmd.CommandText = @"
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = '$schema' 
  AND table_type = 'BASE TABLE'
ORDER BY table_name
"@
        $reader = $cmd.ExecuteReader()
        
        $tables = @()
        while ($reader.Read()) {
            $tableName = $reader.GetString(0)
            $tables += $tableName
        }
        $reader.Close()
        
        if ($tables.Count -eq 0) {
            Write-Host "  No tables found" -ForegroundColor Yellow
        } else {
            foreach ($table in $tables) {
                # Get row count
                $countCmd = $conn.CreateCommand()
                $countCmd.CommandText = "SELECT COUNT(*) FROM `"$schema`".`"$table`""
                $count = $countCmd.ExecuteScalar()
                
                Write-Host "  - $table ($count rows)" -ForegroundColor White
            }
        }
    }
    
    # 3. Check public schema tables
    Write-Host "`n[public] Schema:" -ForegroundColor Cyan
    $cmd = $conn.CreateCommand()
    $cmd.CommandText = @"
SELECT table_name 
FROM information_schema.tables 
WHERE table_schema = 'public' 
  AND table_type = 'BASE TABLE'
ORDER BY table_name
"@
    $reader = $cmd.ExecuteReader()
    
    $publicTables = @()
    while ($reader.Read()) {
        $tableName = $reader.GetString(0)
        $publicTables += $tableName
    }
    $reader.Close()
    
    if ($publicTables.Count -eq 0) {
        Write-Host "  No tables found" -ForegroundColor Yellow
    } else {
        foreach ($table in $publicTables) {
            # Get row count
            $countCmd = $conn.CreateCommand()
            $countCmd.CommandText = "SELECT COUNT(*) FROM public.`"$table`""
            $count = $countCmd.ExecuteScalar()
            
            Write-Host "  - $table ($count rows)" -ForegroundColor White
        }
    }
    
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Magenta
    Write-Host "Summary:" -ForegroundColor Green
    Write-Host "  Schemas: $($schemas.Count)" -ForegroundColor White
    Write-Host "========================================" -ForegroundColor Magenta
    
    $conn.Close()
}
catch {
    Write-Host "❌ Error connecting to PostgreSQL:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Trying alternative method with SqlClient..." -ForegroundColor Yellow
    
    # Intentar con una query simple usando Invoke-Expression
    Write-Host ""
    Write-Host "Please install Npgsql if not available:" -ForegroundColor Cyan
    Write-Host "  Install-Package Npgsql -ProviderName NuGet -Scope CurrentUser" -ForegroundColor White
}

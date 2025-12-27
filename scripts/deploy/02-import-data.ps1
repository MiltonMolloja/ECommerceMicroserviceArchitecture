# Script para importar datos a PostgreSQL
# Ejecuta todos los archivos SQL de migration-data en el orden correcto

$PgHost = "72.61.128.126"
$PgPort = "5433"
$PgDatabase = "postgres"
$PgUsername = "postgres"
$PgPassword = "3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2"
$DataDir = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data"

Write-Host "============================================" -ForegroundColor Magenta
Write-Host "PostgreSQL Data Import" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host ""
Write-Host "Target: $PgHost`:$PgPort" -ForegroundColor Cyan
Write-Host "Database: $PgDatabase" -ForegroundColor Cyan
Write-Host "Data Directory: $DataDir" -ForegroundColor Cyan
Write-Host ""

# Definir el orden de importación (respetando foreign keys)
$importOrder = @(
    # Identity Schema (primero porque otros schemas pueden referenciar AspNetUsers)
    "20-identity-aspnetroles.sql",
    "21-identity-aspnetusers-fixed.sql",  # Usar la versión fixed
    "22-identity-aspnetuserroles.sql",
    "23-identity-aspnetroleclaims.sql",
    "24-identity-aspnetuserclaims.sql",
    "25-identity-aspnetuserlogins.sql",
    "26-identity-aspnetusertokens.sql",
    "27-identity-refreshtokens.sql",
    "28-identity-userauditlogs.sql",
    "29-identity-userbackupcodes.sql",
    
    # Catalog Schema
    "01-catalog-brands.sql",
    "02-catalog-categories.sql",
    "03-catalog-products-fixed.sql",  # Usar la versión fixed (sin columna Stock)
    "04-catalog-productcategories.sql",
    "05-catalog-productinstock.sql",
    "07-catalog-productattributes.sql",  # PRIMERO ProductAttributes
    "06-catalog-attributevalues.sql",    # LUEGO AttributeValues (FK dependency)
    "08-catalog-productattributevalues.sql",
    "09-catalog-productratings.sql",
    "10-catalog-productreviews.sql",
    "11-catalog-banners.sql",
    
    # Customer Schema (usa la versión mapped que incluye direcciones)
    "30-customer-clients-mapped.sql",
    "31-customer-clientaddresses.sql",
    
    # Order Schema (usa las versiones mapped)
    "40-order-paymenttypes.sql",
    "41-order-orderstatuses.sql",
    "42-order-orders-mapped.sql",
    "43-order-orderdetails-mapped.sql",
    
    # Cart Schema (usa las versiones mapped)
    "50-cart-carts-mapped.sql",
    "51-cart-cartitems-mapped.sql",
    
    # Payment Schema
    "60-payment-payments.sql",
    "61-payment-paymentdetails.sql",
    "62-payment-paymenttransactions.sql",
    
    # Notification Schema (usa la versión mapped)
    "70-notification-templates.sql",
    "71-notification-notifications-mapped.sql",
    "72-notification-preferences.sql"
)

# Función para ejecutar un archivo SQL
function Execute-SqlFile {
    param(
        [string]$FileName,
        [string]$ConnectionString
    )
    
    $filePath = Join-Path $DataDir $FileName
    
    if (-not (Test-Path $filePath)) {
        Write-Host "  ⚠️  File not found: $FileName (skipping)" -ForegroundColor Yellow
        return $false
    }
    
    Write-Host "  📄 Importing: $FileName" -ForegroundColor Cyan
    
    try {
        # Crear un proyecto temporal de .NET para ejecutar el script
        $tempDir = "$env:TEMP\pg-import-$(Get-Random)"
        New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
        
        Push-Location $tempDir
        
        # Crear proyecto de consola
        dotnet new console -n PgImport -f net9.0 2>&1 | Out-Null
        Set-Location PgImport
        
        # Agregar Npgsql
        dotnet add package Npgsql 2>&1 | Out-Null
        
        # Crear el código
        $code = @"
using System;
using System.IO;
using Npgsql;

var connectionString = "$ConnectionString";
var sqlFilePath = @"$filePath";

try
{
    var sqlScript = File.ReadAllText(sqlFilePath);
    
    using var conn = new NpgsqlConnection(connectionString);
    conn.Open();
    
    using var cmd = new NpgsqlCommand(sqlScript, conn);
    cmd.CommandTimeout = 300;
    
    var rowsAffected = cmd.ExecuteNonQuery();
    
    Console.WriteLine("SUCCESS");
}
catch (Exception ex)
{
    Console.WriteLine($"ERROR: {ex.Message}");
    Environment.Exit(1);
}
"@

        $code | Out-File -FilePath "Program.cs" -Encoding UTF8
        
        # Ejecutar
        $output = dotnet run --verbosity quiet 2>&1
        
        Pop-Location
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        
        if ($output -match "SUCCESS") {
            Write-Host "    ✅ Imported successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "    ❌ Error: $output" -ForegroundColor Red
            return $false
        }
    }
    catch {
        Write-Host "    ❌ Error: $_" -ForegroundColor Red
        Pop-Location -ErrorAction SilentlyContinue
        Remove-Item -Path $tempDir -Recurse -Force -ErrorAction SilentlyContinue
        return $false
    }
}

# Ejecutar importación
$connectionString = "Host=$PgHost;Port=$PgPort;Database=$PgDatabase;Username=$PgUsername;Password=$PgPassword"
$successCount = 0
$failCount = 0
$skipCount = 0

Write-Host "Starting data import..." -ForegroundColor Yellow
Write-Host ""

foreach ($file in $importOrder) {
    $result = Execute-SqlFile -FileName $file -ConnectionString $connectionString
    
    if ($result -eq $true) {
        $successCount++
    } elseif ($result -eq $false -and (Test-Path (Join-Path $DataDir $file))) {
        $failCount++
    } else {
        $skipCount++
    }
    
    Start-Sleep -Milliseconds 500
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "Import Summary" -ForegroundColor Magenta
Write-Host "============================================" -ForegroundColor Magenta
Write-Host "  ✅ Successful: $successCount" -ForegroundColor Green
Write-Host "  ❌ Failed: $failCount" -ForegroundColor Red
Write-Host "  ⚠️  Skipped: $skipCount" -ForegroundColor Yellow
Write-Host "============================================" -ForegroundColor Magenta

if ($failCount -gt 0) {
    Write-Host ""
    Write-Host "⚠️  Some imports failed. Please review the errors above." -ForegroundColor Yellow
    exit 1
} else {
    Write-Host ""
    Write-Host "✅ Data import completed successfully!" -ForegroundColor Green
}

# TiendaMia Batch Scraper Script
# Usage: .\scrape-batch.ps1 -InputFile productos.txt
# Usage: .\scrape-batch.ps1 -Urls @("url1", "url2", "url3")

param(
    [Parameter(Mandatory=$false)]
    [string]$InputFile = "",

    [Parameter(Mandatory=$false)]
    [string[]]$Urls = @(),

    [Parameter(Mandatory=$false)]
    [int]$DelaySeconds = 2
)

Write-Host "=== TiendaMia Batch Scraper ===" -ForegroundColor Cyan
Write-Host ""

# Determinar fuente de URLs
$productUrls = @()

if ($InputFile -ne "") {
    if (Test-Path $InputFile) {
        $productUrls = Get-Content $InputFile | Where-Object { $_ -match "https://tiendamia.com" }
        Write-Host "Cargadas $($productUrls.Count) URLs desde $InputFile" -ForegroundColor Green
    } else {
        Write-Host "Error: Archivo $InputFile no encontrado" -ForegroundColor Red
        exit 1
    }
} elseif ($Urls.Count -gt 0) {
    $productUrls = $Urls
    Write-Host "Procesando $($productUrls.Count) URLs proporcionadas" -ForegroundColor Green
} else {
    Write-Host "Error: Debe proporcionar -InputFile o -Urls" -ForegroundColor Red
    Write-Host ""
    Write-Host "Ejemplos de uso:" -ForegroundColor Yellow
    Write-Host "  .\scrape-batch.ps1 -InputFile productos.txt"
    Write-Host "  .\scrape-batch.ps1 -Urls @('https://tiendamia.com/ar/p/amz/...')"
    exit 1
}

# Contadores
$total = $productUrls.Count
$success = 0
$failed = 0
$current = 0

Write-Host ""
Write-Host "Iniciando scraping de $total productos..." -ForegroundColor Cyan
Write-Host "Pausa entre requests: $DelaySeconds segundos" -ForegroundColor Gray
Write-Host ""

# Procesar cada URL
foreach ($url in $productUrls) {
    $current++

    Write-Host "[$current/$total] Procesando: $url" -ForegroundColor Yellow

    try {
        # Ejecutar scraper
        $output = dotnet run $url 2>&1

        # Verificar si fue exitoso
        if ($LASTEXITCODE -eq 0) {
            $success++
            Write-Host "  ✓ Exitoso" -ForegroundColor Green

            # Extraer SKU del output si está disponible
            if ($output -match "SKU: ([A-Z0-9-]+)") {
                $sku = $matches[1]
                Write-Host "  SKU: $sku" -ForegroundColor Gray
            }
        } else {
            $failed++
            Write-Host "  ✗ Falló" -ForegroundColor Red
        }
    }
    catch {
        $failed++
        Write-Host "  ✗ Error: $($_.Exception.Message)" -ForegroundColor Red
    }

    # Pausa entre requests (excepto en el último)
    if ($current -lt $total) {
        Write-Host "  Esperando $DelaySeconds segundos..." -ForegroundColor Gray
        Start-Sleep -Seconds $DelaySeconds
    }

    Write-Host ""
}

# Resumen
Write-Host "=== Resumen ===" -ForegroundColor Cyan
Write-Host "Total procesados: $total" -ForegroundColor White
Write-Host "Exitosos: $success" -ForegroundColor Green
Write-Host "Fallidos: $failed" -ForegroundColor Red
Write-Host ""
Write-Host "Archivos guardados en: C:\Notas\Milton\" -ForegroundColor Yellow

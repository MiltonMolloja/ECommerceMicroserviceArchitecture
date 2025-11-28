# Gaming Chairs (24) + RAM (22) + Audifonos (29) + Mouse (16) + Motherboards (20) +
# Liquid Cooling (16) + PC Cases (16) + Power Supplies (17) + Printers (23) +
# Consoles (29) + PlayStation (22) + Xbox (14) = 248 productos

$files = @(
    'C:\Notas\Milton\TiendaMia-Silla-Gamer-URLs.txt',
    'C:\Notas\Milton\TiendaMia-RAM-URLs.txt',
    'C:\Notas\Milton\TiendaMia-Audifonos-URLs.txt',
    'C:\Notas\Milton\TiendaMia-Mouse-URLs.txt',
    'C:\Notas\Milton\TiendaMia-PlacaMadre-URLs.txt',
    'C:\Notas\Milton\TiendaMia-RefrigeracionLiquida-URLs.txt',
    'C:\Notas\Milton\TiendaMia-CasePC-URLs.txt',
    'C:\Notas\Milton\TiendaMia-FuentePC-URLs.txt',
    'C:\Notas\Milton\TiendaMia-Impresora-URLs.txt',
    'C:\Notas\Milton\TiendaMia-Consola-URLs.txt',
    'C:\Notas\Milton\TiendaMia-PlayStation-URLs.txt',
    'C:\Notas\Milton\TiendaMia-Xbox-URLs.txt'
)

$allUrls = @()
foreach ($file in $files) {
    if (Test-Path $file) {
        $urls = Get-Content $file | Where-Object { $_ -match '^https://' }
        $allUrls += $urls
    }
}

$uniqueUrls = $allUrls | Sort-Object -Unique

Write-Host '=== Scraping Gaming & PC Accessories ===' -ForegroundColor Cyan
Write-Host "Total productos: $($uniqueUrls.Count)" -ForegroundColor Green
Write-Host "Tiempo estimado: $([math]::Round($uniqueUrls.Count * 3 / 60, 1)) minutos" -ForegroundColor Gray
Write-Host ''

$count = 0
$success = 0
$failed = 0

foreach ($url in $uniqueUrls) {
    $count++
    Write-Host "[$count/$($uniqueUrls.Count)] Procesando..." -ForegroundColor Yellow

    try {
        dotnet run $url 2>&1 | Out-Null

        if ($LASTEXITCODE -eq 0) {
            $success++
            Write-Host '  OK' -ForegroundColor Green
        } else {
            $failed++
            Write-Host '  FAIL' -ForegroundColor Red
        }
    }
    catch {
        $failed++
        Write-Host '  ERROR' -ForegroundColor Red
    }

    Start-Sleep -Seconds 3
}

Write-Host ''
Write-Host '=== RESUMEN ===' -ForegroundColor Cyan
Write-Host "Exitosos: $success" -ForegroundColor Green
Write-Host "Fallidos: $failed" -ForegroundColor Red
Write-Host ''
Write-Host 'Archivos guardados en: C:\Notas\Milton' -ForegroundColor Yellow

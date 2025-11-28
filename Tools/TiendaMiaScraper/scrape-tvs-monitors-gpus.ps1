# Samsung (9) + LG (7) + TCL (10) + Sony (7) + Monitores (19) + GPUs (24) = 76 productos
$files = @(
    'C:\Notas\Milton\TiendaMia-TV-Samsung-URLs.txt',
    'C:\Notas\Milton\TiendaMia-TV-LG-URLs.txt',
    'C:\Notas\Milton\TiendaMia-TV-TCL-URLs.txt',
    'C:\Notas\Milton\TiendaMia-TV-Sony-URLs.txt',
    'C:\Notas\Milton\TiendaMia-Monitores-URLs.txt',
    'C:\Notas\Milton\TiendaMia-GPU-URLs.txt'
)

$allUrls = @()
foreach ($file in $files) {
    $urls = Get-Content $file | Where-Object { $_ -match '^https://' }
    $allUrls += $urls
}

$uniqueUrls = $allUrls | Sort-Object -Unique

Write-Host "=== Scraping TVs + Monitors + GPUs ===" -ForegroundColor Cyan
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

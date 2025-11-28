$urls = Get-Content 'C:\Notas\Milton\TiendaMia-AMD-Notebooks-Complete-URLs.txt' | Where-Object { $_ -match '^https://' }

Write-Host '=== Scraping 24 AMD Notebooks ===' -ForegroundColor Cyan
Write-Host "Total productos: $($urls.Count)" -ForegroundColor Green
Write-Host 'Tiempo estimado: 1.5 minutos' -ForegroundColor Gray
Write-Host ''

$count = 0
$success = 0
$failed = 0

foreach ($url in $urls) {
    $count++
    Write-Host "[$count/24] Procesando..." -ForegroundColor Yellow

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

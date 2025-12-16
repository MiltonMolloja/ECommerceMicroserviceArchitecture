# Script para probar que el cache se deshabilita correctamente
Write-Host "Testing cache disable functionality..." -ForegroundColor Cyan

# Primero, deshabilitar cache
Write-Host ""
Write-Host "1. Disabling cache..." -ForegroundColor Yellow
& ".\disable-cache.ps1"

Write-Host ""
Write-Host "2. Checking configuration files..." -ForegroundColor Yellow

$services = @(
    "src\Services\Catalog\Catalog.Api\appsettings.json",
    "src\Gateways\Api.Gateway.WebClient\appsettings.json",
    "src\Services\Order\Order.Api\appsettings.json",
    "src\Services\Payment\Payment.Api\appsettings.json"
)

foreach ($servicePath in $services) {
    if (Test-Path $servicePath) {
        $content = Get-Content $servicePath -Raw | ConvertFrom-Json
        $disabled = $content.CacheSettings.Disabled
        
        if ($disabled -eq $true) {
            Write-Host "Cache DISABLED in: $servicePath" -ForegroundColor Green
        } else {
            Write-Host "Cache ENABLED in: $servicePath" -ForegroundColor Red
        }
    }
}

Write-Host ""
Write-Host "3. To re-enable cache, run: .\enable-cache.ps1" -ForegroundColor Yellow
Write-Host "4. Remember to restart your services!" -ForegroundColor Red
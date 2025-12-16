# Script simple para habilitar cache
Write-Host "Enabling cache in all services..." -ForegroundColor Yellow

$services = @(
    "src\Services\Catalog\Catalog.Api\appsettings.json",
    "src\Gateways\Api.Gateway.WebClient\appsettings.json",
    "src\Services\Order\Order.Api\appsettings.json",
    "src\Services\Payment\Payment.Api\appsettings.json"
)

foreach ($servicePath in $services) {
    if (Test-Path $servicePath) {
        $content = Get-Content $servicePath -Raw | ConvertFrom-Json
        
        # Remover o cambiar CacheSettings.Disabled
        if ($content.CacheSettings -and $content.CacheSettings.Disabled) {
            $content.CacheSettings.Disabled = $false
        }
        
        # Guardar
        $content | ConvertTo-Json -Depth 10 | Set-Content $servicePath
        Write-Host "Cache enabled in: $servicePath" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Cache enabled in all services!" -ForegroundColor Cyan
Write-Host "Remember to restart your services for changes to take effect!" -ForegroundColor Red
# Script simple para deshabilitar cache temporalmente
Write-Host "Disabling cache in all services..." -ForegroundColor Yellow

$services = @(
    "src\Services\Catalog\Catalog.Api\appsettings.json",
    "src\Gateways\Api.Gateway.WebClient\appsettings.json",
    "src\Services\Order\Order.Api\appsettings.json",
    "src\Services\Payment\Payment.Api\appsettings.json"
)

foreach ($servicePath in $services) {
    if (Test-Path $servicePath) {
        $content = Get-Content $servicePath -Raw | ConvertFrom-Json
        
        # Agregar o modificar CacheSettings
        if (-not $content.CacheSettings) {
            $content | Add-Member -Type NoteProperty -Name "CacheSettings" -Value @{} -Force
        }
        
        $content.CacheSettings | Add-Member -Type NoteProperty -Name "Disabled" -Value $true -Force
        
        # Guardar
        $content | ConvertTo-Json -Depth 10 | Set-Content $servicePath
        Write-Host "Cache disabled in: $servicePath" -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Cache disabled in all services!" -ForegroundColor Cyan
Write-Host "Remember to restart your services for changes to take effect!" -ForegroundColor Red
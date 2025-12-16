# Script para habilitar/deshabilitar cache temporalmente
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("disable", "enable")]
    [string]$Action
)

$services = @(
    "src\Services\Catalog\Catalog.Api",
    "src\Gateways\Api.Gateway.WebClient",
    "src\Services\Order\Order.Api",
    "src\Services\Payment\Payment.Api"
)

Write-Host "Cache $Action operation started..." -ForegroundColor Yellow

foreach ($service in $services) {
    $appsettingsPath = "$service\appsettings.json"
    $noCachePath = "$service\appsettings.NoCache.json"
    $backupPath = "$service\appsettings.backup.json"
    
    if ($Action -eq "disable") {
        if (Test-Path $appsettingsPath) {
            # Hacer backup del archivo original
            Copy-Item $appsettingsPath $backupPath -Force
            Write-Host "✓ Backup created: $backupPath" -ForegroundColor Green
            
            # Leer configuración actual
            $config = Get-Content $appsettingsPath | ConvertFrom-Json
            
            # Agregar o modificar CacheSettings
            if (-not $config.CacheSettings) {
                $config | Add-Member -Type NoteProperty -Name "CacheSettings" -Value @{}
            }
            $config.CacheSettings | Add-Member -Type NoteProperty -Name "Disabled" -Value $true -Force
            
            # Guardar configuración modificada
            $config | ConvertTo-Json -Depth 10 | Set-Content $appsettingsPath
            Write-Host "✓ Cache disabled for: $service" -ForegroundColor Green
        }
    }
    elseif ($Action -eq "enable") {
        if (Test-Path $backupPath) {
            # Restaurar desde backup
            Copy-Item $backupPath $appsettingsPath -Force
            Remove-Item $backupPath -Force
            Write-Host "✓ Cache enabled for: $service (restored from backup)" -ForegroundColor Green
        }
        else {
            Write-Host "⚠ No backup found for: $service" -ForegroundColor Yellow
        }
    }
}

Write-Host "`nCache $Action operation completed!" -ForegroundColor Cyan

if ($Action -eq "disable") {
    Write-Host "`nTo re-enable cache, run: .\toggle-cache.ps1 enable" -ForegroundColor Yellow
    Write-Host "Remember to restart your services for changes to take effect!" -ForegroundColor Red
}
else {
    Write-Host "`nCache has been restored to original configuration." -ForegroundColor Green
    Write-Host "Remember to restart your services for changes to take effect!" -ForegroundColor Red
}
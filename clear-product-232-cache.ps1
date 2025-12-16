# =====================================================
# Script: Limpiar Cache del Producto 232
# Descripción: Limpia el cache del producto 232 (LG TV)
# =====================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Limpieza de Cache - Producto 232" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si redis-cli está disponible
$redisCli = Get-Command redis-cli -ErrorAction SilentlyContinue

if (-not $redisCli) {
    Write-Host "ERROR: redis-cli no está instalado" -ForegroundColor Red
    Write-Host ""
    Write-Host "Alternativa: Reiniciar el servicio de Catalog API" -ForegroundColor Yellow
    Write-Host "El cache se limpiará automáticamente al reiniciar" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "Buscando claves del producto 232..." -ForegroundColor Yellow
Write-Host ""

# Buscar y eliminar claves relacionadas con el producto 232
$patterns = @(
    "*products:id:232*",
    "*gateway:products:id:232*",
    "*product:232*"
)

$deletedCount = 0

foreach ($pattern in $patterns) {
    Write-Host "  Patrón: $pattern" -ForegroundColor Cyan
    
    $keys = redis-cli --scan --pattern $pattern
    
    if ($keys) {
        foreach ($key in $keys) {
            redis-cli DEL $key | Out-Null
            Write-Host "    ✓ Eliminada: $key" -ForegroundColor Green
            $deletedCount++
        }
    }
    else {
        Write-Host "    (No se encontraron claves)" -ForegroundColor Gray
    }
}

Write-Host ""
if ($deletedCount -gt 0) {
    Write-Host "✓ Se eliminaron $deletedCount claves del cache" -ForegroundColor Green
}
else {
    Write-Host "ℹ No se encontraron claves en cache para el producto 232" -ForegroundColor Yellow
    Write-Host "  (El cache podría estar vacío o usar un patrón diferente)" -ForegroundColor Gray
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

pause

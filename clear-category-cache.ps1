# Script para limpiar el cache de categorías en Redis
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Limpiando Cache de Categorías" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Redis está instalado
$redisPath = "C:\Program Files\Redis\redis-cli.exe"
if (-not (Test-Path $redisPath)) {
    $redisPath = "redis-cli"
}

try {
    # Intentar limpiar todas las claves de categorías
    Write-Host "Buscando claves de categorías en Redis..." -ForegroundColor Yellow
    
    $keys = & $redisPath KEYS "*categories*" 2>&1
    
    if ($keys) {
        Write-Host "Encontradas $($keys.Count) claves. Eliminando..." -ForegroundColor Yellow
        & $redisPath DEL $keys 2>&1
        Write-Host "Cache de categorías limpiado exitosamente!" -ForegroundColor Green
    } else {
        Write-Host "No se encontraron claves de categorías en cache" -ForegroundColor Yellow
    }
    
    # También limpiar cache de productos que incluyen categorías
    Write-Host ""
    Write-Host "Limpiando cache de productos..." -ForegroundColor Yellow
    $productKeys = & $redisPath KEYS "*products*" 2>&1
    if ($productKeys) {
        & $redisPath DEL $productKeys 2>&1
        Write-Host "Cache de productos limpiado!" -ForegroundColor Green
    }
    
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Green
    Write-Host "CACHE LIMPIADO EXITOSAMENTE" -ForegroundColor Green
    Write-Host "============================================" -ForegroundColor Green
    
} catch {
    Write-Host ""
    Write-Host "============================================" -ForegroundColor Red
    Write-Host "ERROR: No se pudo conectar a Redis" -ForegroundColor Red
    Write-Host "============================================" -ForegroundColor Red
    Write-Host ""
    Write-Host "Opciones:" -ForegroundColor Yellow
    Write-Host "1. Asegúrate de que Redis esté corriendo" -ForegroundColor Yellow
    Write-Host "2. O reinicia el backend para que cargue los nuevos datos" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "Presiona cualquier tecla para continuar..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")

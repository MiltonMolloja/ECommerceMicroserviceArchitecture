# =====================================================
# Script: Limpiar Cache de Redis
# Descripción: Limpia todas las claves de cache relacionadas con búsquedas
# =====================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Limpieza de Cache de Redis" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Configuración
$redisHost = "localhost"
$redisPort = 6379
$redisPassword = ""  # Dejar vacío si no tiene password

Write-Host "Conectando a Redis: $redisHost:$redisPort" -ForegroundColor Yellow
Write-Host ""

# Verificar si redis-cli está disponible
$redisCli = Get-Command redis-cli -ErrorAction SilentlyContinue

if (-not $redisCli) {
    Write-Host "ERROR: redis-cli no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Opciones:" -ForegroundColor Yellow
    Write-Host "1. Instalar Redis para Windows desde: https://github.com/microsoftarchive/redis/releases" -ForegroundColor Yellow
    Write-Host "2. Usar Docker: docker exec -it redis redis-cli FLUSHDB" -ForegroundColor Yellow
    Write-Host "3. Reiniciar el servicio de Redis" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

# Menú de opciones
Write-Host "Selecciona qué limpiar:" -ForegroundColor Green
Write-Host "1. Limpiar SOLO cache de búsquedas (gateway:products:search:* y products:search:*)" -ForegroundColor White
Write-Host "2. Limpiar TODA la base de datos de Redis (FLUSHDB)" -ForegroundColor Yellow
Write-Host "3. Cancelar" -ForegroundColor Gray
Write-Host ""

$option = Read-Host "Opción (1-3)"

switch ($option) {
    "1" {
        Write-Host ""
        Write-Host "Limpiando cache de búsquedas..." -ForegroundColor Yellow
        Write-Host ""
        
        # Limpiar cache del Gateway
        Write-Host "  - Eliminando claves: gateway:products:search:*" -ForegroundColor Cyan
        redis-cli --scan --pattern "gateway:products:search:*" | ForEach-Object {
            redis-cli DEL $_
            Write-Host "    ✓ Eliminada: $_" -ForegroundColor Gray
        }
        
        # Limpiar cache de Catalog
        Write-Host "  - Eliminando claves: products:search:*" -ForegroundColor Cyan
        redis-cli --scan --pattern "products:search:*" | ForEach-Object {
            redis-cli DEL $_
            Write-Host "    ✓ Eliminada: $_" -ForegroundColor Gray
        }
        
        Write-Host ""
        Write-Host "✓ Cache de búsquedas limpiado exitosamente!" -ForegroundColor Green
    }
    
    "2" {
        Write-Host ""
        Write-Host "ADVERTENCIA: Esto eliminará TODAS las claves de Redis" -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? (s/n)"
        
        if ($confirm -eq "s" -or $confirm -eq "S") {
            Write-Host ""
            Write-Host "Ejecutando FLUSHDB..." -ForegroundColor Yellow
            redis-cli FLUSHDB
            Write-Host ""
            Write-Host "✓ Toda la base de datos de Redis ha sido limpiada!" -ForegroundColor Green
        }
        else {
            Write-Host ""
            Write-Host "Operación cancelada" -ForegroundColor Yellow
        }
    }
    
    "3" {
        Write-Host ""
        Write-Host "Operación cancelada" -ForegroundColor Yellow
        exit 0
    }
    
    default {
        Write-Host ""
        Write-Host "Opción inválida" -ForegroundColor Red
        exit 1
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Mostrar estadísticas de Redis
Write-Host "Estadísticas de Redis:" -ForegroundColor Cyan
Write-Host ""
redis-cli INFO stats | Select-String -Pattern "total_commands_processed|keyspace_hits|keyspace_misses"
Write-Host ""

# Mostrar claves restantes relacionadas con productos
Write-Host "Claves de productos restantes:" -ForegroundColor Cyan
$productKeys = redis-cli --scan --pattern "*product*" | Measure-Object
Write-Host "  Total de claves con 'product': $($productKeys.Count)" -ForegroundColor White
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

pause

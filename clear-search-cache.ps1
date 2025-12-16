# =====================================================
# Script: Limpiar Cache de Búsquedas
# Descripción: Limpia cache de Gateway y Catalog
# =====================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Limpieza de Cache de Búsquedas" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Configuración
$redisHost = "localhost"
$redisPort = 6379

Write-Host "Conectando a Redis en ${redisHost}:${redisPort}" -ForegroundColor Yellow
Write-Host ""

# Verificar si redis-cli está disponible
try {
    $null = redis-cli --version
} catch {
    Write-Host "ERROR: redis-cli no está instalado o no está en el PATH" -ForegroundColor Red
    Write-Host ""
    Write-Host "Opciones alternativas:" -ForegroundColor Yellow
    Write-Host "1. Instalar Redis CLI" -ForegroundColor Yellow
    Write-Host "2. Usar: docker exec -it ecommerce-redis redis-cli FLUSHDB" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

Write-Host "Limpiando caches de búsquedas..." -ForegroundColor Yellow
Write-Host ""

# Contador de claves eliminadas
$totalDeleted = 0

# Limpiar cache del Gateway
Write-Host "Limpiando Gateway cache (gateway:products:search:*)..." -ForegroundColor Cyan
$gatewayKeys = redis-cli --scan --pattern "gateway:products:search:*"
if ($gatewayKeys) {
    $gatewayKeys | ForEach-Object {
        redis-cli DEL $_
        $totalDeleted++
        Write-Host "  ✓ $_" -ForegroundColor Gray
    }
}

# Limpiar cache de Catalog
Write-Host ""
Write-Host "Limpiando Catalog cache (products:search:*)..." -ForegroundColor Cyan
$catalogKeys = redis-cli --scan --pattern "products:search:*"
if ($catalogKeys) {
    $catalogKeys | ForEach-Object {
        redis-cli DEL $_
        $totalDeleted++
        Write-Host "  ✓ $_" -ForegroundColor Gray
    }
}

# Limpiar cache del Gateway con idioma
Write-Host ""
Write-Host "Limpiando Gateway cache con idioma (gateway:*:products:search:*)..." -ForegroundColor Cyan
$gatewayLangKeys = redis-cli --scan --pattern "gateway:*:products:search:*"
if ($gatewayLangKeys) {
    $gatewayLangKeys | ForEach-Object {
        redis-cli DEL $_
        $totalDeleted++
        Write-Host "  ✓ $_" -ForegroundColor Gray
    }
}

# Limpiar cache de Catalog con idioma
Write-Host ""
Write-Host "Limpiando Catalog cache con idioma (*:products:search:*)..." -ForegroundColor Cyan
$catalogLangKeys = redis-cli --scan --pattern "*:products:search:*"
if ($catalogLangKeys) {
    $catalogLangKeys | ForEach-Object {
        redis-cli DEL $_
        $totalDeleted++
        Write-Host "  ✓ $_" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "✓ Cache limpiada exitosamente!" -ForegroundColor Green
Write-Host "  Total de claves eliminadas: $totalDeleted" -ForegroundColor White
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Mostrar claves de productos restantes
$remainingKeys = redis-cli --scan --pattern "*product*"
$count = ($remainingKeys | Measure-Object).Count
Write-Host "Claves de productos restantes en Redis: $count" -ForegroundColor Cyan
Write-Host ""

pause

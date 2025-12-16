# Limpia toda la cache de Redis usando Docker

Write-Host "Limpiando cache de Redis..." -ForegroundColor Yellow

# Intentar con diferentes nombres de contenedor
$containerNames = @(
    "ecommerce-redis",
    "redis", 
    "ecommercemicroservicearchitecture-redis-1",
    "ecommercemicroservicearchitecture_redis_1"
)

$success = $false

foreach ($container in $containerNames) {
    Write-Host "Intentando con contenedor: $container" -ForegroundColor Cyan
    
    $result = docker exec $container redis-cli FLUSHALL 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "OK - Cache limpiada exitosamente en $container" -ForegroundColor Green
        $success = $true
        break
    }
}

if (-not $success) {
    Write-Host ""
    Write-Host "No se pudo encontrar el contenedor de Redis." -ForegroundColor Red
    Write-Host "Contenedores disponibles:" -ForegroundColor Yellow
    docker ps --format "{{.Names}}"
    Write-Host ""
    Write-Host "Ejecuta manualmente: docker exec -it [nombre-contenedor] redis-cli FLUSHALL" -ForegroundColor Yellow
}

Write-Host ""

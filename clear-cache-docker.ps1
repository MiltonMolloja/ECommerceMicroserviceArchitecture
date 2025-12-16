# =====================================================
# Script: Limpiar Cache usando Docker
# Descripción: Limpia toda la cache de Redis usando Docker
# =====================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Limpieza de Cache Redis (Docker)" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Verificar si Docker está disponible
try {
    docker --version | Out-Null
    Write-Host "✓ Docker está disponible" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "ERROR: Docker no está disponible" -ForegroundColor Red
    Write-Host "Asegúrate de que Docker Desktop esté iniciado" -ForegroundColor Yellow
    Write-Host ""
    pause
    exit 1
}

# Buscar el contenedor de Redis
Write-Host "Buscando contenedor de Redis..." -ForegroundColor Yellow

$redisContainers = @(
    "ecommerce-redis",
    "redis",
    "ecommercemicroservicearchitecture-redis-1",
    "ecommercemicroservicearchitecture_redis_1"
)

$containerFound = $false
$containerName = ""

foreach ($name in $redisContainers) {
    try {
        $result = docker ps --filter "name=$name" --format "{{.Names}}" 2>$null
        if ($result) {
            $containerName = $name
            $containerFound = $true
            break
        }
    } catch {
        continue
    }
}

if (-not $containerFound) {
    Write-Host ""
    Write-Host "ERROR: No se encontró el contenedor de Redis" -ForegroundColor Red
    Write-Host ""
    Write-Host "Contenedores en ejecución:" -ForegroundColor Yellow
    docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Status}}"
    Write-Host ""
    Write-Host "Prueba con el nombre correcto del contenedor manualmente:" -ForegroundColor Yellow
    Write-Host "  docker exec -it <nombre-contenedor> redis-cli FLUSHDB" -ForegroundColor Cyan
    Write-Host ""
    pause
    exit 1
}

Write-Host "✓ Contenedor encontrado: $containerName" -ForegroundColor Green
Write-Host ""

# Menú de opciones
Write-Host "Selecciona qué limpiar:" -ForegroundColor Green
Write-Host "1. FLUSHDB - Limpiar base de datos actual" -ForegroundColor White
Write-Host "2. FLUSHALL - Limpiar TODAS las bases de datos" -ForegroundColor Yellow
Write-Host "3. Cancelar" -ForegroundColor Gray
Write-Host ""

$option = Read-Host "Opción (1-3)"

switch ($option) {
    "1" {
        Write-Host ""
        Write-Host "Ejecutando FLUSHDB..." -ForegroundColor Yellow
        docker exec -it $containerName redis-cli FLUSHDB
        Write-Host ""
        Write-Host "✓ Base de datos actual limpiada!" -ForegroundColor Green
    }
    
    "2" {
        Write-Host ""
        Write-Host "ADVERTENCIA: Esto eliminará TODAS las bases de datos de Redis" -ForegroundColor Red
        $confirm = Read-Host "¿Estás seguro? (s/n)"
        
        if ($confirm -eq "s" -or $confirm -eq "S" -or $confirm -eq "y" -or $confirm -eq "Y") {
            Write-Host ""
            Write-Host "Ejecutando FLUSHALL..." -ForegroundColor Yellow
            docker exec -it $containerName redis-cli FLUSHALL
            Write-Host ""
            Write-Host "✓ Todas las bases de datos limpiadas!" -ForegroundColor Green
        } else {
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

# Verificar estado de Redis
Write-Host "Estado de Redis:" -ForegroundColor Cyan
docker exec $containerName redis-cli INFO stats | Select-String -Pattern "total_commands_processed|keyspace_hits|keyspace_misses|db0"
Write-Host ""

Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

pause

# Script: Agregar atributo Condición (Nuevo/Usado) a productos
# Marca el 25% de productos como usados

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  Agregar Atributo Condición" -ForegroundColor Cyan
Write-Host "  (Nuevo/Usado - 25% usados)" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$scriptPath = Join-Path $PSScriptRoot "scripts\add-product-condition.sql"

# Verificar que el archivo SQL existe
if (-not (Test-Path $scriptPath)) {
    Write-Host "ERROR: No se encuentra el archivo SQL" -ForegroundColor Red
    Write-Host "Ruta: $scriptPath" -ForegroundColor Yellow
    exit 1
}

$dockerSuccess = $false

# Intentar con Docker primero
Write-Host "Detectando Docker..." -ForegroundColor Yellow
$dockerCheck = docker ps 2>&1 | Out-Null

if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Docker disponible" -ForegroundColor Green
    Write-Host "Intentando conectar a SQL Server en Docker..." -ForegroundColor Yellow
    Write-Host ""
    
    # Copiar el script al contenedor
    docker cp $scriptPath sqlserver:/tmp/add-product-condition.sql 2>&1 | Out-Null
    
    # Ejecutar el script
    $result = docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i /tmp/add-product-condition.sql 2>&1
    
    if ($LASTEXITCODE -eq 0) {
        $dockerSuccess = $true
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Green
        Write-Host "  ✓ SCRIPT EJECUTADO EXITOSAMENTE" -ForegroundColor Green
        Write-Host "========================================" -ForegroundColor Green
        Write-Host ""
        
        # Mostrar resultado
        $result | ForEach-Object {
            $line = $_.ToString()
            if ($line -match "^===") {
                Write-Host $line -ForegroundColor Cyan
            } elseif ($line -match "✅") {
                Write-Host $line -ForegroundColor Green
            } elseif ($line -match "⚠️") {
                Write-Host $line -ForegroundColor Yellow
            } elseif ($line -match "ERROR") {
                Write-Host $line -ForegroundColor Red
            } else {
                Write-Host $line
            }
        }
        
        Write-Host ""
        Write-Host "Siguiente paso:" -ForegroundColor Cyan
        Write-Host "  1. Limpiar cache de Redis: " -ForegroundColor White -NoNewline
        Write-Host ".\clear-redis-cache.ps1" -ForegroundColor Yellow
        Write-Host "  2. Probar filtro en API: " -ForegroundColor White -NoNewline
        Write-Host "GET /products/search?filter_attr_X=Y" -ForegroundColor Yellow
        Write-Host ""
    }
    else {
        Write-Host "⚠ Docker no disponible o error de conexión" -ForegroundColor Yellow
    }
}
else {
    Write-Host "⚠ Docker no disponible" -ForegroundColor Yellow
}

# Si Docker no funcionó, intentar con SQL Server local
if (-not $dockerSuccess) {
    Write-Host ""
    Write-Host "Intentando conectar a SQL Server local..." -ForegroundColor Yellow
    
    $sqlcmdPath = Get-Command sqlcmd -ErrorAction SilentlyContinue
    
    if ($sqlcmdPath) {
        Write-Host "✓ sqlcmd encontrado" -ForegroundColor Green
        Write-Host "Ejecutando script..." -ForegroundColor Yellow
        Write-Host ""
        
        $result = sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i $scriptPath 2>&1
        
        if ($LASTEXITCODE -eq 0) {
            Write-Host ""
            Write-Host "========================================" -ForegroundColor Green
            Write-Host "  ✓ SCRIPT EJECUTADO EXITOSAMENTE" -ForegroundColor Green
            Write-Host "========================================" -ForegroundColor Green
            Write-Host ""
            
            # Mostrar resultado
            $result | ForEach-Object {
                $line = $_.ToString()
                if ($line -match "^===") {
                    Write-Host $line -ForegroundColor Cyan
                } elseif ($line -match "✅") {
                    Write-Host $line -ForegroundColor Green
                } elseif ($line -match "⚠️") {
                    Write-Host $line -ForegroundColor Yellow
                } elseif ($line -match "ERROR") {
                    Write-Host $line -ForegroundColor Red
                } else {
                    Write-Host $line
                }
            }
            
            Write-Host ""
            Write-Host "Siguiente paso:" -ForegroundColor Cyan
            Write-Host "  1. Limpiar cache de Redis: " -ForegroundColor White -NoNewline
            Write-Host ".\clear-redis-cache.ps1" -ForegroundColor Yellow
            Write-Host "  2. Probar filtro en API: " -ForegroundColor White -NoNewline
            Write-Host "GET /products/search?filter_attr_X=Y" -ForegroundColor Yellow
            Write-Host ""
            
            exit 0
        }
        else {
            Write-Host ""
            Write-Host "ERROR: El script SQL falló" -ForegroundColor Red
            $result | ForEach-Object { Write-Host $_ -ForegroundColor Red }
            exit 1
        }
    }
    else {
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Red
        Write-Host "  ERROR: No se pudo conectar" -ForegroundColor Red
        Write-Host "========================================" -ForegroundColor Red
        Write-Host ""
        Write-Host "Verifica que:" -ForegroundColor Yellow
        Write-Host "  1. SQL Server esté ejecutándose" -ForegroundColor White
        Write-Host "  2. Docker esté ejecutándose (si usas Docker)" -ForegroundColor White
        Write-Host "  3. Las credenciales sean correctas" -ForegroundColor White
        Write-Host "  4. sqlcmd esté instalado (si usas SQL local)" -ForegroundColor White
        Write-Host ""
        exit 1
    }
}

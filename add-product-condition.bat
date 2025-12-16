@echo off
echo ========================================
echo   Agregar Atributo Condicion (Nuevo/Usado)
echo ========================================
echo.

REM Detectar si SQL Server esta en Docker o local
docker ps >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo Detectado: Docker disponible
    echo Intentando conectar a SQL Server en Docker...
    docker exec -it sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i /scripts/add-product-condition.sql -o /scripts/add-product-condition-output.txt
    if %ERRORLEVEL% EQU 0 (
        echo.
        echo ========================================
        echo   SCRIPT EJECUTADO EXITOSAMENTE
        echo ========================================
        echo.
        echo Ver resultado en: scripts\add-product-condition-output.txt
        docker exec sqlserver cat /scripts/add-product-condition-output.txt
        goto :end
    )
)

echo.
echo Intentando conectar a SQL Server local...
sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i "scripts\add-product-condition.sql" -o "scripts\add-product-condition-output.txt"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================
    echo   SCRIPT EJECUTADO EXITOSAMENTE
    echo ========================================
    echo.
    echo Ver resultado en: scripts\add-product-condition-output.txt
    type "scripts\add-product-condition-output.txt"
) else (
    echo.
    echo ========================================
    echo   ERROR: No se pudo conectar a SQL Server
    echo ========================================
    echo.
    echo Verifica que:
    echo   1. SQL Server este ejecutandose
    echo   2. Docker este ejecutandose (si usas Docker)
    echo   3. Las credenciales sean correctas
    echo.
)

:end
echo.
pause

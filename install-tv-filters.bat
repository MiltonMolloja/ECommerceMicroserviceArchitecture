@echo off
REM =============================================
REM Script para instalar filtros de TVs
REM =============================================

echo ========================================
echo INSTALACION DE FILTROS PARA TVs
echo ========================================
echo.

echo Paso 1: Creando atributos filtrables...
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -E -i scripts\add-product-filter-attributes.sql
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: No se pudieron crear los atributos
    pause
    exit /b 1
)
echo.

echo Paso 2: Asignando atributos a productos de TV...
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -E -i scripts\assign-tv-attributes.sql
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: No se pudieron asignar los atributos
    pause
    exit /b 1
)
echo.

echo Paso 3: Verificando instalacion...
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -E -i scripts\diagnose-tv-attributes.sql -o diagnose-tv-output.txt
if %ERRORLEVEL% NEQ 0 (
    echo ERROR: No se pudo verificar la instalacion
    pause
    exit /b 1
)
echo.

echo ========================================
echo INSTALACION COMPLETADA
echo ========================================
echo.
echo Revisa el archivo diagnose-tv-output.txt para ver los resultados
echo.
pause

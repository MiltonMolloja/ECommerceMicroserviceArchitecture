@echo off
REM =====================================================
REM Script: Habilitar Full-Text Search en SQL Server
REM Descripción: Configura índice de texto completo en Products
REM =====================================================

echo.
echo ================================================
echo   Configuracion de Full-Text Search
echo   Base de datos: CatalogDb
echo   Tabla: Catalog.Products
echo ================================================
echo.

REM Configuración
set SERVER=localhost
set DATABASE=CatalogDb
set SCRIPT_PATH=scripts\enable-fulltext-search.sql

echo Servidor: %SERVER%
echo Base de datos: %DATABASE%
echo Script: %SCRIPT_PATH%
echo.

REM Verificar que el script existe
if not exist "%SCRIPT_PATH%" (
    echo ERROR: No se encuentra el script: %SCRIPT_PATH%
    echo.
    pause
    exit /b 1
)

echo Ejecutando script de configuracion...
echo.

REM Ejecutar el script usando sqlcmd
sqlcmd -S %SERVER% -d %DATABASE% -E -i "%SCRIPT_PATH%"

if errorlevel 1 (
    echo.
    echo ERROR: Fallo la ejecucion del script.
    echo.
    echo Posibles causas:
    echo 1. SQL Server no esta corriendo
    echo 2. No tienes permisos suficientes
    echo 3. Full-Text Search no esta instalado en SQL Server
    echo 4. La base de datos CatalogDb no existe
    echo.
    echo Para instalar Full-Text Search:
    echo 1. Ejecuta SQL Server Setup
    echo 2. Selecciona "Agregar caracteristicas"
    echo 3. Selecciona "Full-Text and Semantic Extractions for Search"
    echo.
    pause
    exit /b 1
)

echo.
echo ================================================
echo   Configuracion completada exitosamente!
echo ================================================
echo.
echo Full-Text Index creado en Catalog.Products
echo.
echo Nota: La poblacion del indice puede tardar varios minutos.
echo Puedes verificar el progreso con esta query:
echo.
echo SELECT * FROM sys.dm_fts_index_population 
echo WHERE database_id = DB_ID('CatalogDb');
echo.
echo ================================================
echo.
pause

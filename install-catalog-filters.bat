@echo off
echo ============================================
echo Instalacion de Filtros de Catalogo
echo ============================================
echo.

echo [1/3] Ejecutando script SQL para agregar atributos...
echo.

sqlcmd -S localhost -d ECommerce.Catalog -i "scripts\add-product-filter-attributes.sql"

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: No se pudo ejecutar el script SQL.
    echo Verifica que:
    echo   - SQL Server este corriendo
    echo   - La base de datos ECommerce.Catalog exista
    echo   - sqlcmd este en el PATH
    echo.
    pause
    exit /b 1
)

echo.
echo [2/3] Compilando proyecto Clients.WebClient...
echo.

cd src\Clients\Clients.WebClient
dotnet build

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo ERROR: No se pudo compilar el proyecto.
    echo.
    pause
    exit /b 1
)

echo.
echo ============================================
echo Instalacion completada exitosamente!
echo ============================================
echo.
echo Para ejecutar la aplicacion:
echo   cd src\Clients\Clients.WebClient
echo   dotnet run
echo.
echo Luego navega a: http://localhost:5000/Products
echo.
echo [3/3] Presiona cualquier tecla para salir...
pause > nul

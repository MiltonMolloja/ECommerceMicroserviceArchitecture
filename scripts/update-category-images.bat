@echo off
echo =============================================
echo Actualizando imagenes de categorias
echo =============================================
echo.

sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -E -i update-category-images.sql

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Script ejecutado exitosamente
) else (
    echo.
    echo ❌ Error al ejecutar el script
)

echo.
pause

@echo off
chcp 65001 >nul
echo =============================================
echo Corrigiendo textos de banners
echo =============================================
echo.

sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -E -i fix-banner-texts-direct.sql

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ✅ Textos corregidos exitosamente
) else (
    echo.
    echo ❌ Error al ejecutar el script
)

echo.
pause

@echo off
echo =============================================
echo Add Sample Reviews and Ratings
echo =============================================
echo.

sqlcmd -S localhost -d "ECommerce.Catalog" -E -i "%~dp0add-sample-reviews-and-ratings.sql"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =============================================
    echo SUCCESS: Reviews and ratings added!
    echo =============================================
) else (
    echo.
    echo =============================================
    echo ERROR: Failed to add reviews and ratings
    echo =============================================
)

pause

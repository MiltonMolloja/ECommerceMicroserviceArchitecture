@echo off
echo Checking TV products and their categories...
echo.

sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -i scripts\check-tv-product-categories.sql -o check-tv-categories-output.txt

echo.
echo Results saved to: check-tv-categories-output.txt
echo.
type check-tv-categories-output.txt
echo.
pause

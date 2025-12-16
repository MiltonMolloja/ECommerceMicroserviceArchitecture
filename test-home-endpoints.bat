@echo off
REM =============================================
REM Script: Test Home Endpoints
REM Description: Tests all Home API endpoints
REM =============================================

SET BASE_URL=http://localhost:20000
SET LANG=es

echo.
echo ========================================
echo Testing Home API Endpoints
echo ========================================
echo.

echo [1/8] Testing Aggregator Endpoint...
echo GET %BASE_URL%/v1/Home
curl -s "%BASE_URL%/v1/Home" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"banners" /C:"featured" /C:"error"
echo.

echo [2/8] Testing Banners Endpoint...
echo GET %BASE_URL%/v1/Home/Banners
curl -s "%BASE_URL%/v1/Home/Banners" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"bannerId" /C:"error"
echo.

echo [3/8] Testing Featured Products...
echo GET %BASE_URL%/v1/Home/Featured?page=1^&take=4
curl -s "%BASE_URL%/v1/Home/Featured?page=1&take=4" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"productId" /C:"error"
echo.

echo [4/8] Testing Deals...
echo GET %BASE_URL%/v1/Home/Deals?page=1^&take=4
curl -s "%BASE_URL%/v1/Home/Deals?page=1&take=4" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"productId" /C:"error"
echo.

echo [5/8] Testing Bestsellers...
echo GET %BASE_URL%/v1/Home/Bestsellers?page=1^&take=4
curl -s "%BASE_URL%/v1/Home/Bestsellers?page=1&take=4" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"productId" /C:"error"
echo.

echo [6/8] Testing New Arrivals...
echo GET %BASE_URL%/v1/Home/NewArrivals?page=1^&take=4
curl -s "%BASE_URL%/v1/Home/NewArrivals?page=1&take=4" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"productId" /C:"error"
echo.

echo [7/8] Testing Top Rated...
echo GET %BASE_URL%/v1/Home/TopRated?page=1^&take=4
curl -s "%BASE_URL%/v1/Home/TopRated?page=1&take=4" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"productId" /C:"error"
echo.

echo [8/8] Testing Featured Categories...
echo GET %BASE_URL%/v1/Home/Categories
curl -s "%BASE_URL%/v1/Home/Categories" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"categoryId" /C:"error"
echo.

echo.
echo ========================================
echo Testing Complete
echo ========================================
echo.
echo Now testing with English language...
echo.

SET LANG=en

echo [Aggregator - English]
curl -s "%BASE_URL%/v1/Home" -H "Accept-Language: %LANG%" -w "\nHTTP Status: %%{http_code}\n" | findstr /C:"HTTP Status" /C:"banners" /C:"error"
echo.

echo ========================================
echo All Tests Complete!
echo ========================================
pause

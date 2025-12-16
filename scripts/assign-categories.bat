@echo off
echo ============================================
echo Asignando Productos a Categorias
echo ============================================
echo.

sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -E -i verify-and-assign-categories.sql -o assign-categories-output.txt

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================
    echo EXITO: Productos asignados correctamente
    echo ============================================
    echo.
    echo Ver detalles en: assign-categories-output.txt
    type assign-categories-output.txt
) else (
    echo.
    echo ============================================
    echo ERROR: Fallo al asignar productos
    echo ============================================
)

pause

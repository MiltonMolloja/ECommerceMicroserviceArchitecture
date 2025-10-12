-- Script para verificar los logs en la base de datos
USE ECommerceDb;
GO

PRINT '========================================';
PRINT 'VERIFICACION DE LOGS EN BASE DE DATOS';
PRINT '========================================';
PRINT '';

-- Verificar que el esquema existe
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Logging')
BEGIN
    PRINT 'Esquema [Logging]: EXISTE';

    -- Verificar que la tabla existe
    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Logs' AND schema_id = SCHEMA_ID('Logging'))
    BEGIN
        PRINT 'Tabla [Logging].[Logs]: EXISTE';
        PRINT '';

        -- Contar total de logs
        DECLARE @TotalLogs INT;
        SELECT @TotalLogs = COUNT(*) FROM [Logging].[Logs];
        PRINT 'Total de logs registrados: ' + CAST(@TotalLogs AS VARCHAR(10));
        PRINT '';

        -- Mostrar logs por nivel
        PRINT 'Logs por nivel:';
        SELECT
            LogLevel,
            COUNT(*) AS Total
        FROM [Logging].[Logs]
        GROUP BY LogLevel
        ORDER BY COUNT(*) DESC;
        PRINT '';

        -- Mostrar logs por servicio
        PRINT 'Logs por servicio:';
        SELECT
            ServiceName,
            COUNT(*) AS Total
        FROM [Logging].[Logs]
        GROUP BY ServiceName
        ORDER BY COUNT(*) DESC;
        PRINT '';

        -- Mostrar últimos 20 logs
        PRINT 'Últimos 20 logs registrados:';
        SELECT TOP 20
            Id,
            Timestamp,
            LogLevel,
            ServiceName,
            LEFT(Category, 40) AS Category,
            LEFT(Message, 80) AS Message,
            Environment
        FROM [Logging].[Logs]
        ORDER BY Timestamp DESC;
        PRINT '';

        -- Logs de las últimas 24 horas
        DECLARE @Last24Hours INT;
        SELECT @Last24Hours = COUNT(*)
        FROM [Logging].[Logs]
        WHERE Timestamp >= DATEADD(HOUR, -24, GETUTCDATE());
        PRINT 'Logs en las últimas 24 horas: ' + CAST(@Last24Hours AS VARCHAR(10));

        -- Logs de hoy
        DECLARE @Today INT;
        SELECT @Today = COUNT(*)
        FROM [Logging].[Logs]
        WHERE CAST(Timestamp AS DATE) = CAST(GETUTCDATE() AS DATE);
        PRINT 'Logs de hoy: ' + CAST(@Today AS VARCHAR(10));
    END
    ELSE
    BEGIN
        PRINT 'ERROR: La tabla [Logging].[Logs] NO existe.';
        PRINT 'Ejecuta el script create-logging-table.sql para crearla.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: El esquema [Logging] NO existe.';
    PRINT 'Ejecuta el script create-logging-table.sql para crearlo.';
END
GO

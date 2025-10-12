-- Script para verificar y consultar los logs en la base de datos
USE ECommerceDb;
GO

-- Verificar si el esquema y tabla existen
IF EXISTS (SELECT * FROM sys.schemas WHERE name = 'Logging')
BEGIN
    PRINT 'Esquema [Logging]: EXISTE';

    IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Logs' AND schema_id = SCHEMA_ID('Logging'))
    BEGIN
        PRINT 'Tabla [Logging].[Logs]: EXISTE';
        PRINT '';

        -- Mostrar estructura de la tabla
        PRINT 'Estructura de la tabla:';
        SELECT
            COLUMN_NAME,
            DATA_TYPE,
            CHARACTER_MAXIMUM_LENGTH,
            IS_NULLABLE
        FROM INFORMATION_SCHEMA.COLUMNS
        WHERE TABLE_SCHEMA = 'Logging' AND TABLE_NAME = 'Logs'
        ORDER BY ORDINAL_POSITION;
        PRINT '';

        -- Contar registros
        DECLARE @Count INT;
        SELECT @Count = COUNT(*) FROM [Logging].[Logs];
        PRINT 'Total de logs: ' + CAST(@Count AS VARCHAR(10));
        PRINT '';

        -- Mostrar últimos 10 logs
        IF @Count > 0
        BEGIN
            PRINT 'Últimos 10 logs:';
            SELECT TOP 10
                Id,
                Timestamp,
                LogLevel,
                ServiceName,
                LEFT(Category, 40) AS Category,
                LEFT(Message, 80) AS Message,
                Environment
            FROM [Logging].[Logs]
            ORDER BY Timestamp DESC;
        END
        ELSE
        BEGIN
            PRINT 'No hay logs registrados aún.';
            PRINT 'Inicia un servicio para generar logs.';
        END
    END
    ELSE
    BEGIN
        PRINT 'ERROR: La tabla [Logging].[Logs] NO existe.';
    END
END
ELSE
BEGIN
    PRINT 'ERROR: El esquema [Logging] NO existe.';
END
GO

-- Script para crear el esquema Logging y la tabla Logs en la base de datos ECommerceDb
USE ECommerceDb;
GO

-- Crear esquema Logging si no existe
IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Logging')
BEGIN
    EXEC('CREATE SCHEMA [Logging]');
    PRINT 'Esquema [Logging] creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El esquema [Logging] ya existe.';
END
GO

-- Verificar si la tabla ya existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Logs' AND schema_id = SCHEMA_ID('Logging'))
BEGIN
    CREATE TABLE [Logging].[Logs]
    (
        [Id] INT IDENTITY(1,1) PRIMARY KEY,
        [Timestamp] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [LogLevel] NVARCHAR(50) NOT NULL,
        [Category] NVARCHAR(500) NULL,
        [Message] NVARCHAR(MAX) NULL,
        [Exception] NVARCHAR(MAX) NULL,
        [Environment] NVARCHAR(50) NULL,
        [MachineName] NVARCHAR(100) NULL,
        [ServiceName] NVARCHAR(100) NULL,

        INDEX IX_Logs_Timestamp ([Timestamp] DESC),
        INDEX IX_Logs_LogLevel ([LogLevel]),
        INDEX IX_Logs_Environment ([Environment]),
        INDEX IX_Logs_ServiceName ([ServiceName])
    );

    PRINT 'Tabla [Logging].[Logs] creada exitosamente con índices.';
END
ELSE
BEGIN
    PRINT 'La tabla [Logging].[Logs] ya existe.';
END
GO

-- Mostrar estructura de la tabla
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    CHARACTER_MAXIMUM_LENGTH,
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Logging' AND TABLE_NAME = 'Logs'
ORDER BY ORDINAL_POSITION;
GO

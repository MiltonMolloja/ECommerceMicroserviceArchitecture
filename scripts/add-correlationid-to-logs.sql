-- Add CorrelationId column to Logging.Logs table
USE [ECommerce]
GO

-- Check if column already exists
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID('[Logging].[Logs]')
    AND name = 'CorrelationId'
)
BEGIN
    ALTER TABLE [Logging].[Logs]
    ADD [CorrelationId] NVARCHAR(100) NULL;

    PRINT 'Column CorrelationId added successfully to [Logging].[Logs]';
END
ELSE
BEGIN
    PRINT 'Column CorrelationId already exists in [Logging].[Logs]';
END
GO

-- Create index on CorrelationId for faster queries
IF NOT EXISTS (
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Logs_CorrelationId'
    AND object_id = OBJECT_ID('[Logging].[Logs]')
)
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Logs_CorrelationId]
    ON [Logging].[Logs] ([CorrelationId])
    INCLUDE ([Timestamp], [LogLevel], [Message]);

    PRINT 'Index IX_Logs_CorrelationId created successfully';
END
ELSE
BEGIN
    PRINT 'Index IX_Logs_CorrelationId already exists';
END
GO

-- Display current table structure
SELECT
    COLUMN_NAME,
    DATA_TYPE,
    IS_NULLABLE,
    CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Logging'
AND TABLE_NAME = 'Logs'
ORDER BY ORDINAL_POSITION;
GO

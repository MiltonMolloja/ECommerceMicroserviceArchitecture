-- Script para agregar la columna UserAgent a la tabla RefreshTokens
-- Ejecutar en SQL Server Management Studio o Azure Data Studio

USE [ECommerceDb]
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT 1
    FROM sys.columns
    WHERE object_id = OBJECT_ID(N'[Identity].[RefreshTokens]')
    AND name = 'UserAgent'
)
BEGIN
    -- Agregar la columna UserAgent
    ALTER TABLE [Identity].[RefreshTokens]
    ADD [UserAgent] NVARCHAR(500) NULL;

    PRINT 'Columna UserAgent agregada exitosamente a RefreshTokens';
END
ELSE
BEGIN
    PRINT 'La columna UserAgent ya existe en RefreshTokens';
END
GO

-- Script para configurar Full-Text Search
-- Ejecutar manualmente en SQL Server Management Studio o usando sqlcmd

USE [ECommerceDb];
GO

-- Crear Full-Text Catalog si no existe
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'CatalogFTS')
BEGIN
    CREATE FULLTEXT CATALOG CatalogFTS AS DEFAULT;
END
GO

-- Crear Full-Text Index en la tabla Products
IF NOT EXISTS (SELECT * FROM sys.fulltext_indexes WHERE object_id = OBJECT_ID('[Catalog].[Products]'))
BEGIN
    CREATE FULLTEXT INDEX ON [Catalog].[Products](
        [NameSpanish] LANGUAGE 3082,        -- Spanish
        [NameEnglish] LANGUAGE 1033,        -- English
        [DescriptionSpanish] LANGUAGE 3082, -- Spanish
        [DescriptionEnglish] LANGUAGE 1033, -- English
        [SKU] LANGUAGE 1033                 -- English
    )
    KEY INDEX PK_Products
    ON CatalogFTS
    WITH CHANGE_TRACKING AUTO;
END
GO

PRINT 'Full-Text Search configurado correctamente';
GO

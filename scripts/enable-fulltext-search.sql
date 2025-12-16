-- =====================================================
-- Script: Configurar Full-Text Search en SQL Server
-- Descripción: Habilita búsqueda de texto completo en la tabla Products
-- =====================================================

USE CatalogDb;
GO

-- Verificar si Full-Text Search está instalado
IF FULLTEXTSERVICEPROPERTY('IsFullTextInstalled') = 0
BEGIN
    PRINT 'ERROR: Full-Text Search no está instalado en esta instancia de SQL Server.';
    PRINT 'Instala el componente "Full-Text and Semantic Extractions for Search" desde SQL Server Setup.';
    RETURN;
END
GO

-- 1. Crear Full-Text Catalog si no existe
IF NOT EXISTS (SELECT * FROM sys.fulltext_catalogs WHERE name = 'CatalogFullTextCatalog')
BEGIN
    CREATE FULLTEXT CATALOG CatalogFullTextCatalog AS DEFAULT;
    PRINT 'Full-Text Catalog creado: CatalogFullTextCatalog';
END
ELSE
BEGIN
    PRINT 'Full-Text Catalog ya existe: CatalogFullTextCatalog';
END
GO

-- 2. Verificar que la tabla Products tenga una Primary Key o Unique Index
-- (Requerido para crear Full-Text Index)
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE object_id = OBJECT_ID('Catalog.Products') 
    AND is_primary_key = 1
)
BEGIN
    PRINT 'ERROR: La tabla Catalog.Products no tiene Primary Key.';
    PRINT 'Full-Text Index requiere una Primary Key o Unique Index.';
    RETURN;
END
GO

-- 3. Eliminar Full-Text Index existente si hay uno
IF EXISTS (
    SELECT * FROM sys.fulltext_indexes 
    WHERE object_id = OBJECT_ID('Catalog.Products')
)
BEGIN
    DROP FULLTEXT INDEX ON Catalog.Products;
    PRINT 'Full-Text Index existente eliminado.';
END
GO

-- 4. Crear Full-Text Index en la tabla Products
CREATE FULLTEXT INDEX ON Catalog.Products
(
    NameSpanish LANGUAGE 3082,        -- Español (3082)
    NameEnglish LANGUAGE 1033,        -- Inglés (1033)
    DescriptionSpanish LANGUAGE 3082,
    DescriptionEnglish LANGUAGE 1033,
    SKU LANGUAGE 1033
)
KEY INDEX PK_Products                 -- Nombre de tu Primary Key
ON CatalogFullTextCatalog
WITH CHANGE_TRACKING AUTO;            -- Auto-actualización del índice
GO

PRINT 'Full-Text Index creado exitosamente en Catalog.Products';
GO

-- 5. Verificar el estado del Full-Text Index
SELECT 
    OBJECT_NAME(object_id) AS TableName,
    is_enabled,
    change_tracking_state_desc,
    crawl_type_desc,
    crawl_start_date,
    crawl_end_date
FROM sys.fulltext_indexes
WHERE object_id = OBJECT_ID('Catalog.Products');
GO

-- 6. Poblar el índice (forzar población inicial)
ALTER FULLTEXT INDEX ON Catalog.Products START FULL POPULATION;
GO

PRINT 'Full-Text Index población iniciada.';
PRINT 'Puedes verificar el progreso con:';
PRINT 'SELECT * FROM sys.dm_fts_index_population WHERE database_id = DB_ID()';
GO

-- =====================================================
-- Verificación del progreso de población
-- =====================================================
PRINT '';
PRINT '--- Estado de población del índice ---';
SELECT 
    DB_NAME(database_id) AS DatabaseName,
    OBJECT_NAME(table_id) AS TableName,
    status_description,
    completion_type_description,
    start_time,
    range_count,
    CASE 
        WHEN range_count > 0 
        THEN CAST((range_count_completed * 100.0 / range_count) AS DECIMAL(5,2))
        ELSE 100.0 
    END AS PercentComplete
FROM sys.dm_fts_index_population
WHERE database_id = DB_ID();
GO

-- =====================================================
-- Ejemplos de uso de Full-Text Search
-- =====================================================
PRINT '';
PRINT '--- Ejemplos de consultas con Full-Text Search ---';
PRINT '';
PRINT '-- Búsqueda simple:';
PRINT 'SELECT ProductId, NameSpanish, NameEnglish';
PRINT 'FROM Catalog.Products';
PRINT 'WHERE CONTAINS((NameSpanish, NameEnglish), ''laptop'');';
PRINT '';
PRINT '-- Búsqueda con operadores (AND, OR, NEAR):';
PRINT 'SELECT ProductId, NameSpanish';
PRINT 'FROM Catalog.Products';
PRINT 'WHERE CONTAINS(NameSpanish, ''laptop AND gaming'');';
PRINT '';
PRINT '-- Búsqueda con comodines:';
PRINT 'SELECT ProductId, NameSpanish';
PRINT 'FROM Catalog.Products';
PRINT 'WHERE CONTAINS(NameSpanish, ''"compu*"'');';
PRINT '';
PRINT '-- FREETEXT (búsqueda más flexible):';
PRINT 'SELECT ProductId, NameSpanish';
PRINT 'FROM Catalog.Products';
PRINT 'WHERE FREETEXT((NameSpanish, DescriptionSpanish), ''computadora portátil para juegos'');';
GO

-- =====================================================
-- Mantenimiento del Full-Text Index
-- =====================================================
PRINT '';
PRINT '--- Comandos de mantenimiento ---';
PRINT '';
PRINT '-- Reorganizar el índice:';
PRINT 'ALTER FULLTEXT INDEX ON Catalog.Products REORGANIZE;';
PRINT '';
PRINT '-- Población incremental (solo cambios):';
PRINT 'ALTER FULLTEXT INDEX ON Catalog.Products START INCREMENTAL POPULATION;';
PRINT '';
PRINT '-- Población completa (recrear todo):';
PRINT 'ALTER FULLTEXT INDEX ON Catalog.Products START FULL POPULATION;';
PRINT '';
PRINT '-- Pausar y reanudar tracking:';
PRINT 'ALTER FULLTEXT INDEX ON Catalog.Products SET CHANGE_TRACKING MANUAL;';
PRINT 'ALTER FULLTEXT INDEX ON Catalog.Products SET CHANGE_TRACKING AUTO;';
GO

-- =====================================================
-- Verificación final
-- =====================================================
PRINT '';
PRINT '=== Configuración completada ===';
PRINT 'Full-Text Search está habilitado en Catalog.Products';
PRINT 'Columnas indexadas: NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, SKU';
PRINT '';
PRINT 'NOTA: La población del índice puede tardar varios minutos dependiendo del tamaño de la tabla.';
PRINT 'Verifica el progreso con la query de población mostrada arriba.';
GO

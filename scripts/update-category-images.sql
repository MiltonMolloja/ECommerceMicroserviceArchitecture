-- =============================================
-- Script: Actualizar imágenes de categorías faltantes
-- Descripción: Agrega URLs de imágenes de Unsplash para las categorías sin imagen
-- Fecha: 2025-12-13
-- =============================================

USE ECommerceDb;
GO

-- Verificar categorías actuales
SELECT CategoryId, NameSpanish, NameEnglish, ImageUrl
FROM Catalog.Categories
WHERE ImageUrl IS NULL OR ImageUrl = '';
GO

-- Actualizar imágenes de categorías
UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=400'
WHERE CategoryId = 2 AND NameSpanish = 'Periféricos';

UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1591799264318-7e6ef8ddb7ea?w=400'
WHERE CategoryId = 4 AND NameSpanish = 'Componentes';

UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1527443224154-c4a3942d3acf?w=400'
WHERE CategoryId = 5 AND NameSpanish = 'Monitores';

UPDATE Catalog.Categories
SET ImageUrl = 'https://images.unsplash.com/photo-1555041469-a586c61ea9bc?w=400'
WHERE CategoryId = 6 AND NameSpanish = 'Mobiliario';

GO

-- Verificar actualización
SELECT 
    CategoryId,
    NameSpanish,
    NameEnglish,
    ImageUrl,
    IsFeatured,
    IsActive
FROM Catalog.Categories
ORDER BY DisplayOrder;
GO

PRINT '✅ Imágenes de categorías actualizadas correctamente';
GO

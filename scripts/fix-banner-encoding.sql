-- =============================================
-- Script: Corregir codificación de caracteres en Banners
-- Problema: Caracteres especiales mal codificados (UTF-8 → Latin1)
-- Fecha: 2025-12-13
-- =============================================

USE ECommerceDb;
GO

-- Ver estado actual de los banners
SELECT BannerId, Title, Subtitle 
FROM Catalog.Banners;
GO

-- Corregir textos con problemas de codificación
-- "Ãº" → "ú"
-- "Ã­" → "í"
-- "Ã±" → "ñ"
-- "Ã¡" → "á"
-- "Ã©" → "é"
-- "Ã³" → "ó"

UPDATE Catalog.Banners
SET Title = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
    Title,
    'Ãº', 'ú'),
    'Ã­', 'í'),
    'Ã±', 'ñ'),
    'Ã¡', 'á'),
    'Ã©', 'é'),
    'Ã³', 'ó'),
    'Ã', 'í')
WHERE Title LIKE '%Ã%';

UPDATE Catalog.Banners
SET Subtitle = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(
    Subtitle,
    'Ãº', 'ú'),
    'Ã­', 'í'),
    'Ã±', 'ñ'),
    'Ã¡', 'á'),
    'Ã©', 'é'),
    'Ã³', 'ó'),
    'Ã', 'í')
WHERE Subtitle LIKE '%Ã%';

GO

-- Verificar corrección
SELECT BannerId, Title, Subtitle 
FROM Catalog.Banners;
GO

-- Si los REPLACE no funcionan correctamente, usar UPDATE directo:
-- Descomenta y ajusta según tus banners

/*
UPDATE Catalog.Banners
SET Title = 'Descubre lo último en tecnología'
WHERE BannerId = 1;

UPDATE Catalog.Banners
SET Subtitle = 'Las mejores ofertas en electrónica'
WHERE BannerId = 1;
*/

PRINT '✅ Codificación de banners corregida';
GO

-- =============================================
-- Script: Corregir textos de Banners (UPDATE directo)
-- Ejecutar si fix-banner-encoding.sql no funciona
-- Fecha: 2025-12-13
-- =============================================

USE ECommerceDb;
GO

-- Primero ver los banners actuales
SELECT BannerId, Title, Subtitle, ButtonText, ButtonUrl, ImageUrl
FROM Catalog.Banners
ORDER BY DisplayOrder;
GO

-- Actualizar Banner 1 (Tecnología)
UPDATE Catalog.Banners
SET 
    Title = N'Descubre lo último en tecnología',
    Subtitle = N'Las mejores ofertas en electrónica'
WHERE BannerId = 1;

-- Actualizar Banner 2 (si existe y tiene problemas)
UPDATE Catalog.Banners
SET 
    Title = N'Ofertas especiales',
    Subtitle = N'Hasta 50% de descuento'
WHERE BannerId = 2 AND Title LIKE '%Ã%';

-- Actualizar Banner 3 (si existe y tiene problemas)
UPDATE Catalog.Banners
SET 
    Title = N'Envío gratis',
    Subtitle = N'En compras mayores a $25'
WHERE BannerId = 3 AND Title LIKE '%Ã%';

GO

-- Verificar corrección
SELECT BannerId, Title, Subtitle, ButtonText
FROM Catalog.Banners
ORDER BY DisplayOrder;
GO

PRINT '✅ Textos de banners actualizados correctamente';
GO

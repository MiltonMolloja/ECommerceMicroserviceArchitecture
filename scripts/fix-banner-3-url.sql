-- =============================================
-- Script: Fix Banner 3 URL
-- Fecha: 2025-12-13
-- Descripción: Cambiar /deals por /s?hasDiscount=true
-- =============================================

USE ECommerceDb;
GO

PRINT 'Actualizando URL del Banner 3...';

UPDATE [Catalog].[Banners]
SET [LinkUrl] = '/s?hasDiscount=true',
    [UpdatedAt] = GETUTCDATE()
WHERE [TitleEnglish] = 'Special Offers'
  AND [LinkUrl] = '/deals';

PRINT 'Banner 3 actualizado exitosamente.';

-- Verificar resultado
SELECT 
    BannerId,
    TitleSpanish,
    TitleEnglish,
    LinkUrl,
    UpdatedAt
FROM [Catalog].[Banners]
WHERE [TitleEnglish] = 'Special Offers';
GO

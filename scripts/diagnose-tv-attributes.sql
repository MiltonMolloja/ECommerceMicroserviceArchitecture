-- Diagnóstico de atributos para TVs
USE ECommerceDb;
GO

PRINT '========================================';
PRINT 'DIAGNÓSTICO DE ATRIBUTOS PARA TVs';
PRINT '========================================';
PRINT '';

-- 1. Verificar atributos filtrables
PRINT '1. ATRIBUTOS FILTRABLES:';
PRINT '----------------------------------------';
SELECT 
    AttributeId,
    AttributeName,
    AttributeNameEnglish,
    AttributeType,
    IsFilterable,
    IsSearchable,
    DisplayOrder
FROM Catalog.ProductAttributes
WHERE IsFilterable = 1
ORDER BY DisplayOrder;
PRINT '';

-- 2. Verificar productos que contienen "TV" en el nombre
PRINT '2. PRODUCTOS CON "TV" EN EL NOMBRE:';
PRINT '----------------------------------------';
SELECT TOP 10
    ProductId,
    NameSpanish AS Name,
    BrandId
FROM Catalog.Products
WHERE NameSpanish LIKE '%TV%' OR NameEnglish LIKE '%TV%' OR NameSpanish LIKE '%television%' OR NameEnglish LIKE '%television%'
ORDER BY ProductId;
PRINT '';

-- 3. Verificar atributos asignados a productos de TV
PRINT '3. ATRIBUTOS ASIGNADOS A PRODUCTOS DE TV:';
PRINT '----------------------------------------';
SELECT 
    p.ProductId,
    p.NameSpanish AS ProductName,
    pa.AttributeName,
    av.ValueText,
    pav.NumericValue
FROM Catalog.Products p
INNER JOIN Catalog.ProductAttributeValues pav ON p.ProductId = pav.ProductId
INNER JOIN Catalog.ProductAttributes pa ON pav.AttributeId = pa.AttributeId
LEFT JOIN Catalog.AttributeValues av ON pav.ValueId = av.ValueId
WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
ORDER BY p.ProductId, pa.AttributeName;
PRINT '';

-- 4. Contar productos de TV por atributo
PRINT '4. CONTEO DE PRODUCTOS DE TV POR ATRIBUTO:';
PRINT '----------------------------------------';
SELECT 
    pa.AttributeName,
    pa.IsFilterable,
    COUNT(DISTINCT pav.ProductId) AS TVProductCount
FROM Catalog.ProductAttributes pa
LEFT JOIN Catalog.ProductAttributeValues pav ON pa.AttributeId = pav.AttributeId
LEFT JOIN Catalog.Products p ON pav.ProductId = p.ProductId
WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
GROUP BY pa.AttributeName, pa.IsFilterable
ORDER BY TVProductCount DESC;
PRINT '';

-- 5. Verificar si hay valores de atributos para atributos filtrables
PRINT '5. VALORES DE ATRIBUTOS FILTRABLES:';
PRINT '----------------------------------------';
SELECT 
    pa.AttributeName,
    av.ValueText,
    av.ValueTextEnglish,
    COUNT(DISTINCT pav.ProductId) AS ProductCount
FROM Catalog.ProductAttributes pa
INNER JOIN Catalog.AttributeValues av ON pa.AttributeId = av.AttributeId
LEFT JOIN Catalog.ProductAttributeValues pav ON av.AttributeValueId = pav.AttributeValueId
WHERE pa.IsFilterable = 1
GROUP BY pa.AttributeName, av.ValueText, av.ValueTextEnglish
ORDER BY pa.AttributeName, ProductCount DESC;
PRINT '';

-- 6. Verificar productos de TV SIN atributos
PRINT '6. PRODUCTOS DE TV SIN ATRIBUTOS:';
PRINT '----------------------------------------';
SELECT TOP 10
    p.ProductId,
    p.NameSpanish AS Name
FROM Catalog.Products p
WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
AND NOT EXISTS (
    SELECT 1 
    FROM Catalog.ProductAttributeValues pav 
    WHERE pav.ProductId = p.ProductId
)
ORDER BY p.ProductId;
PRINT '';

-- 7. Verificar estructura de tablas
PRINT '7. ESTRUCTURA DE TABLAS:';
PRINT '----------------------------------------';
PRINT 'Tabla ProductAttributes:';
SELECT COUNT(*) AS TotalAttributes FROM Catalog.ProductAttributes;
PRINT 'Tabla AttributeValues:';
SELECT COUNT(*) AS TotalAttributeValues FROM Catalog.AttributeValues;
PRINT 'Tabla ProductAttributeValues:';
SELECT COUNT(*) AS TotalProductAttributeValues FROM Catalog.ProductAttributeValues;
PRINT '';

PRINT '========================================';
PRINT 'DIAGNÓSTICO COMPLETADO';
PRINT '========================================';

-- Verificar atributos y valores en la base de datos
USE ECommerce;
GO

PRINT '========================================';
PRINT 'VERIFICACIÓN DE ATRIBUTOS Y VALORES';
PRINT '========================================';
PRINT '';

-- 1. Verificar atributos creados
PRINT '1. ATRIBUTOS CREADOS:';
PRINT '----------------------------------------';
SELECT 
    AttributeId,
    AttributeName,
    AttributeNameEnglish,
    AttributeType,
    IsFilterable,
    IsSearchable
FROM Catalog.ProductAttributes
ORDER BY DisplayOrder;
PRINT '';

-- 2. Verificar valores de atributos
PRINT '2. VALORES DE ATRIBUTOS:';
PRINT '----------------------------------------';
SELECT 
    av.AttributeValueId,
    pa.AttributeName,
    av.ValueText,
    av.ValueTextEnglish
FROM Catalog.AttributeValues av
INNER JOIN Catalog.ProductAttributes pa ON av.AttributeId = pa.AttributeId
ORDER BY pa.AttributeName, av.DisplayOrder;
PRINT '';

-- 3. Verificar productos con atributos asignados
PRINT '3. PRODUCTOS CON ATRIBUTOS:';
PRINT '----------------------------------------';
SELECT 
    p.ProductId,
    p.Name,
    pa.AttributeName,
    av.ValueText,
    pav.ValueNumeric
FROM Catalog.ProductAttributeValues pav
INNER JOIN Catalog.Products p ON pav.ProductId = p.ProductId
INNER JOIN Catalog.ProductAttributes pa ON pav.AttributeId = pa.AttributeId
LEFT JOIN Catalog.AttributeValues av ON pav.AttributeValueId = av.AttributeValueId
ORDER BY p.ProductId, pa.AttributeName;
PRINT '';

-- 4. Contar productos por atributo
PRINT '4. CONTEO DE PRODUCTOS POR ATRIBUTO:';
PRINT '----------------------------------------';
SELECT 
    pa.AttributeName,
    COUNT(DISTINCT pav.ProductId) AS ProductCount
FROM Catalog.ProductAttributes pa
LEFT JOIN Catalog.ProductAttributeValues pav ON pa.AttributeId = pav.AttributeId
GROUP BY pa.AttributeName
ORDER BY pa.AttributeName;
PRINT '';

-- 5. Verificar productos sin atributos
PRINT '5. PRODUCTOS SIN ATRIBUTOS:';
PRINT '----------------------------------------';
SELECT 
    p.ProductId,
    p.Name
FROM Catalog.Products p
WHERE NOT EXISTS (
    SELECT 1 
    FROM Catalog.ProductAttributeValues pav 
    WHERE pav.ProductId = p.ProductId
)
ORDER BY p.ProductId;
PRINT '';

PRINT '========================================';
PRINT 'VERIFICACIÓN COMPLETADA';
PRINT '========================================';

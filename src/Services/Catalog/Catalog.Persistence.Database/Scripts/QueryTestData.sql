-- ===============================================
-- Consultas para verificar datos de prueba
-- ===============================================

USE ECommerceDb;
GO

PRINT '=============================================';
PRINT 'VERIFICACIÓN DE DATOS DE PRUEBA';
PRINT '=============================================';
PRINT '';

-- Ver productos insertados
PRINT '1. PRODUCTOS INSERTADOS (IDs 201-250):';
PRINT '---------------------------------------------';
SELECT
    ProductId,
    NameSpanish AS [Nombre],
    Price AS [Precio],
    Stock,
    Brand AS [Marca]
FROM Catalog.Products
WHERE ProductId BETWEEN 201 AND 250
ORDER BY ProductId;

PRINT '';
PRINT '2. ATRIBUTOS DISPONIBLES:';
PRINT '---------------------------------------------';
SELECT
    AttributeId,
    AttributeName AS [Nombre],
    AttributeType AS [Tipo],
    IsFilterable AS [Filtrable]
FROM Catalog.ProductAttributes
WHERE AttributeId BETWEEN 101 AND 108
ORDER BY DisplayOrder;

PRINT '';
PRINT '3. VALORES DE ATRIBUTOS:';
PRINT '---------------------------------------------';
SELECT
    pa.AttributeName,
    av.ValueText,
    COUNT(*) AS [Cantidad Productos]
FROM Catalog.AttributeValues av
INNER JOIN Catalog.ProductAttributes pa ON av.AttributeId = pa.AttributeId
WHERE pa.AttributeId BETWEEN 101 AND 108
GROUP BY pa.AttributeName, av.ValueText
ORDER BY pa.AttributeName, av.DisplayOrder;

PRINT '';
PRINT '4. RATINGS POR PRODUCTO (Top 10 mejor calificados):';
PRINT '---------------------------------------------';
SELECT TOP 10
    p.ProductId,
    p.NameSpanish AS [Producto],
    pr.AverageRating AS [Rating Promedio],
    pr.TotalReviews AS [Total Reviews],
    pr.Rating5Star AS [5★],
    pr.Rating4Star AS [4★],
    pr.Rating3Star AS [3★]
FROM Catalog.ProductRatings pr
INNER JOIN Catalog.Products p ON pr.ProductId = p.ProductId
WHERE p.ProductId BETWEEN 201 AND 250
ORDER BY pr.AverageRating DESC, pr.TotalReviews DESC;

PRINT '';
PRINT '5. REVIEWS RECIENTES (Últimas 10):';
PRINT '---------------------------------------------';
SELECT TOP 10
    p.NameSpanish AS [Producto],
    pr.Rating,
    pr.Title AS [Título],
    pr.IsVerifiedPurchase AS [Compra Verificada],
    pr.CreatedAt AS [Fecha]
FROM Catalog.ProductReviews pr
INNER JOIN Catalog.Products p ON pr.ProductId = p.ProductId
WHERE p.ProductId BETWEEN 201 AND 250
ORDER BY pr.CreatedAt DESC;

PRINT '';
PRINT '6. ESTADÍSTICAS GENERALES:';
PRINT '---------------------------------------------';
SELECT
    'Total Productos' AS [Métrica],
    CAST(COUNT(*) AS VARCHAR) AS [Valor]
FROM Catalog.Products WHERE ProductId BETWEEN 201 AND 250
UNION ALL
SELECT 'Total Reviews', CAST(COUNT(*) AS VARCHAR)
FROM Catalog.ProductReviews WHERE ProductId BETWEEN 201 AND 250
UNION ALL
SELECT 'Rating Promedio General', CAST(AVG(AverageRating) AS VARCHAR(10))
FROM Catalog.ProductRatings WHERE ProductId BETWEEN 201 AND 250
UNION ALL
SELECT 'Productos con Rating >4.0', CAST(COUNT(*) AS VARCHAR)
FROM Catalog.ProductRatings WHERE ProductId BETWEEN 201 AND 250 AND AverageRating > 4.0;

PRINT '';
PRINT '7. PRODUCTOS POR CATEGORÍA/RANGO:';
PRINT '---------------------------------------------';
SELECT
    CASE
        WHEN ProductId BETWEEN 201 AND 210 THEN 'Smartphones'
        WHEN ProductId BETWEEN 211 AND 220 THEN 'Laptops'
        WHEN ProductId BETWEEN 221 AND 230 THEN 'Zapatillas'
        WHEN ProductId BETWEEN 231 AND 240 THEN 'TVs'
        WHEN ProductId BETWEEN 241 AND 250 THEN 'Accesorios'
        ELSE 'Otros'
    END AS [Categoría],
    COUNT(*) AS [Total],
    MIN(Price) AS [Precio Mín],
    MAX(Price) AS [Precio Máx],
    AVG(Price) AS [Precio Promedio]
FROM Catalog.Products
WHERE ProductId BETWEEN 201 AND 250
GROUP BY
    CASE
        WHEN ProductId BETWEEN 201 AND 210 THEN 'Smartphones'
        WHEN ProductId BETWEEN 211 AND 220 THEN 'Laptops'
        WHEN ProductId BETWEEN 221 AND 230 THEN 'Zapatillas'
        WHEN ProductId BETWEEN 231 AND 240 THEN 'TVs'
        WHEN ProductId BETWEEN 241 AND 250 THEN 'Accesorios'
        ELSE 'Otros'
    END
ORDER BY [Categoría];

PRINT '';
PRINT '=============================================';
PRINT 'FIN VERIFICACIÓN';
PRINT '=============================================';
GO

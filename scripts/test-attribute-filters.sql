-- Test: Verificar filtros de atributos
-- Este script verifica que los productos con atributos específicos existen

USE [ecommerce-db];
GO

PRINT '=== Test de Filtros de Atributos ==='
PRINT ''

-- 1. Verificar que el atributo 107 existe
PRINT '1. Verificando atributo 107...'
SELECT 
    AttributeId,
    AttributeName,
    AttributeType,
    IsFilterable
FROM ProductAttributes
WHERE AttributeId = 107;
PRINT ''

-- 2. Verificar valores del atributo 107
PRINT '2. Valores disponibles para atributo 107...'
SELECT 
    ValueId,
    AttributeId,
    ValueSpanish,
    ValueEnglish,
    SortOrder
FROM AttributeValues
WHERE AttributeId = 107
ORDER BY SortOrder;
PRINT ''

-- 3. Contar productos con atributo 107 y valores específicos
PRINT '3. Productos con atributo 107 = 1056 o 1036...'
SELECT COUNT(DISTINCT pav.ProductId) as TotalProducts
FROM ProductAttributeValues pav
WHERE pav.AttributeId = 107
  AND pav.ValueId IN (1056, 1036);
PRINT ''

-- 4. Listar productos específicos con sus valores
PRINT '4. Detalle de productos filtrados...'
SELECT 
    p.ProductId,
    p.NameSpanish,
    p.Price,
    pav.AttributeId,
    av.ValueSpanish as AttributeValue,
    pav.ValueId
FROM Products p
INNER JOIN ProductAttributeValues pav ON p.ProductId = pav.ProductId
LEFT JOIN AttributeValues av ON pav.ValueId = av.ValueId
WHERE pav.AttributeId = 107
  AND pav.ValueId IN (1056, 1036)
  AND p.IsActive = 1
  AND p.NameSpanish LIKE '%tv%'
ORDER BY p.NameSpanish;
PRINT ''

-- 5. Verificar que hay stock disponible
PRINT '5. Productos con stock disponible...'
SELECT 
    p.ProductId,
    p.NameSpanish,
    ps.Stock
FROM Products p
INNER JOIN ProductAttributeValues pav ON p.ProductId = pav.ProductId
LEFT JOIN ProductInStock ps ON p.ProductId = ps.ProductId
WHERE pav.AttributeId = 107
  AND pav.ValueId IN (1056, 1036)
  AND p.IsActive = 1
  AND p.NameSpanish LIKE '%tv%'
ORDER BY p.NameSpanish;
PRINT ''

-- 6. Simular el WHERE que genera EF Core
PRINT '6. Simulando query EF Core (subconsulta con EXISTS)...'
SELECT 
    p.ProductId,
    p.NameSpanish,
    p.Price,
    p.IsActive
FROM Products p
WHERE p.IsActive = 1
  AND p.NameSpanish LIKE '%tv%'
  AND EXISTS (
      SELECT 1
      FROM ProductAttributeValues pav
      WHERE pav.ProductId = p.ProductId
        AND pav.AttributeId = 107
        AND pav.ValueId IN (1056, 1036)
  )
ORDER BY p.NameSpanish;

PRINT ''
PRINT '=== Fin del Test ==='

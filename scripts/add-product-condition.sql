-- Script: Agregar atributo "Condición" y marcar 25% de productos como usados
-- Fecha: 2025-01-03
-- Descripción: Crea el atributo Condición (Nuevo/Usado) y asigna valores aleatorios

USE [ECommerceDb];
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '=== Agregando atributo Condición ==='
PRINT ''

-- 1. Verificar si el atributo ya existe
IF EXISTS (SELECT 1 FROM Catalog.ProductAttributes WHERE AttributeName = 'Condition')
BEGIN
    PRINT '⚠️  El atributo "Condition" ya existe. Eliminando datos anteriores...'
    
    -- Eliminar valores de productos
    DELETE FROM Catalog.ProductAttributeValues WHERE AttributeId = (SELECT AttributeId FROM Catalog.ProductAttributes WHERE AttributeName = 'Condition');
    
    -- Eliminar valores del atributo
    DELETE FROM Catalog.AttributeValues WHERE AttributeId = (SELECT AttributeId FROM Catalog.ProductAttributes WHERE AttributeName = 'Condition');
    
    -- Eliminar el atributo
    DELETE FROM Catalog.ProductAttributes WHERE AttributeName = 'Condition';
    
    PRINT '✅ Datos anteriores eliminados'
    PRINT ''
END

-- 2. Crear el atributo "Condition"
PRINT '1. Creando atributo "Condition"...'

DECLARE @ConditionAttributeId INT;

INSERT INTO Catalog.ProductAttributes (AttributeName, AttributeType, IsRequired, IsFilterable, IsSearchable, SortOrder)
VALUES ('Condition', 'Select', 0, 1, 1, 2); -- SortOrder 2 para que aparezca al inicio

SET @ConditionAttributeId = SCOPE_IDENTITY();

PRINT '   ✅ AttributeId creado: ' + CAST(@ConditionAttributeId AS VARCHAR(10))
PRINT ''

-- 3. Crear valores para el atributo (Nuevo, Usado)
PRINT '2. Creando valores del atributo...'

DECLARE @NuevoValueId INT;
DECLARE @UsadoValueId INT;

-- Nuevo
INSERT INTO Catalog.AttributeValues (AttributeId, ValueSpanish, ValueEnglish, SortOrder)
VALUES (@ConditionAttributeId, 'Nuevo', 'New', 1);

SET @NuevoValueId = SCOPE_IDENTITY();
PRINT '   ✅ Valor "Nuevo/New" creado: ' + CAST(@NuevoValueId AS VARCHAR(10))

-- Usado
INSERT INTO Catalog.AttributeValues (AttributeId, ValueSpanish, ValueEnglish, SortOrder)
VALUES (@ConditionAttributeId, 'Usado', 'Used', 2);

SET @UsadoValueId = SCOPE_IDENTITY();
PRINT '   ✅ Valor "Usado/Used" creado: ' + CAST(@UsadoValueId AS VARCHAR(10))
PRINT ''

-- 4. Obtener total de productos activos
DECLARE @TotalProducts INT;
SELECT @TotalProducts = COUNT(*) FROM Catalog.Products WHERE IsActive = 1;

PRINT '3. Total de productos activos: ' + CAST(@TotalProducts AS VARCHAR(10))
PRINT ''

-- 5. Calcular 25% para productos usados
DECLARE @UsedCount INT = CAST(@TotalProducts * 0.25 AS INT);
PRINT '4. Productos que serán marcados como usados (25%): ' + CAST(@UsedCount AS VARCHAR(10))
PRINT ''

-- 6. Asignar condición "Usado" al 25% de productos (selección aleatoria)
PRINT '5. Asignando condición "Usado" a productos aleatorios...'

INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
SELECT TOP (@UsedCount)
    ProductId,
    @ConditionAttributeId,
    @UsadoValueId
FROM Catalog.Products
WHERE IsActive = 1
ORDER BY NEWID(); -- Orden aleatorio

DECLARE @UsedAssigned INT = @@ROWCOUNT;
PRINT '   ✅ Productos marcados como "Usado": ' + CAST(@UsedAssigned AS VARCHAR(10))
PRINT ''

-- 7. Asignar condición "Nuevo" al resto de productos
PRINT '6. Asignando condición "Nuevo" a productos restantes...'

INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
SELECT 
    p.ProductId,
    @ConditionAttributeId,
    @NuevoValueId
FROM Catalog.Products p
WHERE p.IsActive = 1
  AND NOT EXISTS (
      SELECT 1 
      FROM Catalog.ProductAttributeValues pav 
      WHERE pav.ProductId = p.ProductId 
        AND pav.AttributeId = @ConditionAttributeId
  );

DECLARE @NewAssigned INT = @@ROWCOUNT;
PRINT '   ✅ Productos marcados como "Nuevo": ' + CAST(@NewAssigned AS VARCHAR(10))
PRINT ''

-- 8. Verificar distribución final
PRINT '7. Verificando distribución final...'
PRINT ''

SELECT 
    av.ValueSpanish as Condicion,
    av.ValueEnglish as Condition,
    COUNT(*) as TotalProductos,
    CAST(COUNT(*) * 100.0 / @TotalProducts AS DECIMAL(5,2)) as Porcentaje
FROM Catalog.ProductAttributeValues pav
INNER JOIN Catalog.AttributeValues av ON pav.ValueId = av.ValueId
WHERE pav.AttributeId = @ConditionAttributeId
GROUP BY av.ValueSpanish, av.ValueEnglish
ORDER BY av.SortOrder;

PRINT ''
PRINT '8. Listado de productos usados (primeros 20)...'
PRINT ''

SELECT TOP 20
    p.ProductId,
    p.NameSpanish,
    p.Price,
    av.ValueSpanish as Condicion
FROM Catalog.Products p
INNER JOIN Catalog.ProductAttributeValues pav ON p.ProductId = pav.ProductId
INNER JOIN Catalog.AttributeValues av ON pav.ValueId = av.ValueId
WHERE pav.AttributeId = @ConditionAttributeId
  AND av.ValueEnglish = 'Used'
ORDER BY p.NameSpanish;

PRINT ''
PRINT '=== ✅ Proceso Completado ==='
PRINT ''
PRINT 'Resumen:'
PRINT '  - AttributeId: ' + CAST(@ConditionAttributeId AS VARCHAR(10))
PRINT '  - Valor "Nuevo": ' + CAST(@NuevoValueId AS VARCHAR(10))
PRINT '  - Valor "Usado": ' + CAST(@UsadoValueId AS VARCHAR(10))
PRINT '  - Total productos: ' + CAST(@TotalProducts AS VARCHAR(10))
PRINT '  - Productos nuevos: ' + CAST(@NewAssigned AS VARCHAR(10))
PRINT '  - Productos usados: ' + CAST(@UsedAssigned AS VARCHAR(10))
PRINT ''
PRINT 'Uso en filtros:'
PRINT '  GET /products/search?filter_attr_' + CAST(@ConditionAttributeId AS VARCHAR(10)) + '=' + CAST(@NuevoValueId AS VARCHAR(10)) + '  (Nuevos)'
PRINT '  GET /products/search?filter_attr_' + CAST(@ConditionAttributeId AS VARCHAR(10)) + '=' + CAST(@UsadoValueId AS VARCHAR(10)) + '  (Usados)'
PRINT ''

GO

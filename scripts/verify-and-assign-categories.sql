-- ============================================
-- Script: Verificar y Asignar Categorías a Productos
-- Descripción: Verifica el estado de categorías y asigna productos
-- ============================================

USE [ECommerceDb]
GO

-- 1. Verificar categorías existentes
PRINT '=== CATEGORÍAS EXISTENTES ==='
SELECT 
    CategoryId,
    NameSpanish,
    NameEnglish,
    Slug,
    ParentCategoryId,
    IsActive,
    DisplayOrder
FROM Catalog.Categories
ORDER BY ParentCategoryId, DisplayOrder
GO

-- 2. Verificar productos sin categoría
PRINT ''
PRINT '=== PRODUCTOS SIN CATEGORÍA ==='
SELECT 
    p.ProductId,
    p.NameSpanish,
    p.NameEnglish,
    COUNT(pc.CategoryId) AS CategoryCount
FROM Products p
LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId
GROUP BY p.ProductId, p.NameSpanish, p.NameEnglish
HAVING COUNT(pc.CategoryId) = 0
GO

-- 3. Contar productos por categoría
PRINT ''
PRINT '=== PRODUCTOS POR CATEGORÍA ==='
SELECT 
    c.CategoryId,
    c.NameSpanish,
    c.Slug,
    COUNT(pc.ProductId) AS ProductCount
FROM Categories c
LEFT JOIN ProductCategories pc ON c.CategoryId = pc.CategoryId
GROUP BY c.CategoryId, c.NameSpanish, c.Slug
ORDER BY ProductCount DESC
GO

-- 4. Crear categorías básicas si no existen
PRINT ''
PRINT '=== CREANDO CATEGORÍAS BÁSICAS ==='

-- Verificar si ya existen categorías
IF NOT EXISTS (SELECT 1 FROM Categories)
BEGIN
    PRINT 'Creando categorías raíz...'
    
    -- Electrónica
    INSERT INTO Categories (NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Slug, ParentCategoryId, IsActive, DisplayOrder)
    VALUES 
        ('Electrónica', 'Electronics', 'Productos electrónicos y tecnología', 'Electronic products and technology', 'electronica', NULL, 1, 1),
        ('Computadoras', 'Computers', 'Computadoras y accesorios', 'Computers and accessories', 'computadoras', NULL, 1, 2),
        ('Hogar', 'Home', 'Productos para el hogar', 'Home products', 'hogar', NULL, 1, 3),
        ('Deportes', 'Sports', 'Artículos deportivos', 'Sports articles', 'deportes', NULL, 1, 4)
    
    PRINT 'Categorías raíz creadas'
END
ELSE
BEGIN
    PRINT 'Las categorías ya existen'
END
GO

-- 5. Asignar productos a categorías automáticamente basado en palabras clave
PRINT ''
PRINT '=== ASIGNANDO PRODUCTOS A CATEGORÍAS ==='

DECLARE @ElectronicaId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'electronica')
DECLARE @ComputadorasId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'computadoras')
DECLARE @HogarId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'hogar')
DECLARE @DeportesId INT = (SELECT CategoryId FROM Categories WHERE Slug = 'deportes')

-- Asignar productos de electrónica
IF @ElectronicaId IS NOT NULL
BEGIN
    INSERT INTO ProductCategories (ProductId, CategoryId)
    SELECT DISTINCT p.ProductId, @ElectronicaId
    FROM Products p
    LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId AND pc.CategoryId = @ElectronicaId
    WHERE pc.ProductId IS NULL
      AND (
          p.NameSpanish LIKE '%TV%' OR
          p.NameSpanish LIKE '%televisor%' OR
          p.NameSpanish LIKE '%monitor%' OR
          p.NameSpanish LIKE '%audio%' OR
          p.NameSpanish LIKE '%parlante%' OR
          p.NameSpanish LIKE '%auricular%' OR
          p.NameEnglish LIKE '%TV%' OR
          p.NameEnglish LIKE '%television%' OR
          p.NameEnglish LIKE '%monitor%' OR
          p.NameEnglish LIKE '%audio%' OR
          p.NameEnglish LIKE '%speaker%' OR
          p.NameEnglish LIKE '%headphone%'
      )
    
    PRINT 'Productos asignados a Electrónica: ' + CAST(@@ROWCOUNT AS VARCHAR)
END

-- Asignar productos de computadoras
IF @ComputadorasId IS NOT NULL
BEGIN
    INSERT INTO ProductCategories (ProductId, CategoryId)
    SELECT DISTINCT p.ProductId, @ComputadorasId
    FROM Products p
    LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId AND pc.CategoryId = @ComputadorasId
    WHERE pc.ProductId IS NULL
      AND (
          p.NameSpanish LIKE '%laptop%' OR
          p.NameSpanish LIKE '%notebook%' OR
          p.NameSpanish LIKE '%computadora%' OR
          p.NameSpanish LIKE '%PC%' OR
          p.NameSpanish LIKE '%procesador%' OR
          p.NameSpanish LIKE '%GPU%' OR
          p.NameSpanish LIKE '%teclado%' OR
          p.NameSpanish LIKE '%mouse%' OR
          p.NameEnglish LIKE '%laptop%' OR
          p.NameEnglish LIKE '%notebook%' OR
          p.NameEnglish LIKE '%computer%' OR
          p.NameEnglish LIKE '%PC%' OR
          p.NameEnglish LIKE '%processor%' OR
          p.NameEnglish LIKE '%GPU%' OR
          p.NameEnglish LIKE '%keyboard%' OR
          p.NameEnglish LIKE '%mouse%'
      )
    
    PRINT 'Productos asignados a Computadoras: ' + CAST(@@ROWCOUNT AS VARCHAR)
END

-- Asignar productos restantes a una categoría general si no tienen ninguna
DECLARE @GeneralId INT = (SELECT TOP 1 CategoryId FROM Categories WHERE IsActive = 1 ORDER BY DisplayOrder)

IF @GeneralId IS NOT NULL
BEGIN
    INSERT INTO ProductCategories (ProductId, CategoryId)
    SELECT DISTINCT p.ProductId, @GeneralId
    FROM Products p
    LEFT JOIN ProductCategories pc ON p.ProductId = pc.ProductId
    WHERE pc.ProductId IS NULL
    
    PRINT 'Productos sin categoría asignados a categoría general: ' + CAST(@@ROWCOUNT AS VARCHAR)
END

GO

-- 6. Verificar resultado final
PRINT ''
PRINT '=== RESULTADO FINAL ==='
SELECT 
    c.CategoryId,
    c.NameSpanish,
    c.Slug,
    COUNT(pc.ProductId) AS ProductCount
FROM Categories c
LEFT JOIN ProductCategories pc ON c.CategoryId = pc.CategoryId
GROUP BY c.CategoryId, c.NameSpanish, c.Slug
ORDER BY ProductCount DESC
GO

PRINT ''
PRINT '=== SCRIPT COMPLETADO ==='

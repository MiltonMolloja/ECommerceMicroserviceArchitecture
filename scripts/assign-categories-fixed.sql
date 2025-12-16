-- ============================================
-- Script: Verificar y Asignar Categorías a Productos
-- Base de datos: ECommerceDb
-- Schema: Catalog
-- ============================================

USE [ECommerceDb]
GO

PRINT '=== VERIFICANDO CATEGORÍAS EXISTENTES ==='
SELECT 
    CategoryId,
    NameSpanish,
    Slug,
    ParentCategoryId,
    COUNT_BIG(*) OVER() AS TotalCategorias
FROM Catalog.Categories
ORDER BY DisplayOrder
GO

PRINT ''
PRINT '=== PRODUCTOS POR CATEGORÍA ACTUAL ==='
SELECT 
    c.CategoryId,
    c.NameSpanish,
    c.Slug,
    COUNT(pc.ProductId) AS ProductCount
FROM Catalog.Categories c
LEFT JOIN Catalog.ProductCategories pc ON c.CategoryId = pc.CategoryId
GROUP BY c.CategoryId, c.NameSpanish, c.Slug
ORDER BY ProductCount DESC, c.NameSpanish
GO

PRINT ''
PRINT '=== CREANDO CATEGORÍAS BÁSICAS SI NO EXISTEN ==='

-- Verificar y crear categorías solo si la tabla está vacía
IF NOT EXISTS (SELECT 1 FROM Catalog.Categories)
BEGIN
    PRINT 'Tabla vacía. Creando categorías...'
    
    INSERT INTO Catalog.Categories (NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Slug, ParentCategoryId, IsActive, DisplayOrder)
    VALUES 
        (N'Electrónica', N'Electronics', N'Productos electrónicos y tecnología', N'Electronic products and technology', 'electronica', NULL, 1, 1),
        (N'Computadoras', N'Computers', N'Computadoras y accesorios', N'Computers and accessories', 'computadoras', NULL, 1, 2),
        (N'Hogar y Cocina', N'Home & Kitchen', N'Productos para el hogar', N'Home products', 'hogar', NULL, 1, 3),
        (N'General', N'General', N'Productos generales', N'General products', 'general', NULL, 1, 99)
    
    PRINT 'Categorías creadas exitosamente'
END
ELSE
BEGIN
    PRINT 'Las categorías ya existen. Continuando...'
END
GO

PRINT ''
PRINT '=== ASIGNANDO PRODUCTOS A CATEGORÍAS ==='

DECLARE @Assigned INT = 0

-- Obtener IDs de categorías
DECLARE @ElectronicaId INT = (SELECT TOP 1 CategoryId FROM Catalog.Categories WHERE Slug = 'electronica')
DECLARE @ComputadorasId INT = (SELECT TOP 1 CategoryId FROM Catalog.Categories WHERE Slug = 'computadoras')
DECLARE @GeneralId INT = (SELECT TOP 1 CategoryId FROM Catalog.Categories WHERE Slug = 'general' OR DisplayOrder = 99)

-- Si no existe categoría General, usar la primera disponible
IF @GeneralId IS NULL
    SET @GeneralId = (SELECT TOP 1 CategoryId FROM Catalog.Categories WHERE IsActive = 1 ORDER BY DisplayOrder)

PRINT 'Categoría Electrónica ID: ' + ISNULL(CAST(@ElectronicaId AS VARCHAR), 'NULL')
PRINT 'Categoría Computadoras ID: ' + ISNULL(CAST(@ComputadorasId AS VARCHAR), 'NULL')
PRINT 'Categoría General ID: ' + ISNULL(CAST(@GeneralId AS VARCHAR), 'NULL')
PRINT ''

-- Asignar productos de electrónica (TVs, monitores, audio)
IF @ElectronicaId IS NOT NULL
BEGIN
    INSERT INTO Catalog.ProductCategories (ProductId, CategoryId)
    SELECT DISTINCT p.ProductId, @ElectronicaId
    FROM Catalog.Products p
    LEFT JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId AND pc.CategoryId = @ElectronicaId
    WHERE pc.ProductId IS NULL
      AND (
          p.NameSpanish LIKE '%TV%' OR
          p.NameSpanish LIKE '%televisor%' OR
          p.NameSpanish LIKE '%televisión%' OR
          p.NameSpanish LIKE '%monitor%' OR
          p.NameSpanish LIKE '%audio%' OR
          p.NameSpanish LIKE '%parlante%' OR
          p.NameEnglish LIKE '%TV%' OR
          p.NameEnglish LIKE '%television%' OR
          p.NameEnglish LIKE '%monitor%' OR
          p.NameEnglish LIKE '%audio%'
      )
    
    SET @Assigned = @@ROWCOUNT
    PRINT 'Productos asignados a Electrónica: ' + CAST(@Assigned AS VARCHAR)
END

-- Asignar productos de computadoras
IF @ComputadorasId IS NOT NULL
BEGIN
    INSERT INTO Catalog.ProductCategories (ProductId, CategoryId)
    SELECT DISTINCT p.ProductId, @ComputadorasId
    FROM Catalog.Products p
    LEFT JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId AND pc.CategoryId = @ComputadorasId
    WHERE pc.ProductId IS NULL
      AND (
          p.NameSpanish LIKE '%laptop%' OR
          p.NameSpanish LIKE '%notebook%' OR
          p.NameSpanish LIKE '%computadora%' OR
          p.NameSpanish LIKE '%PC%' OR
          p.NameSpanish LIKE '%procesador%' OR
          p.NameSpanish LIKE '%CPU%' OR
          p.NameSpanish LIKE '%GPU%' OR
          p.NameSpanish LIKE '%Intel%' OR
          p.NameSpanish LIKE '%AMD%' OR
          p.NameSpanish LIKE '%Ryzen%' OR
          p.NameSpanish LIKE '%teclado%' OR
          p.NameSpanish LIKE '%mouse%' OR
          p.NameEnglish LIKE '%laptop%' OR
          p.NameEnglish LIKE '%computer%' OR
          p.NameEnglish LIKE '%processor%' OR
          p.NameEnglish LIKE '%Intel%' OR
          p.NameEnglish LIKE '%AMD%'
      )
    
    SET @Assigned = @@ROWCOUNT
    PRINT 'Productos asignados a Computadoras: ' + CAST(@Assigned AS VARCHAR)
END

-- Asignar productos sin categoría a categoría general
IF @GeneralId IS NOT NULL
BEGIN
    INSERT INTO Catalog.ProductCategories (ProductId, CategoryId)
    SELECT DISTINCT p.ProductId, @GeneralId
    FROM Catalog.Products p
    WHERE NOT EXISTS (
        SELECT 1 FROM Catalog.ProductCategories pc WHERE pc.ProductId = p.ProductId
    )
    
    SET @Assigned = @@ROWCOUNT
    PRINT 'Productos sin categoría asignados a General: ' + CAST(@Assigned AS VARCHAR)
END

GO

PRINT ''
PRINT '=== RESULTADO FINAL ==='
SELECT 
    c.CategoryId,
    c.NameSpanish,
    c.Slug,
    c.IsActive,
    COUNT(pc.ProductId) AS ProductCount
FROM Catalog.Categories c
LEFT JOIN Catalog.ProductCategories pc ON c.CategoryId = pc.CategoryId
GROUP BY c.CategoryId, c.NameSpanish, c.Slug, c.IsActive
ORDER BY ProductCount DESC, c.NameSpanish
GO

PRINT ''
PRINT '============================================'
PRINT 'SCRIPT COMPLETADO EXITOSAMENTE'
PRINT '============================================'

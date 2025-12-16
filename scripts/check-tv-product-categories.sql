-- Check if products matching "tv" have categories assigned
USE ECommerceDb;
GO

-- 1. Find products matching "tv"
SELECT 
    p.ProductId,
    p.NameSpanish,
    p.NameEnglish,
    p.BrandId,
    b.Name AS BrandName,
    COUNT(pc.CategoryId) AS CategoryCount
FROM Catalog.Products p
LEFT JOIN Catalog.Brands b ON p.BrandId = b.BrandId
LEFT JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId
WHERE 
    p.NameSpanish LIKE '%tv%' OR 
    p.NameEnglish LIKE '%tv%' OR
    p.DescriptionSpanish LIKE '%tv%' OR
    p.DescriptionEnglish LIKE '%tv%' OR
    p.SKU LIKE '%tv%'
GROUP BY p.ProductId, p.NameSpanish, p.NameEnglish, p.BrandId, b.Name
ORDER BY CategoryCount DESC, p.ProductId;

-- 2. Show categories for products matching "tv"
SELECT 
    p.ProductId,
    p.NameSpanish AS ProductName,
    c.CategoryId,
    c.NameSpanish AS CategoryName,
    c.NameEnglish AS CategoryNameEN
FROM Catalog.Products p
INNER JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId
INNER JOIN Catalog.Categories c ON pc.CategoryId = c.CategoryId
WHERE 
    p.NameSpanish LIKE '%tv%' OR 
    p.NameEnglish LIKE '%tv%' OR
    p.DescriptionSpanish LIKE '%tv%' OR
    p.DescriptionEnglish LIKE '%tv%'
ORDER BY p.ProductId, c.CategoryId;

-- 3. Count products with and without categories
SELECT 
    'Products matching TV' AS Description,
    COUNT(*) AS Total,
    SUM(CASE WHEN pc.CategoryId IS NOT NULL THEN 1 ELSE 0 END) AS WithCategories,
    SUM(CASE WHEN pc.CategoryId IS NULL THEN 1 ELSE 0 END) AS WithoutCategories
FROM Catalog.Products p
LEFT JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId
WHERE 
    p.NameSpanish LIKE '%tv%' OR 
    p.NameEnglish LIKE '%tv%' OR
    p.DescriptionSpanish LIKE '%tv%' OR
    p.DescriptionEnglish LIKE '%tv%';

-- 4. Show all categories with product count
SELECT 
    c.CategoryId,
    c.NameSpanish,
    c.NameEnglish,
    COUNT(DISTINCT pc.ProductId) AS ProductCount,
    COUNT(DISTINCT CASE 
        WHEN p.NameSpanish LIKE '%tv%' OR 
             p.NameEnglish LIKE '%tv%' OR
             p.DescriptionSpanish LIKE '%tv%' OR
             p.DescriptionEnglish LIKE '%tv%'
        THEN pc.ProductId 
    END) AS TVProductCount
FROM Catalog.Categories c
LEFT JOIN Catalog.ProductCategories pc ON c.CategoryId = pc.CategoryId
LEFT JOIN Catalog.Products p ON pc.ProductId = p.ProductId
GROUP BY c.CategoryId, c.NameSpanish, c.NameEnglish
ORDER BY TVProductCount DESC, ProductCount DESC;

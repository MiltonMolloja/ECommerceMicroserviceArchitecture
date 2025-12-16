-- =============================================
-- Script: Add IsFeatured and ImageUrl to Categories
-- Description: Adds fields needed for Home page featured categories
-- Author: System
-- Date: 2025-12-10
-- =============================================

USE ECommerceDb;
GO

-- Check if columns already exist
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Catalog.Categories') AND name = 'IsFeatured')
BEGIN
    PRINT 'Adding IsFeatured column to Categories table...';
    
    ALTER TABLE Catalog.Categories
    ADD IsFeatured BIT NOT NULL DEFAULT 0;
    
    PRINT '✓ IsFeatured column added successfully';
END
ELSE
BEGIN
    PRINT '⚠ IsFeatured column already exists';
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('Catalog.Categories') AND name = 'ImageUrl')
BEGIN
    PRINT 'Adding ImageUrl column to Categories table...';
    
    ALTER TABLE Catalog.Categories
    ADD ImageUrl NVARCHAR(500) NULL;
    
    PRINT '✓ ImageUrl column added successfully';
END
ELSE
BEGIN
    PRINT '⚠ ImageUrl column already exists';
END
GO

-- Create index on IsFeatured for better query performance
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE object_id = OBJECT_ID('Catalog.Categories') AND name = 'IX_Categories_IsFeatured')
BEGIN
    PRINT 'Creating index on IsFeatured...';
    
    CREATE NONCLUSTERED INDEX IX_Categories_IsFeatured
    ON Catalog.Categories(IsFeatured)
    INCLUDE (CategoryId, NameSpanish, NameEnglish, Slug, ImageUrl, DisplayOrder)
    WHERE IsFeatured = 1 AND IsActive = 1;
    
    PRINT '✓ Index IX_Categories_IsFeatured created successfully';
END
ELSE
BEGIN
    PRINT '⚠ Index IX_Categories_IsFeatured already exists';
END
GO

-- Mark some categories as featured (example data)
PRINT 'Marking sample categories as featured...';

UPDATE Catalog.Categories
SET IsFeatured = 1,
    ImageUrl = CASE 
        WHEN NameEnglish LIKE '%Computer%' OR NameEnglish LIKE '%Laptop%' THEN 'https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=400'
        WHEN NameEnglish LIKE '%Phone%' OR NameEnglish LIKE '%Mobile%' THEN 'https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=400'
        WHEN NameEnglish LIKE '%TV%' OR NameEnglish LIKE '%Television%' THEN 'https://images.unsplash.com/photo-1593359677879-a4bb92f829d1?w=400'
        WHEN NameEnglish LIKE '%Audio%' OR NameEnglish LIKE '%Headphone%' THEN 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400'
        WHEN NameEnglish LIKE '%Camera%' THEN 'https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=400'
        WHEN NameEnglish LIKE '%Gaming%' THEN 'https://images.unsplash.com/photo-1538481199705-c710c4e965fc?w=400'
        WHEN NameEnglish LIKE '%Watch%' OR NameEnglish LIKE '%Wearable%' THEN 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=400'
        WHEN NameEnglish LIKE '%Tablet%' THEN 'https://images.unsplash.com/photo-1561154464-82e9adf32764?w=400'
        ELSE NULL
    END
WHERE ParentCategoryId IS NULL  -- Only top-level categories
  AND IsActive = 1;

DECLARE @FeaturedCount INT = @@ROWCOUNT;
PRINT '✓ Marked ' + CAST(@FeaturedCount AS NVARCHAR(10)) + ' categories as featured';
GO

-- Verification query
PRINT '';
PRINT '=== Featured Categories ===';
SELECT 
    CategoryId,
    NameEnglish,
    NameSpanish,
    Slug,
    IsFeatured,
    ImageUrl,
    DisplayOrder
FROM Catalog.Categories
WHERE IsFeatured = 1 AND IsActive = 1
ORDER BY DisplayOrder, NameEnglish;
GO

PRINT '';
PRINT '✓ Migration completed successfully!';
GO

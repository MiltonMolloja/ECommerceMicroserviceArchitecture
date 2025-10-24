-- =============================================
-- Migration Script: Expand Catalog Model
-- Description: Migrates from simple Product schema to expanded multilingual model
-- Date: 2025-01-24
-- =============================================

BEGIN TRANSACTION;

-- Step 1: Delete existing seed data (if any)
DELETE FROM [Catalog].[Stocks];
DELETE FROM [Catalog].[Products];

-- Step 2: Rename Stocks table to ProductInStock
EXEC sp_rename '[Catalog].[Stocks]', 'ProductInStock';
EXEC sp_rename '[Catalog].[IX_Stocks_ProductId]', 'IX_ProductInStock_ProductId', 'INDEX';

-- Step 3: Add new columns to Products table
ALTER TABLE [Catalog].[Products]
ADD
    [NameSpanish] NVARCHAR(200) NULL,
    [NameEnglish] NVARCHAR(200) NULL,
    [DescriptionSpanish] NVARCHAR(1000) NULL,
    [DescriptionEnglish] NVARCHAR(1000) NULL,
    [SKU] NVARCHAR(50) NULL,
    [Brand] NVARCHAR(100) NULL,
    [Slug] NVARCHAR(200) NULL,
    [OriginalPrice] DECIMAL(18,2) NULL,
    [DiscountPercentage] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [TaxRate] DECIMAL(5,2) NOT NULL DEFAULT 0,
    [Images] NVARCHAR(4000) NULL,
    [MetaTitle] NVARCHAR(100) NULL,
    [MetaDescription] NVARCHAR(300) NULL,
    [MetaKeywords] NVARCHAR(500) NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [IsFeatured] BIT NOT NULL DEFAULT 0,
    [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
    [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE();

-- Step 4: Migrate existing data (if any exists)
UPDATE [Catalog].[Products]
SET
    [NameSpanish] = ISNULL([Name], 'Product'),
    [NameEnglish] = ISNULL([Name], 'Product'),
    [DescriptionSpanish] = ISNULL([Description], 'Description'),
    [DescriptionEnglish] = ISNULL([Description], 'Description'),
    [SKU] = 'SKU-' + CAST([ProductId] AS NVARCHAR(50)),
    [Slug] = LOWER(REPLACE(ISNULL([Name], 'product-' + CAST([ProductId] AS NVARCHAR(50))), ' ', '-'))
WHERE [Name] IS NOT NULL OR [Description] IS NOT NULL;

-- Step 5: Make new columns NOT NULL after data migration
ALTER TABLE [Catalog].[Products] ALTER COLUMN [NameSpanish] NVARCHAR(200) NOT NULL;
ALTER TABLE [Catalog].[Products] ALTER COLUMN [NameEnglish] NVARCHAR(200) NOT NULL;
ALTER TABLE [Catalog].[Products] ALTER COLUMN [DescriptionSpanish] NVARCHAR(1000) NOT NULL;
ALTER TABLE [Catalog].[Products] ALTER COLUMN [DescriptionEnglish] NVARCHAR(1000) NOT NULL;
ALTER TABLE [Catalog].[Products] ALTER COLUMN [SKU] NVARCHAR(50) NOT NULL;
ALTER TABLE [Catalog].[Products] ALTER COLUMN [Slug] NVARCHAR(200) NOT NULL;

-- Step 6: Drop old columns
ALTER TABLE [Catalog].[Products] DROP COLUMN [Name];
ALTER TABLE [Catalog].[Products] DROP COLUMN [Description];

-- Step 7: Add unique indexes
CREATE UNIQUE NONCLUSTERED INDEX [IX_Products_SKU] ON [Catalog].[Products]([SKU]);
CREATE UNIQUE NONCLUSTERED INDEX [IX_Products_Slug] ON [Catalog].[Products]([Slug]);

-- Step 8: Add additional indexes for performance
CREATE NONCLUSTERED INDEX [IX_Products_Brand] ON [Catalog].[Products]([Brand]);
CREATE NONCLUSTERED INDEX [IX_Products_IsActive] ON [Catalog].[Products]([IsActive]);
CREATE NONCLUSTERED INDEX [IX_Products_IsFeatured] ON [Catalog].[Products]([IsFeatured]);
CREATE NONCLUSTERED INDEX [IX_Products_IsActive_IsFeatured] ON [Catalog].[Products]([IsActive], [IsFeatured]);

-- Step 9: Add columns to ProductInStock table
ALTER TABLE [Catalog].[ProductInStock]
ADD
    [MinStock] INT NOT NULL DEFAULT 0,
    [MaxStock] INT NOT NULL DEFAULT 1000;

-- Step 10: Add check constraints to ProductInStock
ALTER TABLE [Catalog].[ProductInStock]
ADD CONSTRAINT [CK_Stock_Positive] CHECK ([Stock] >= 0),
    CONSTRAINT [CK_MinStock_Positive] CHECK ([MinStock] >= 0),
    CONSTRAINT [CK_MaxStock_Valid] CHECK ([MaxStock] > [MinStock]);

-- Step 11: Create Categories table
CREATE TABLE [Catalog].[Categories] (
    [CategoryId] INT IDENTITY(1,1) NOT NULL,
    [NameSpanish] NVARCHAR(100) NOT NULL,
    [NameEnglish] NVARCHAR(100) NOT NULL,
    [DescriptionSpanish] NVARCHAR(500) NULL,
    [DescriptionEnglish] NVARCHAR(500) NULL,
    [Slug] NVARCHAR(200) NOT NULL,
    [ParentCategoryId] INT NULL,
    [IsActive] BIT NOT NULL DEFAULT 1,
    [DisplayOrder] INT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([CategoryId] ASC),
    CONSTRAINT [FK_Categories_ParentCategory] FOREIGN KEY ([ParentCategoryId])
        REFERENCES [Catalog].[Categories]([CategoryId])
);

CREATE UNIQUE NONCLUSTERED INDEX [IX_Categories_Slug] ON [Catalog].[Categories]([Slug]);
CREATE NONCLUSTERED INDEX [IX_Categories_ParentCategoryId] ON [Catalog].[Categories]([ParentCategoryId]);
CREATE NONCLUSTERED INDEX [IX_Categories_IsActive] ON [Catalog].[Categories]([IsActive]);
CREATE NONCLUSTERED INDEX [IX_Categories_DisplayOrder] ON [Catalog].[Categories]([DisplayOrder]);

-- Step 12: Create ProductCategories table (N:M)
CREATE TABLE [Catalog].[ProductCategories] (
    [ProductId] INT NOT NULL,
    [CategoryId] INT NOT NULL,
    [IsPrimary] BIT NOT NULL DEFAULT 0,
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY CLUSTERED ([ProductId], [CategoryId]),
    CONSTRAINT [FK_ProductCategories_Product] FOREIGN KEY ([ProductId])
        REFERENCES [Catalog].[Products]([ProductId])
        ON DELETE CASCADE,
    CONSTRAINT [FK_ProductCategories_Category] FOREIGN KEY ([CategoryId])
        REFERENCES [Catalog].[Categories]([CategoryId])
        ON DELETE CASCADE
);

CREATE NONCLUSTERED INDEX [IX_ProductCategories_CategoryId] ON [Catalog].[ProductCategories]([CategoryId]);
CREATE NONCLUSTERED INDEX [IX_ProductCategories_IsPrimary] ON [Catalog].[ProductCategories]([IsPrimary]);

-- Step 13: Insert migration record
IF NOT EXISTS (SELECT 1 FROM [__EFMigrationsHistory] WHERE [MigrationId] = '20251024000000_ExpandCatalogModel')
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES ('20251024000000_ExpandCatalogModel', '9.0.0');
END

COMMIT TRANSACTION;

PRINT 'Migration completed successfully!';

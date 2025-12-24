IF SCHEMA_ID(N'Catalog') IS NULL EXEC(N'CREATE SCHEMA [Catalog];');
GO


CREATE TABLE [Catalog].[Banners] (
    [BannerId] int NOT NULL IDENTITY,
    [TitleSpanish] nvarchar(200) NOT NULL,
    [TitleEnglish] nvarchar(200) NOT NULL,
    [SubtitleSpanish] nvarchar(500) NULL,
    [SubtitleEnglish] nvarchar(500) NULL,
    [ImageUrl] nvarchar(500) NOT NULL,
    [ImageUrlMobile] nvarchar(500) NULL,
    [LinkUrl] nvarchar(500) NULL,
    [ButtonTextSpanish] nvarchar(100) NULL,
    [ButtonTextEnglish] nvarchar(100) NULL,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [Position] nvarchar(50) NOT NULL DEFAULT N'hero',
    [StartDate] datetime2 NULL,
    [EndDate] datetime2 NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Banners] PRIMARY KEY ([BannerId])
);
GO


CREATE TABLE [Catalog].[Brands] (
    [BrandId] int NOT NULL IDENTITY,
    [Name] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [LogoUrl] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_Brands] PRIMARY KEY ([BrandId])
);
GO


CREATE TABLE [Catalog].[Categories] (
    [CategoryId] int NOT NULL IDENTITY,
    [NameSpanish] nvarchar(100) NOT NULL,
    [NameEnglish] nvarchar(100) NOT NULL,
    [DescriptionSpanish] nvarchar(500) NULL,
    [DescriptionEnglish] nvarchar(500) NULL,
    [Slug] nvarchar(200) NOT NULL,
    [ParentCategoryId] int NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ImageUrl] nvarchar(500) NULL,
    CONSTRAINT [PK_Categories] PRIMARY KEY ([CategoryId]),
    CONSTRAINT [FK_Categories_Categories_ParentCategoryId] FOREIGN KEY ([ParentCategoryId]) REFERENCES [Catalog].[Categories] ([CategoryId]) ON DELETE NO ACTION
);
GO


CREATE TABLE [Catalog].[Products] (
    [ProductId] int NOT NULL IDENTITY,
    [NameSpanish] nvarchar(200) NOT NULL,
    [NameEnglish] nvarchar(200) NOT NULL,
    [DescriptionSpanish] nvarchar(1000) NOT NULL,
    [DescriptionEnglish] nvarchar(1000) NOT NULL,
    [SKU] nvarchar(50) NOT NULL,
    [Brand] nvarchar(100) NULL,
    [BrandId] int NULL,
    [Slug] nvarchar(200) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [OriginalPrice] decimal(18,2) NULL,
    [DiscountPercentage] decimal(5,2) NOT NULL DEFAULT 0.0,
    [TaxRate] decimal(5,2) NOT NULL DEFAULT 0.0,
    [Images] nvarchar(4000) NULL,
    [MetaTitle] nvarchar(100) NULL,
    [MetaDescription] nvarchar(300) NULL,
    [MetaKeywords] nvarchar(500) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsFeatured] bit NOT NULL DEFAULT CAST(0 AS bit),
    [TotalSold] int NOT NULL DEFAULT 0,
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETUTCDATE()),
    CONSTRAINT [PK_Products] PRIMARY KEY ([ProductId]),
    CONSTRAINT [FK_Products_Brands_BrandId] FOREIGN KEY ([BrandId]) REFERENCES [Catalog].[Brands] ([BrandId]) ON DELETE SET NULL
);
GO


CREATE TABLE [Catalog].[ProductAttributes] (
    [AttributeId] int NOT NULL IDENTITY,
    [AttributeName] nvarchar(100) NOT NULL,
    [AttributeNameEnglish] nvarchar(100) NULL,
    [AttributeType] nvarchar(20) NOT NULL,
    [Unit] nvarchar(20) NULL,
    [IsFilterable] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsSearchable] bit NOT NULL DEFAULT CAST(0 AS bit),
    [DisplayOrder] int NOT NULL DEFAULT 0,
    [CategoryId] int NULL,
    CONSTRAINT [PK_ProductAttributes] PRIMARY KEY ([AttributeId]),
    CONSTRAINT [FK_ProductAttributes_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Catalog].[Categories] ([CategoryId]) ON DELETE SET NULL
);
GO


CREATE TABLE [Catalog].[ProductCategories] (
    [ProductId] int NOT NULL,
    [CategoryId] int NOT NULL,
    [IsPrimary] bit NOT NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([ProductId], [CategoryId]),
    CONSTRAINT [FK_ProductCategories_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [Catalog].[Categories] ([CategoryId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductCategories_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Catalog].[Products] ([ProductId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Catalog].[ProductInStock] (
    [ProductInStockId] int NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [Stock] int NOT NULL DEFAULT 0,
    [MinStock] int NOT NULL DEFAULT 0,
    [MaxStock] int NOT NULL DEFAULT 1000,
    CONSTRAINT [PK_ProductInStock] PRIMARY KEY ([ProductInStockId]),
    CONSTRAINT [CK_MaxStock_Valid] CHECK ([MaxStock] > [MinStock]),
    CONSTRAINT [CK_MinStock_Positive] CHECK ([MinStock] >= 0),
    CONSTRAINT [CK_Stock_Positive] CHECK ([Stock] >= 0),
    CONSTRAINT [FK_ProductInStock_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Catalog].[Products] ([ProductId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Catalog].[ProductRatings] (
    [ProductId] int NOT NULL,
    [AverageRating] decimal(3,2) NOT NULL DEFAULT 0.0,
    [TotalReviews] int NOT NULL DEFAULT 0,
    [Rating5Star] int NOT NULL DEFAULT 0,
    [Rating4Star] int NOT NULL DEFAULT 0,
    [Rating3Star] int NOT NULL DEFAULT 0,
    [Rating2Star] int NOT NULL DEFAULT 0,
    [Rating1Star] int NOT NULL DEFAULT 0,
    [LastUpdated] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_ProductRatings] PRIMARY KEY ([ProductId]),
    CONSTRAINT [FK_ProductRatings_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Catalog].[Products] ([ProductId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Catalog].[ProductReviews] (
    [ReviewId] bigint NOT NULL IDENTITY,
    [ProductId] int NOT NULL,
    [UserId] int NOT NULL,
    [Rating] decimal(2,1) NOT NULL,
    [Title] nvarchar(200) NULL,
    [Comment] nvarchar(max) NULL,
    [IsVerifiedPurchase] bit NOT NULL DEFAULT CAST(0 AS bit),
    [HelpfulCount] int NOT NULL DEFAULT 0,
    [NotHelpfulCount] int NOT NULL DEFAULT 0,
    [IsApproved] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    [UpdatedAt] datetime2 NOT NULL DEFAULT (GETDATE()),
    CONSTRAINT [PK_ProductReviews] PRIMARY KEY ([ReviewId]),
    CONSTRAINT [CK_ProductReviews_Rating] CHECK ([Rating] >= 1.0 AND [Rating] <= 5.0),
    CONSTRAINT [FK_ProductReviews_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Catalog].[Products] ([ProductId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Catalog].[AttributeValues] (
    [ValueId] int NOT NULL IDENTITY,
    [AttributeId] int NOT NULL,
    [ValueText] nvarchar(200) NOT NULL,
    [ValueTextEnglish] nvarchar(200) NULL,
    [DisplayOrder] int NOT NULL DEFAULT 0,
    CONSTRAINT [PK_AttributeValues] PRIMARY KEY ([ValueId]),
    CONSTRAINT [FK_AttributeValues_ProductAttributes_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [Catalog].[ProductAttributes] ([AttributeId]) ON DELETE CASCADE
);
GO


CREATE TABLE [Catalog].[ProductAttributeValues] (
    [ProductId] int NOT NULL,
    [AttributeId] int NOT NULL,
    [ValueId] int NOT NULL,
    [TextValue] nvarchar(500) NULL,
    [NumericValue] decimal(18,4) NULL,
    [BooleanValue] bit NULL,
    CONSTRAINT [PK_ProductAttributeValues] PRIMARY KEY ([ProductId], [AttributeId], [ValueId]),
    CONSTRAINT [FK_ProductAttributeValues_AttributeValues_ValueId] FOREIGN KEY ([ValueId]) REFERENCES [Catalog].[AttributeValues] ([ValueId]) ON DELETE NO ACTION,
    CONSTRAINT [FK_ProductAttributeValues_ProductAttributes_AttributeId] FOREIGN KEY ([AttributeId]) REFERENCES [Catalog].[ProductAttributes] ([AttributeId]) ON DELETE CASCADE,
    CONSTRAINT [FK_ProductAttributeValues_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Catalog].[Products] ([ProductId]) ON DELETE CASCADE
);
GO


CREATE INDEX [IX_AttributeValues_AttributeId] ON [Catalog].[AttributeValues] ([AttributeId]);
GO


CREATE INDEX [IX_AttributeValues_AttributeId_DisplayOrder] ON [Catalog].[AttributeValues] ([AttributeId], [DisplayOrder]);
GO


CREATE INDEX [IX_Banners_Dates] ON [Catalog].[Banners] ([StartDate], [EndDate]);
GO


CREATE INDEX [IX_Banners_DisplayOrder] ON [Catalog].[Banners] ([DisplayOrder]);
GO


CREATE INDEX [IX_Banners_Position_Active] ON [Catalog].[Banners] ([Position], [IsActive]);
GO


CREATE INDEX [IX_Brands_IsActive] ON [Catalog].[Brands] ([IsActive]);
GO


CREATE UNIQUE INDEX [IX_Brands_Name] ON [Catalog].[Brands] ([Name]);
GO


CREATE INDEX [IX_Categories_DisplayOrder] ON [Catalog].[Categories] ([DisplayOrder]);
GO


CREATE INDEX [IX_Categories_IsActive] ON [Catalog].[Categories] ([IsActive]);
GO


CREATE INDEX [IX_Categories_IsFeatured] ON [Catalog].[Categories] ([IsFeatured]);
GO


CREATE INDEX [IX_Categories_ParentCategoryId] ON [Catalog].[Categories] ([ParentCategoryId]);
GO


CREATE UNIQUE INDEX [IX_Categories_Slug] ON [Catalog].[Categories] ([Slug]);
GO


CREATE INDEX [IX_ProductAttributes_AttributeName] ON [Catalog].[ProductAttributes] ([AttributeName]);
GO


CREATE INDEX [IX_ProductAttributes_CategoryId_IsFilterable] ON [Catalog].[ProductAttributes] ([CategoryId], [IsFilterable]);
GO


CREATE INDEX [IX_ProductAttributeValues_Attribute_Numeric] ON [Catalog].[ProductAttributeValues] ([AttributeId], [NumericValue]) WHERE [NumericValue] IS NOT NULL;
GO


CREATE INDEX [IX_ProductAttributeValues_Attribute_Value] ON [Catalog].[ProductAttributeValues] ([AttributeId], [ValueId]);
GO


CREATE INDEX [IX_ProductAttributeValues_ValueId] ON [Catalog].[ProductAttributeValues] ([ValueId]);
GO


CREATE INDEX [IX_ProductCategories_CategoryId] ON [Catalog].[ProductCategories] ([CategoryId]);
GO


CREATE INDEX [IX_ProductCategories_IsPrimary] ON [Catalog].[ProductCategories] ([IsPrimary]);
GO


CREATE UNIQUE INDEX [IX_ProductInStock_ProductId] ON [Catalog].[ProductInStock] ([ProductId]);
GO


CREATE INDEX [IX_ProductRatings_Rating] ON [Catalog].[ProductRatings] ([AverageRating] DESC);
GO


CREATE INDEX [IX_ProductReviews_CreatedAt] ON [Catalog].[ProductReviews] ([CreatedAt]);
GO


CREATE INDEX [IX_ProductReviews_Product_Rating] ON [Catalog].[ProductReviews] ([ProductId], [Rating], [IsApproved]);
GO


CREATE INDEX [IX_ProductReviews_User] ON [Catalog].[ProductReviews] ([UserId]);
GO


CREATE INDEX [IX_Products_Brand] ON [Catalog].[Products] ([Brand]);
GO


CREATE INDEX [IX_Products_BrandId] ON [Catalog].[Products] ([BrandId]);
GO


CREATE INDEX [IX_Products_IsActive] ON [Catalog].[Products] ([IsActive]);
GO


CREATE INDEX [IX_Products_IsActive_IsFeatured] ON [Catalog].[Products] ([IsActive], [IsFeatured]);
GO


CREATE INDEX [IX_Products_IsFeatured] ON [Catalog].[Products] ([IsFeatured]);
GO


CREATE UNIQUE INDEX [IX_Products_SKU] ON [Catalog].[Products] ([SKU]);
GO


CREATE UNIQUE INDEX [IX_Products_Slug] ON [Catalog].[Products] ([Slug]);
GO


CREATE INDEX [IX_Products_TotalSold] ON [Catalog].[Products] ([TotalSold]);
GO



-- Catalog Schema Tables for PostgreSQL
-- Generated from EF Core model

CREATE SCHEMA IF NOT EXISTS "Catalog";

CREATE TABLE "Catalog"."Brands" (
    "BrandId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000) NULL,
    "LogoUrl" VARCHAR(500) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX "IX_Brands_Name" ON "Catalog"."Brands" ("Name");
CREATE INDEX "IX_Brands_IsActive" ON "Catalog"."Brands" ("IsActive");

CREATE TABLE "Catalog"."Categories" (
    "CategoryId" SERIAL PRIMARY KEY,
    "NameSpanish" VARCHAR(100) NOT NULL,
    "NameEnglish" VARCHAR(100) NOT NULL,
    "DescriptionSpanish" VARCHAR(500) NULL,
    "DescriptionEnglish" VARCHAR(500) NULL,
    "Slug" VARCHAR(200) NOT NULL,
    "ParentCategoryId" INT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "DisplayOrder" INT NOT NULL DEFAULT 0,
    "IsFeatured" BOOLEAN NOT NULL DEFAULT FALSE,
    "ImageUrl" VARCHAR(500) NULL,
    CONSTRAINT "FK_Categories_Parent" FOREIGN KEY ("ParentCategoryId") REFERENCES "Catalog"."Categories" ("CategoryId") ON DELETE SET NULL
);

CREATE UNIQUE INDEX "IX_Categories_Slug" ON "Catalog"."Categories" ("Slug");
CREATE INDEX "IX_Categories_ParentCategoryId" ON "Catalog"."Categories" ("ParentCategoryId");
CREATE INDEX "IX_Categories_IsActive" ON "Catalog"."Categories" ("IsActive");
CREATE INDEX "IX_Categories_IsFeatured" ON "Catalog"."Categories" ("IsFeatured");
CREATE INDEX "IX_Categories_DisplayOrder" ON "Catalog"."Categories" ("DisplayOrder");

CREATE TABLE "Catalog"."Products" (
    "ProductId" SERIAL PRIMARY KEY,
    "NameSpanish" VARCHAR(200) NOT NULL,
    "NameEnglish" VARCHAR(200) NOT NULL,
    "DescriptionSpanish" VARCHAR(1000) NOT NULL,
    "DescriptionEnglish" VARCHAR(1000) NOT NULL,
    "SKU" VARCHAR(50) NOT NULL,
    "Brand" VARCHAR(100) NULL,
    "BrandId" INT NULL,
    "Slug" VARCHAR(200) NOT NULL,
    "Price" DECIMAL(18,2) NOT NULL,
    "OriginalPrice" DECIMAL(18,2) NULL,
    "DiscountPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0.0,
    "TaxRate" DECIMAL(5,2) NOT NULL DEFAULT 0.0,
    "Images" VARCHAR(4000) NULL,
    "MetaTitle" VARCHAR(100) NULL,
    "MetaDescription" VARCHAR(300) NULL,
    "MetaKeywords" VARCHAR(500) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsFeatured" BOOLEAN NOT NULL DEFAULT FALSE,
    "TotalSold" INT NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_Products_Brands" FOREIGN KEY ("BrandId") REFERENCES "Catalog"."Brands" ("BrandId") ON DELETE SET NULL
);

CREATE UNIQUE INDEX "IX_Products_SKU" ON "Catalog"."Products" ("SKU");
CREATE UNIQUE INDEX "IX_Products_Slug" ON "Catalog"."Products" ("Slug");
CREATE INDEX "IX_Products_Brand" ON "Catalog"."Products" ("Brand");
CREATE INDEX "IX_Products_BrandId" ON "Catalog"."Products" ("BrandId");
CREATE INDEX "IX_Products_IsActive" ON "Catalog"."Products" ("IsActive");
CREATE INDEX "IX_Products_IsFeatured" ON "Catalog"."Products" ("IsFeatured");
CREATE INDEX "IX_Products_IsActive_IsFeatured" ON "Catalog"."Products" ("IsActive", "IsFeatured");
CREATE INDEX "IX_Products_TotalSold" ON "Catalog"."Products" ("TotalSold");

CREATE TABLE "Catalog"."Banners" (
    "BannerId" SERIAL PRIMARY KEY,
    "TitleSpanish" VARCHAR(200) NOT NULL,
    "TitleEnglish" VARCHAR(200) NOT NULL,
    "SubtitleSpanish" VARCHAR(500) NULL,
    "SubtitleEnglish" VARCHAR(500) NULL,
    "ImageUrl" VARCHAR(500) NOT NULL,
    "ImageUrlMobile" VARCHAR(500) NULL,
    "LinkUrl" VARCHAR(500) NULL,
    "ButtonTextSpanish" VARCHAR(100) NULL,
    "ButtonTextEnglish" VARCHAR(100) NULL,
    "DisplayOrder" INT NOT NULL DEFAULT 0,
    "Position" VARCHAR(50) NOT NULL DEFAULT 'hero',
    "StartDate" TIMESTAMP NULL,
    "EndDate" TIMESTAMP NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX "IX_Banners_DisplayOrder" ON "Catalog"."Banners" ("DisplayOrder");
CREATE INDEX "IX_Banners_Position_Active" ON "Catalog"."Banners" ("Position", "IsActive");
CREATE INDEX "IX_Banners_Dates" ON "Catalog"."Banners" ("StartDate", "EndDate");

CREATE TABLE "Catalog"."ProductCategories" (
    "ProductId" INT NOT NULL,
    "CategoryId" INT NOT NULL,
    "IsPrimary" BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY ("ProductId", "CategoryId"),
    CONSTRAINT "FK_ProductCategories_Products" FOREIGN KEY ("ProductId") REFERENCES "Catalog"."Products" ("ProductId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductCategories_Categories" FOREIGN KEY ("CategoryId") REFERENCES "Catalog"."Categories" ("CategoryId") ON DELETE CASCADE
);

CREATE INDEX "IX_ProductCategories_CategoryId" ON "Catalog"."ProductCategories" ("CategoryId");
CREATE INDEX "IX_ProductCategories_IsPrimary" ON "Catalog"."ProductCategories" ("IsPrimary");

CREATE TABLE "Catalog"."ProductInStock" (
    "ProductInStockId" SERIAL PRIMARY KEY,
    "ProductId" INT NOT NULL,
    "Stock" INT NOT NULL DEFAULT 0,
    "MinStock" INT NOT NULL DEFAULT 0,
    "MaxStock" INT NOT NULL DEFAULT 1000,
    CONSTRAINT "CK_Stock_Positive" CHECK ("Stock" >= 0),
    CONSTRAINT "CK_MinStock_Positive" CHECK ("MinStock" >= 0),
    CONSTRAINT "CK_MaxStock_Valid" CHECK ("MaxStock" > "MinStock"),
    CONSTRAINT "FK_ProductInStock_Products" FOREIGN KEY ("ProductId") REFERENCES "Catalog"."Products" ("ProductId") ON DELETE CASCADE
);

CREATE UNIQUE INDEX "IX_ProductInStock_ProductId" ON "Catalog"."ProductInStock" ("ProductId");

CREATE TABLE "Catalog"."ProductRatings" (
    "ProductId" INT PRIMARY KEY,
    "AverageRating" DECIMAL(3,2) NOT NULL DEFAULT 0.0,
    "TotalReviews" INT NOT NULL DEFAULT 0,
    "Rating5Star" INT NOT NULL DEFAULT 0,
    "Rating4Star" INT NOT NULL DEFAULT 0,
    "Rating3Star" INT NOT NULL DEFAULT 0,
    "Rating2Star" INT NOT NULL DEFAULT 0,
    "Rating1Star" INT NOT NULL DEFAULT 0,
    "LastUpdated" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_ProductRatings_Products" FOREIGN KEY ("ProductId") REFERENCES "Catalog"."Products" ("ProductId") ON DELETE CASCADE
);

CREATE INDEX "IX_ProductRatings_Rating" ON "Catalog"."ProductRatings" ("AverageRating" DESC);

CREATE TABLE "Catalog"."ProductReviews" (
    "ReviewId" BIGSERIAL PRIMARY KEY,
    "ProductId" INT NOT NULL,
    "UserId" INT NOT NULL,
    "Rating" DECIMAL(2,1) NOT NULL,
    "Title" VARCHAR(200) NULL,
    "Comment" TEXT NULL,
    "IsVerifiedPurchase" BOOLEAN NOT NULL DEFAULT FALSE,
    "HelpfulCount" INT NOT NULL DEFAULT 0,
    "NotHelpfulCount" INT NOT NULL DEFAULT 0,
    "IsApproved" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "CK_ProductReviews_Rating" CHECK ("Rating" >= 1.0 AND "Rating" <= 5.0),
    CONSTRAINT "FK_ProductReviews_Products" FOREIGN KEY ("ProductId") REFERENCES "Catalog"."Products" ("ProductId") ON DELETE CASCADE
);

CREATE INDEX "IX_ProductReviews_Product_Rating" ON "Catalog"."ProductReviews" ("ProductId", "Rating", "IsApproved");
CREATE INDEX "IX_ProductReviews_User" ON "Catalog"."ProductReviews" ("UserId");
CREATE INDEX "IX_ProductReviews_CreatedAt" ON "Catalog"."ProductReviews" ("CreatedAt");

CREATE TABLE "Catalog"."ProductAttributes" (
    "AttributeId" SERIAL PRIMARY KEY,
    "AttributeName" VARCHAR(100) NOT NULL,
    "AttributeNameEnglish" VARCHAR(100) NULL,
    "AttributeType" VARCHAR(20) NOT NULL,
    "Unit" VARCHAR(20) NULL,
    "IsFilterable" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsSearchable" BOOLEAN NOT NULL DEFAULT FALSE,
    "DisplayOrder" INT NOT NULL DEFAULT 0,
    "CategoryId" INT NULL,
    CONSTRAINT "FK_ProductAttributes_Categories" FOREIGN KEY ("CategoryId") REFERENCES "Catalog"."Categories" ("CategoryId") ON DELETE SET NULL
);

CREATE INDEX "IX_ProductAttributes_AttributeName" ON "Catalog"."ProductAttributes" ("AttributeName");
CREATE INDEX "IX_ProductAttributes_CategoryId_IsFilterable" ON "Catalog"."ProductAttributes" ("CategoryId", "IsFilterable");

CREATE TABLE "Catalog"."AttributeValues" (
    "ValueId" SERIAL PRIMARY KEY,
    "AttributeId" INT NOT NULL,
    "ValueText" VARCHAR(200) NOT NULL,
    "ValueTextEnglish" VARCHAR(200) NULL,
    "DisplayOrder" INT NOT NULL DEFAULT 0,
    CONSTRAINT "FK_AttributeValues_ProductAttributes" FOREIGN KEY ("AttributeId") REFERENCES "Catalog"."ProductAttributes" ("AttributeId") ON DELETE CASCADE
);

CREATE INDEX "IX_AttributeValues_AttributeId" ON "Catalog"."AttributeValues" ("AttributeId");
CREATE INDEX "IX_AttributeValues_AttributeId_DisplayOrder" ON "Catalog"."AttributeValues" ("AttributeId", "DisplayOrder");

CREATE TABLE "Catalog"."ProductAttributeValues" (
    "ProductId" INT NOT NULL,
    "AttributeId" INT NOT NULL,
    "ValueId" INT NOT NULL,
    "TextValue" VARCHAR(500) NULL,
    "NumericValue" DECIMAL(18,4) NULL,
    "BooleanValue" BOOLEAN NULL,
    PRIMARY KEY ("ProductId", "AttributeId", "ValueId"),
    CONSTRAINT "FK_ProductAttributeValues_Products" FOREIGN KEY ("ProductId") REFERENCES "Catalog"."Products" ("ProductId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductAttributeValues_ProductAttributes" FOREIGN KEY ("AttributeId") REFERENCES "Catalog"."ProductAttributes" ("AttributeId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductAttributeValues_AttributeValues" FOREIGN KEY ("ValueId") REFERENCES "Catalog"."AttributeValues" ("ValueId") ON DELETE NO ACTION
);

CREATE INDEX "IX_ProductAttributeValues_Attribute_Value" ON "Catalog"."ProductAttributeValues" ("AttributeId", "ValueId");
CREATE INDEX "IX_ProductAttributeValues_ValueId" ON "Catalog"."ProductAttributeValues" ("ValueId");
CREATE INDEX "IX_ProductAttributeValues_Attribute_Numeric" ON "Catalog"."ProductAttributeValues" ("AttributeId", "NumericValue") WHERE "NumericValue" IS NOT NULL;

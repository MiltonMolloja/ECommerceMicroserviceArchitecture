-- ============================================
-- E-Commerce Microservices - PostgreSQL Schema
-- Complete Database Creation Script
-- ============================================
-- This script creates all schemas and tables for the E-Commerce platform
-- Execute this BEFORE importing data
-- ============================================

-- ============================================
-- 1. CREATE SCHEMAS
-- ============================================

CREATE SCHEMA IF NOT EXISTS "Identity";
CREATE SCHEMA IF NOT EXISTS "Catalog";
CREATE SCHEMA IF NOT EXISTS "Customer";
CREATE SCHEMA IF NOT EXISTS "Order";
CREATE SCHEMA IF NOT EXISTS "Cart";
CREATE SCHEMA IF NOT EXISTS "Payment";
CREATE SCHEMA IF NOT EXISTS "Notification";

-- ============================================
-- 2. IDENTITY SCHEMA
-- ============================================

-- AspNetRoles
CREATE TABLE IF NOT EXISTS "Identity"."AspNetRoles" (
    "Id" VARCHAR(450) PRIMARY KEY,
    "Name" VARCHAR(256) NULL,
    "NormalizedName" VARCHAR(256) NULL,
    "ConcurrencyStamp" TEXT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS "RoleNameIndex" ON "Identity"."AspNetRoles" ("NormalizedName");

-- AspNetUsers
CREATE TABLE IF NOT EXISTS "Identity"."AspNetUsers" (
    "Id" VARCHAR(450) PRIMARY KEY,
    "UserName" VARCHAR(256) NULL,
    "NormalizedUserName" VARCHAR(256) NULL,
    "Email" VARCHAR(256) NULL,
    "NormalizedEmail" VARCHAR(256) NULL,
    "EmailConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "PasswordHash" TEXT NULL,
    "SecurityStamp" TEXT NULL,
    "ConcurrencyStamp" TEXT NULL,
    "PhoneNumber" TEXT NULL,
    "PhoneNumberConfirmed" BOOLEAN NOT NULL DEFAULT FALSE,
    "TwoFactorEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "LockoutEnd" TIMESTAMPTZ NULL,
    "LockoutEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "AccessFailedCount" INT NOT NULL DEFAULT 0,
    "FirstName" VARCHAR(100) NULL,
    "LastName" VARCHAR(100) NULL,
    "DateOfBirth" DATE NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "LastLoginAt" TIMESTAMP NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX IF NOT EXISTS "UserNameIndex" ON "Identity"."AspNetUsers" ("NormalizedUserName");
CREATE INDEX IF NOT EXISTS "EmailIndex" ON "Identity"."AspNetUsers" ("NormalizedEmail");

-- AspNetUserRoles
CREATE TABLE IF NOT EXISTS "Identity"."AspNetUserRoles" (
    "UserId" VARCHAR(450) NOT NULL,
    "RoleId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("UserId", "RoleId"),
    CONSTRAINT "FK_AspNetUserRoles_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE,
    CONSTRAINT "FK_AspNetUserRoles_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "Identity"."AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserRoles_RoleId" ON "Identity"."AspNetUserRoles" ("RoleId");

-- AspNetUserClaims
CREATE TABLE IF NOT EXISTS "Identity"."AspNetUserClaims" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetUserClaims_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserClaims_UserId" ON "Identity"."AspNetUserClaims" ("UserId");

-- AspNetRoleClaims
CREATE TABLE IF NOT EXISTS "Identity"."AspNetRoleClaims" (
    "Id" SERIAL PRIMARY KEY,
    "RoleId" VARCHAR(450) NOT NULL,
    "ClaimType" TEXT NULL,
    "ClaimValue" TEXT NULL,
    CONSTRAINT "FK_AspNetRoleClaims_AspNetRoles" FOREIGN KEY ("RoleId") REFERENCES "Identity"."AspNetRoles" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetRoleClaims_RoleId" ON "Identity"."AspNetRoleClaims" ("RoleId");

-- AspNetUserLogins
CREATE TABLE IF NOT EXISTS "Identity"."AspNetUserLogins" (
    "LoginProvider" VARCHAR(450) NOT NULL,
    "ProviderKey" VARCHAR(450) NOT NULL,
    "ProviderDisplayName" TEXT NULL,
    "UserId" VARCHAR(450) NOT NULL,
    PRIMARY KEY ("LoginProvider", "ProviderKey"),
    CONSTRAINT "FK_AspNetUserLogins_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AspNetUserLogins_UserId" ON "Identity"."AspNetUserLogins" ("UserId");

-- AspNetUserTokens
CREATE TABLE IF NOT EXISTS "Identity"."AspNetUserTokens" (
    "UserId" VARCHAR(450) NOT NULL,
    "LoginProvider" VARCHAR(450) NOT NULL,
    "Name" VARCHAR(450) NOT NULL,
    "Value" TEXT NULL,
    PRIMARY KEY ("UserId", "LoginProvider", "Name"),
    CONSTRAINT "FK_AspNetUserTokens_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE
);

-- RefreshTokens
CREATE TABLE IF NOT EXISTS "Identity"."RefreshTokens" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "Token" VARCHAR(500) NOT NULL,
    "JwtId" VARCHAR(500) NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsRevoked" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpiresAt" TIMESTAMP NOT NULL,
    "RevokedAt" TIMESTAMP NULL,
    "ReplacedByToken" VARCHAR(500) NULL,
    CONSTRAINT "FK_RefreshTokens_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_UserId" ON "Identity"."RefreshTokens" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_Token" ON "Identity"."RefreshTokens" ("Token");
CREATE INDEX IF NOT EXISTS "IX_RefreshTokens_JwtId" ON "Identity"."RefreshTokens" ("JwtId");

-- UserBackupCodes
CREATE TABLE IF NOT EXISTS "Identity"."UserBackupCodes" (
    "Id" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "Code" VARCHAR(100) NOT NULL,
    "IsUsed" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UsedAt" TIMESTAMP NULL,
    CONSTRAINT "FK_UserBackupCodes_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserBackupCodes_UserId" ON "Identity"."UserBackupCodes" ("UserId");

-- UserAuditLogs
CREATE TABLE IF NOT EXISTS "Identity"."UserAuditLogs" (
    "Id" BIGSERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NOT NULL,
    "Action" VARCHAR(100) NOT NULL,
    "Details" TEXT NULL,
    "IpAddress" VARCHAR(50) NULL,
    "UserAgent" VARCHAR(500) NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_UserAuditLogs_AspNetUsers" FOREIGN KEY ("UserId") REFERENCES "Identity"."AspNetUsers" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_UserAuditLogs_UserId" ON "Identity"."UserAuditLogs" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_UserAuditLogs_CreatedAt" ON "Identity"."UserAuditLogs" ("CreatedAt");

-- ============================================
-- 3. CATALOG SCHEMA
-- ============================================

-- Brands
CREATE TABLE IF NOT EXISTS "Catalog"."Brands" (
    "BrandId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(200) NOT NULL,
    "Description" VARCHAR(1000) NULL,
    "LogoUrl" VARCHAR(500) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Brands_Name" ON "Catalog"."Brands" ("Name");
CREATE INDEX IF NOT EXISTS "IX_Brands_IsActive" ON "Catalog"."Brands" ("IsActive");

-- Categories
CREATE TABLE IF NOT EXISTS "Catalog"."Categories" (
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

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Categories_Slug" ON "Catalog"."Categories" ("Slug");
CREATE INDEX IF NOT EXISTS "IX_Categories_ParentCategoryId" ON "Catalog"."Categories" ("ParentCategoryId");
CREATE INDEX IF NOT EXISTS "IX_Categories_IsActive" ON "Catalog"."Categories" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Categories_IsFeatured" ON "Catalog"."Categories" ("IsFeatured");
CREATE INDEX IF NOT EXISTS "IX_Categories_DisplayOrder" ON "Catalog"."Categories" ("DisplayOrder");

-- Products
CREATE TABLE IF NOT EXISTS "Catalog"."Products" (
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

CREATE UNIQUE INDEX IF NOT EXISTS "IX_Products_SKU" ON "Catalog"."Products" ("SKU");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_Products_Slug" ON "Catalog"."Products" ("Slug");
CREATE INDEX IF NOT EXISTS "IX_Products_Brand" ON "Catalog"."Products" ("Brand");
CREATE INDEX IF NOT EXISTS "IX_Products_BrandId" ON "Catalog"."Products" ("BrandId");
CREATE INDEX IF NOT EXISTS "IX_Products_IsActive" ON "Catalog"."Products" ("IsActive");
CREATE INDEX IF NOT EXISTS "IX_Products_IsFeatured" ON "Catalog"."Products" ("IsFeatured");
CREATE INDEX IF NOT EXISTS "IX_Products_IsActive_IsFeatured" ON "Catalog"."Products" ("IsActive", "IsFeatured");
CREATE INDEX IF NOT EXISTS "IX_Products_TotalSold" ON "Catalog"."Products" ("TotalSold");

-- Banners
CREATE TABLE IF NOT EXISTS "Catalog"."Banners" (
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

CREATE INDEX IF NOT EXISTS "IX_Banners_DisplayOrder" ON "Catalog"."Banners" ("DisplayOrder");
CREATE INDEX IF NOT EXISTS "IX_Banners_Position_Active" ON "Catalog"."Banners" ("Position", "IsActive");
CREATE INDEX IF NOT EXISTS "IX_Banners_Dates" ON "Catalog"."Banners" ("StartDate", "EndDate");

-- ProductCategories
CREATE TABLE IF NOT EXISTS "Catalog"."ProductCategories" (
    "ProductId" INT NOT NULL,
    "CategoryId" INT NOT NULL,
    "IsPrimary" BOOLEAN NOT NULL DEFAULT FALSE,
    PRIMARY KEY ("ProductId", "CategoryId"),
    CONSTRAINT "FK_ProductCategories_Products" FOREIGN KEY ("ProductId") REFERENCES "Catalog"."Products" ("ProductId") ON DELETE CASCADE,
    CONSTRAINT "FK_ProductCategories_Categories" FOREIGN KEY ("CategoryId") REFERENCES "Catalog"."Categories" ("CategoryId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ProductCategories_CategoryId" ON "Catalog"."ProductCategories" ("CategoryId");
CREATE INDEX IF NOT EXISTS "IX_ProductCategories_IsPrimary" ON "Catalog"."ProductCategories" ("IsPrimary");

-- ProductInStock
CREATE TABLE IF NOT EXISTS "Catalog"."ProductInStock" (
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

CREATE UNIQUE INDEX IF NOT EXISTS "IX_ProductInStock_ProductId" ON "Catalog"."ProductInStock" ("ProductId");

-- ProductRatings
CREATE TABLE IF NOT EXISTS "Catalog"."ProductRatings" (
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

CREATE INDEX IF NOT EXISTS "IX_ProductRatings_Rating" ON "Catalog"."ProductRatings" ("AverageRating" DESC);

-- ProductReviews
CREATE TABLE IF NOT EXISTS "Catalog"."ProductReviews" (
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

CREATE INDEX IF NOT EXISTS "IX_ProductReviews_Product_Rating" ON "Catalog"."ProductReviews" ("ProductId", "Rating", "IsApproved");
CREATE INDEX IF NOT EXISTS "IX_ProductReviews_User" ON "Catalog"."ProductReviews" ("UserId");
CREATE INDEX IF NOT EXISTS "IX_ProductReviews_CreatedAt" ON "Catalog"."ProductReviews" ("CreatedAt");

-- ProductAttributes
CREATE TABLE IF NOT EXISTS "Catalog"."ProductAttributes" (
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

CREATE INDEX IF NOT EXISTS "IX_ProductAttributes_AttributeName" ON "Catalog"."ProductAttributes" ("AttributeName");
CREATE INDEX IF NOT EXISTS "IX_ProductAttributes_CategoryId_IsFilterable" ON "Catalog"."ProductAttributes" ("CategoryId", "IsFilterable");

-- AttributeValues
CREATE TABLE IF NOT EXISTS "Catalog"."AttributeValues" (
    "ValueId" SERIAL PRIMARY KEY,
    "AttributeId" INT NOT NULL,
    "ValueText" VARCHAR(200) NOT NULL,
    "ValueTextEnglish" VARCHAR(200) NULL,
    "DisplayOrder" INT NOT NULL DEFAULT 0,
    CONSTRAINT "FK_AttributeValues_ProductAttributes" FOREIGN KEY ("AttributeId") REFERENCES "Catalog"."ProductAttributes" ("AttributeId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_AttributeValues_AttributeId" ON "Catalog"."AttributeValues" ("AttributeId");
CREATE INDEX IF NOT EXISTS "IX_AttributeValues_AttributeId_DisplayOrder" ON "Catalog"."AttributeValues" ("AttributeId", "DisplayOrder");

-- ProductAttributeValues
CREATE TABLE IF NOT EXISTS "Catalog"."ProductAttributeValues" (
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

CREATE INDEX IF NOT EXISTS "IX_ProductAttributeValues_Attribute_Value" ON "Catalog"."ProductAttributeValues" ("AttributeId", "ValueId");
CREATE INDEX IF NOT EXISTS "IX_ProductAttributeValues_ValueId" ON "Catalog"."ProductAttributeValues" ("ValueId");

-- ============================================
-- 4. CUSTOMER SCHEMA
-- ============================================

-- Clients
CREATE TABLE IF NOT EXISTS "Customer"."Clients" (
    "ClientId" SERIAL PRIMARY KEY,
    "UserId" VARCHAR(450) NULL,
    "Phone" VARCHAR(20) NULL,
    "MobilePhone" VARCHAR(20) NULL,
    "Gender" VARCHAR(20) NULL,
    "ProfileImageUrl" VARCHAR(500) NULL,
    "PreferredLanguage" VARCHAR(10) NOT NULL DEFAULT 'es',
    "PreferredCurrency" VARCHAR(3) NOT NULL DEFAULT 'USD',
    "NewsletterSubscribed" BOOLEAN NOT NULL DEFAULT FALSE,
    "SmsNotificationsEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "EmailNotificationsEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "IsEmailVerified" BOOLEAN NOT NULL DEFAULT FALSE,
    "IsPhoneVerified" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_Clients_UserId" ON "Customer"."Clients" ("UserId");

-- ClientAddresses
CREATE TABLE IF NOT EXISTS "Customer"."ClientAddresses" (
    "AddressId" SERIAL PRIMARY KEY,
    "ClientId" INT NOT NULL,
    "AddressType" VARCHAR(50) NOT NULL,
    "RecipientName" VARCHAR(200) NOT NULL,
    "Phone" VARCHAR(20) NULL,
    "AddressLine1" VARCHAR(200) NOT NULL,
    "AddressLine2" VARCHAR(200) NULL,
    "City" VARCHAR(100) NOT NULL,
    "State" VARCHAR(100) NULL,
    "PostalCode" VARCHAR(20) NOT NULL,
    "Country" VARCHAR(100) NOT NULL,
    "IsDefault" BOOLEAN NOT NULL DEFAULT FALSE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_ClientAddresses_Clients" FOREIGN KEY ("ClientId") REFERENCES "Customer"."Clients" ("ClientId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_ClientAddresses_ClientId" ON "Customer"."ClientAddresses" ("ClientId");
CREATE INDEX IF NOT EXISTS "IX_ClientAddresses_IsDefault" ON "Customer"."ClientAddresses" ("IsDefault");

-- ============================================
-- 5. ORDER SCHEMA
-- ============================================

-- Orders
CREATE TABLE IF NOT EXISTS "Order"."Orders" (
    "OrderId" SERIAL PRIMARY KEY,
    "ClientId" INT NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "OrderDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ShippingRecipientName" VARCHAR(200) NULL,
    "ShippingPhone" VARCHAR(20) NULL,
    "ShippingAddressLine1" VARCHAR(200) NULL,
    "ShippingAddressLine2" VARCHAR(200) NULL,
    "ShippingCity" VARCHAR(100) NULL,
    "ShippingState" VARCHAR(100) NULL,
    "ShippingPostalCode" VARCHAR(20) NULL,
    "ShippingCountry" VARCHAR(100) NULL,
    "BillingAddressLine1" VARCHAR(200) NULL,
    "BillingCity" VARCHAR(100) NULL,
    "BillingPostalCode" VARCHAR(20) NULL,
    "BillingCountry" VARCHAR(100) NULL,
    "Subtotal" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TaxTotal" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "ShippingCost" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "DiscountTotal" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Total" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "PaymentMethod" VARCHAR(50) NULL,
    "PaymentStatus" VARCHAR(50) NULL,
    "TrackingNumber" VARCHAR(100) NULL,
    "EstimatedDeliveryDate" TIMESTAMP NULL,
    "DeliveredAt" TIMESTAMP NULL,
    "CancelledAt" TIMESTAMP NULL,
    "CancellationReason" TEXT NULL,
    "Notes" TEXT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_Orders_ClientId" ON "Order"."Orders" ("ClientId");
CREATE INDEX IF NOT EXISTS "IX_Orders_Status" ON "Order"."Orders" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Orders_OrderDate" ON "Order"."Orders" ("OrderDate");

-- OrderDetails
CREATE TABLE IF NOT EXISTS "Order"."OrderDetails" (
    "OrderDetailId" SERIAL PRIMARY KEY,
    "OrderId" INT NOT NULL,
    "ProductId" INT NOT NULL,
    "ProductName" VARCHAR(200) NOT NULL,
    "ProductSKU" VARCHAR(50) NULL,
    "Quantity" INT NOT NULL,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "DiscountPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "DiscountAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "TaxRate" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "TaxAmount" DECIMAL(18,2) NOT NULL DEFAULT 0,
    "Subtotal" DECIMAL(18,2) NOT NULL,
    "Total" DECIMAL(18,2) NOT NULL,
    CONSTRAINT "FK_OrderDetails_Orders" FOREIGN KEY ("OrderId") REFERENCES "Order"."Orders" ("OrderId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_OrderDetails_OrderId" ON "Order"."OrderDetails" ("OrderId");
CREATE INDEX IF NOT EXISTS "IX_OrderDetails_ProductId" ON "Order"."OrderDetails" ("ProductId");

-- ============================================
-- 6. CART SCHEMA
-- ============================================

-- ShoppingCarts
CREATE TABLE IF NOT EXISTS "Cart"."ShoppingCarts" (
    "CartId" SERIAL PRIMARY KEY,
    "ClientId" INT NULL,
    "SessionId" VARCHAR(100) NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Active',
    "CouponCode" VARCHAR(50) NULL,
    "CouponDiscountPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "ExpiresAt" TIMESTAMP NULL,
    "ConvertedAt" TIMESTAMP NULL,
    "OrderId" INT NULL
);

CREATE INDEX IF NOT EXISTS "IX_ShoppingCarts_ClientId" ON "Cart"."ShoppingCarts" ("ClientId");
CREATE INDEX IF NOT EXISTS "IX_ShoppingCarts_SessionId" ON "Cart"."ShoppingCarts" ("SessionId");
CREATE INDEX IF NOT EXISTS "IX_ShoppingCarts_Status" ON "Cart"."ShoppingCarts" ("Status");
CREATE INDEX IF NOT EXISTS "IX_ShoppingCarts_ClientId_Status" ON "Cart"."ShoppingCarts" ("ClientId", "Status");

-- CartItems
CREATE TABLE IF NOT EXISTS "Cart"."CartItems" (
    "CartItemId" SERIAL PRIMARY KEY,
    "CartId" INT NOT NULL,
    "ProductId" INT NOT NULL,
    "Quantity" INT NOT NULL DEFAULT 1,
    "UnitPrice" DECIMAL(18,2) NOT NULL,
    "DiscountPercentage" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "TaxRate" DECIMAL(5,2) NOT NULL DEFAULT 0,
    "AddedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_CartItems_ShoppingCarts" FOREIGN KEY ("CartId") REFERENCES "Cart"."ShoppingCarts" ("CartId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_CartItems_CartId" ON "Cart"."CartItems" ("CartId");
CREATE INDEX IF NOT EXISTS "IX_CartItems_ProductId" ON "Cart"."CartItems" ("ProductId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_CartItems_Cart_Product" ON "Cart"."CartItems" ("CartId", "ProductId");

-- ============================================
-- 7. PAYMENT SCHEMA
-- ============================================

-- Payments
CREATE TABLE IF NOT EXISTS "Payment"."Payments" (
    "PaymentId" SERIAL PRIMARY KEY,
    "OrderId" INT NOT NULL,
    "ClientId" INT NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Currency" VARCHAR(3) NOT NULL DEFAULT 'USD',
    "PaymentMethod" VARCHAR(50) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "TransactionId" VARCHAR(200) NULL,
    "PaymentGateway" VARCHAR(50) NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "CompletedAt" TIMESTAMP NULL,
    "FailedAt" TIMESTAMP NULL,
    "FailureReason" TEXT NULL
);

CREATE INDEX IF NOT EXISTS "IX_Payments_OrderId" ON "Payment"."Payments" ("OrderId");
CREATE INDEX IF NOT EXISTS "IX_Payments_ClientId" ON "Payment"."Payments" ("ClientId");
CREATE INDEX IF NOT EXISTS "IX_Payments_Status" ON "Payment"."Payments" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Payments_TransactionId" ON "Payment"."Payments" ("TransactionId");

-- PaymentDetails
CREATE TABLE IF NOT EXISTS "Payment"."PaymentDetails" (
    "PaymentDetailId" SERIAL PRIMARY KEY,
    "PaymentId" INT NOT NULL,
    "Key" VARCHAR(100) NOT NULL,
    "Value" TEXT NULL,
    CONSTRAINT "FK_PaymentDetails_Payments" FOREIGN KEY ("PaymentId") REFERENCES "Payment"."Payments" ("PaymentId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_PaymentDetails_PaymentId" ON "Payment"."PaymentDetails" ("PaymentId");

-- PaymentTransactions
CREATE TABLE IF NOT EXISTS "Payment"."PaymentTransactions" (
    "TransactionId" SERIAL PRIMARY KEY,
    "PaymentId" INT NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Amount" DECIMAL(18,2) NOT NULL,
    "Status" VARCHAR(50) NOT NULL,
    "GatewayTransactionId" VARCHAR(200) NULL,
    "GatewayResponse" TEXT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT "FK_PaymentTransactions_Payments" FOREIGN KEY ("PaymentId") REFERENCES "Payment"."Payments" ("PaymentId") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_PaymentTransactions_PaymentId" ON "Payment"."PaymentTransactions" ("PaymentId");
CREATE INDEX IF NOT EXISTS "IX_PaymentTransactions_GatewayTransactionId" ON "Payment"."PaymentTransactions" ("GatewayTransactionId");

-- ============================================
-- 8. NOTIFICATION SCHEMA
-- ============================================

-- NotificationTemplates
CREATE TABLE IF NOT EXISTS "Notification"."NotificationTemplates" (
    "TemplateId" SERIAL PRIMARY KEY,
    "TemplateName" VARCHAR(100) NOT NULL,
    "TemplateType" VARCHAR(50) NOT NULL,
    "Subject" VARCHAR(200) NULL,
    "Body" TEXT NOT NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE UNIQUE INDEX IF NOT EXISTS "IX_NotificationTemplates_TemplateName" ON "Notification"."NotificationTemplates" ("TemplateName");

-- Notifications
CREATE TABLE IF NOT EXISTS "Notification"."Notifications" (
    "NotificationId" BIGSERIAL PRIMARY KEY,
    "ClientId" INT NOT NULL,
    "Type" VARCHAR(50) NOT NULL,
    "Channel" VARCHAR(50) NOT NULL,
    "Subject" VARCHAR(200) NULL,
    "Message" TEXT NOT NULL,
    "Status" VARCHAR(50) NOT NULL DEFAULT 'Pending',
    "SentAt" TIMESTAMP NULL,
    "ReadAt" TIMESTAMP NULL,
    "FailedAt" TIMESTAMP NULL,
    "FailureReason" TEXT NULL,
    "Metadata" TEXT NULL,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_Notifications_ClientId" ON "Notification"."Notifications" ("ClientId");
CREATE INDEX IF NOT EXISTS "IX_Notifications_Status" ON "Notification"."Notifications" ("Status");
CREATE INDEX IF NOT EXISTS "IX_Notifications_CreatedAt" ON "Notification"."Notifications" ("CreatedAt");

-- NotificationPreferences
CREATE TABLE IF NOT EXISTS "Notification"."NotificationPreferences" (
    "PreferenceId" SERIAL PRIMARY KEY,
    "ClientId" INT NOT NULL,
    "NotificationType" VARCHAR(50) NOT NULL,
    "EmailEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "SmsEnabled" BOOLEAN NOT NULL DEFAULT FALSE,
    "PushEnabled" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "UpdatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX IF NOT EXISTS "IX_NotificationPreferences_ClientId" ON "Notification"."NotificationPreferences" ("ClientId");
CREATE UNIQUE INDEX IF NOT EXISTS "IX_NotificationPreferences_Client_Type" ON "Notification"."NotificationPreferences" ("ClientId", "NotificationType");

-- ============================================
-- SCRIPT COMPLETE
-- ============================================

-- Display completion message
DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'PostgreSQL Schema Creation Complete!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Schemas created: 7';
    RAISE NOTICE 'Tables created: ~40';
    RAISE NOTICE '';
    RAISE NOTICE 'Next steps:';
    RAISE NOTICE '1. Verify tables: SELECT schemaname, tablename FROM pg_tables WHERE schemaname NOT IN (''pg_catalog'', ''information_schema'') ORDER BY schemaname, tablename;';
    RAISE NOTICE '2. Import data using migration-data/*.sql files';
    RAISE NOTICE '============================================';
END $$;

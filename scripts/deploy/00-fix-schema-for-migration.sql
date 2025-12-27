-- ============================================
-- Schema Fixes for SQL Server to PostgreSQL Migration
-- Adjusts PostgreSQL schema to match exported data
-- ============================================

-- ============================================
-- 1. IDENTITY SCHEMA FIXES
-- ============================================

-- RefreshTokens: Add missing columns from SQL Server
ALTER TABLE "Identity"."RefreshTokens" 
ADD COLUMN IF NOT EXISTS "CreatedByIp" VARCHAR(50) NULL,
ADD COLUMN IF NOT EXISTS "RevokedByIp" VARCHAR(50) NULL,
ADD COLUMN IF NOT EXISTS "UserAgent" VARCHAR(500) NULL;

-- RefreshTokens: Make JwtId nullable temporarily
ALTER TABLE "Identity"."RefreshTokens"
ALTER COLUMN "JwtId" DROP NOT NULL;

-- UserAuditLogs: Rename CreatedAt to Timestamp to match SQL Server export
ALTER TABLE "Identity"."UserAuditLogs" 
RENAME COLUMN "CreatedAt" TO "Timestamp";

-- ============================================
-- 2. CUSTOMER SCHEMA FIXES
-- ============================================

-- Clients: Add columns from SQL Server that don't exist in PostgreSQL
ALTER TABLE "Customer"."Clients"
ADD COLUMN IF NOT EXISTS "Name" VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS "Email" VARCHAR(256) NULL,
ADD COLUMN IF NOT EXISTS "Address" VARCHAR(500) NULL,
ADD COLUMN IF NOT EXISTS "City" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "State" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "Country" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "PostalCode" VARCHAR(20) NULL;

-- ClientAddresses: Add missing columns
ALTER TABLE "Customer"."ClientAddresses"
ADD COLUMN IF NOT EXISTS "AddressName" VARCHAR(100) NULL,
ADD COLUMN IF NOT EXISTS "RecipientPhone" VARCHAR(20) NULL;

-- ============================================
-- 3. ORDER SCHEMA FIXES
-- ============================================

-- Orders: Rename columns to match SQL Server export (case sensitivity)
ALTER TABLE "Order"."Orders"
RENAME COLUMN "Subtotal" TO "SubTotal";

ALTER TABLE "Order"."Orders"
RENAME COLUMN "TaxTotal" TO "Tax";

ALTER TABLE "Order"."Orders"
RENAME COLUMN "DiscountTotal" TO "Discount";

-- Orders: Add missing columns
ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "ShippingAddress" VARCHAR(500) NULL,
DROP COLUMN IF EXISTS "ShippingAddressLine1",
DROP COLUMN IF EXISTS "ShippingAddressLine2";

-- OrderDetails: Make Subtotal nullable temporarily for import
ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "Subtotal" DROP NOT NULL;

-- Orders: Make UpdatedAt nullable
ALTER TABLE "Order"."Orders"
ALTER COLUMN "UpdatedAt" DROP NOT NULL;

-- Create PaymentTypes and OrderStatuses reference tables (they don't exist in PostgreSQL schema)
CREATE TABLE IF NOT EXISTS "Order"."PaymentTypes" (
    "PaymentTypeId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE IF NOT EXISTS "Order"."OrderStatuses" (
    "OrderStatusId" SERIAL PRIMARY KEY,
    "Name" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "DisplayOrder" INT NOT NULL DEFAULT 0
);

-- ============================================
-- 4. CART SCHEMA FIXES
-- ============================================

-- Rename ShoppingCarts to Carts to match migration scripts
ALTER TABLE "Cart"."ShoppingCarts" RENAME TO "Carts";

-- Carts: Add missing columns from SQL Server
ALTER TABLE "Cart"."Carts"
ADD COLUMN IF NOT EXISTS "IsActive" BOOLEAN NOT NULL DEFAULT TRUE;

-- CartItems: Add missing columns
ALTER TABLE "Cart"."CartItems"
ADD COLUMN IF NOT EXISTS "ProductName" VARCHAR(200) NULL,
ADD COLUMN IF NOT EXISTS "ProductSKU" VARCHAR(50) NULL;

-- Fix FK constraint name after table rename
ALTER TABLE "Cart"."CartItems" DROP CONSTRAINT IF EXISTS "FK_CartItems_ShoppingCarts";
ALTER TABLE "Cart"."CartItems" ADD CONSTRAINT "FK_CartItems_Carts" 
    FOREIGN KEY ("CartId") REFERENCES "Cart"."Carts" ("CartId") ON DELETE CASCADE;

-- ============================================
-- 5. PAYMENT SCHEMA FIXES
-- ============================================

-- Payments: Rename ClientId to UserId to match SQL Server export
ALTER TABLE "Payment"."Payments"
RENAME COLUMN "ClientId" TO "UserId";

-- Payments: Add missing columns
ALTER TABLE "Payment"."Payments"
ADD COLUMN IF NOT EXISTS "PaymentDate" TIMESTAMP NULL;

-- PaymentDetails: Add columns from SQL Server
ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "CardLast4Digits" VARCHAR(4) NULL,
ADD COLUMN IF NOT EXISTS "CardBrand" VARCHAR(50) NULL,
ADD COLUMN IF NOT EXISTS "CardExpiryMonth" INT NULL,
ADD COLUMN IF NOT EXISTS "CardExpiryYear" INT NULL,
ADD COLUMN IF NOT EXISTS "ExpiryMonth" INT NULL,
ADD COLUMN IF NOT EXISTS "ExpiryYear" INT NULL;

-- PaymentTransactions: Rename Type to TransactionType
ALTER TABLE "Payment"."PaymentTransactions"
RENAME COLUMN "Type" TO "TransactionType";

-- PaymentTransactions: Add missing columns
ALTER TABLE "Payment"."PaymentTransactions"
ADD COLUMN IF NOT EXISTS "ErrorMessage" TEXT NULL;

-- ============================================
-- 6. NOTIFICATION SCHEMA FIXES
-- ============================================

-- NotificationPreferences: Rename ClientId to UserId
ALTER TABLE "Notification"."NotificationPreferences"
RENAME COLUMN "ClientId" TO "UserId";

-- NotificationPreferences: Add missing columns
ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "EmailNotifications" BOOLEAN NOT NULL DEFAULT TRUE,
ADD COLUMN IF NOT EXISTS "SmsNotifications" BOOLEAN NOT NULL DEFAULT FALSE,
ADD COLUMN IF NOT EXISTS "PushNotifications" BOOLEAN NOT NULL DEFAULT TRUE;

-- ============================================
-- 7. CATALOG SCHEMA - Fix import order issue
-- ============================================

-- No changes needed, just need to import in correct order:
-- ProductAttributes BEFORE AttributeValues

-- ============================================
-- COMPLETION MESSAGE
-- ============================================

DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Schema fixes applied successfully!';
    RAISE NOTICE '============================================';
    RAISE NOTICE 'Tables adjusted to match SQL Server export';
    RAISE NOTICE 'Ready for data import';
    RAISE NOTICE '============================================';
END $$;

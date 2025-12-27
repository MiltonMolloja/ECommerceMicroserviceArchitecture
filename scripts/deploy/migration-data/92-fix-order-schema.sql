-- Fix Order schema - Add missing columns and fix types

-- Fix Status column type (enum stored as int, not string)
-- First update existing string values to their int equivalents
UPDATE "Order"."Orders" SET "Status" = '0' WHERE "Status" = 'AwaitingPayment';
UPDATE "Order"."Orders" SET "Status" = '1' WHERE "Status" = 'PaymentProcessing';
UPDATE "Order"."Orders" SET "Status" = '2' WHERE "Status" = 'PaymentFailed';
UPDATE "Order"."Orders" SET "Status" = '3' WHERE "Status" = 'Paid';
UPDATE "Order"."Orders" SET "Status" = '4' WHERE "Status" = 'Processing';
UPDATE "Order"."Orders" SET "Status" = '5' WHERE "Status" = 'ReadyToShip';
UPDATE "Order"."Orders" SET "Status" = '6' WHERE "Status" = 'Shipped';
UPDATE "Order"."Orders" SET "Status" = '7' WHERE "Status" = 'InTransit';
UPDATE "Order"."Orders" SET "Status" = '8' WHERE "Status" = 'OutForDelivery';
UPDATE "Order"."Orders" SET "Status" = '9' WHERE "Status" = 'Delivered';
UPDATE "Order"."Orders" SET "Status" = '10' WHERE "Status" = 'Cancelled';
UPDATE "Order"."Orders" SET "Status" = '11' WHERE "Status" = 'Refunded';
UPDATE "Order"."Orders" SET "Status" = '12' WHERE "Status" = 'PartiallyRefunded';
UPDATE "Order"."Orders" SET "Status" = '13' WHERE "Status" = 'ReturnRequested';
UPDATE "Order"."Orders" SET "Status" = '14' WHERE "Status" = 'Returned';
UPDATE "Order"."Orders" SET "Status" = '15' WHERE "Status" = 'OnHold';
UPDATE "Order"."Orders" SET "Status" = '16' WHERE "Status" = 'PaymentDisputed';

-- Now change the column type to integer
ALTER TABLE "Order"."Orders" 
ALTER COLUMN "Status" TYPE INTEGER USING "Status"::INTEGER;

-- Orders missing columns
ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "BillingSameAsShipping" BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "PaymentType" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "SubTotal" DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "Tax" DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "Discount" DECIMAL(18,2) NOT NULL DEFAULT 0;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "ShippingAddress" VARCHAR(500) NULL;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "PaidAt" TIMESTAMP NULL;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "ShippedAt" TIMESTAMP NULL;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "PaymentTransactionId" VARCHAR(100) NULL;

ALTER TABLE "Order"."Orders"
ADD COLUMN IF NOT EXISTS "PaymentGateway" VARCHAR(50) NULL;

-- Copy values from existing columns if they exist
UPDATE "Order"."Orders" SET "SubTotal" = "Subtotal" WHERE "SubTotal" = 0 AND "Subtotal" > 0;
UPDATE "Order"."Orders" SET "Tax" = "TaxTotal" WHERE "Tax" = 0 AND "TaxTotal" > 0;
UPDATE "Order"."Orders" SET "Discount" = "DiscountTotal" WHERE "Discount" = 0 AND "DiscountTotal" > 0;

-- OrderDetails - make extra columns have defaults (not in entity but required by schema)
ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "Subtotal" SET DEFAULT 0;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "DiscountPercentage" SET DEFAULT 0;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "DiscountAmount" SET DEFAULT 0;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "TaxRate" SET DEFAULT 0;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "TaxAmount" SET DEFAULT 0;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "ProductName" DROP NOT NULL;

-- Also need to add computed columns or make them nullable
ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "Subtotal" DROP NOT NULL;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "DiscountPercentage" DROP NOT NULL;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "DiscountAmount" DROP NOT NULL;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "TaxRate" DROP NOT NULL;

ALTER TABLE "Order"."OrderDetails"
ALTER COLUMN "TaxAmount" DROP NOT NULL;

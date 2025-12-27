-- Cleanup duplicate/unused columns from Orders table

-- Drop old columns that are replaced by new ones
-- First copy any data to new columns
UPDATE "Order"."Orders" 
SET "SubTotal" = "Subtotal" 
WHERE "SubTotal" = 0 AND "Subtotal" IS NOT NULL AND "Subtotal" > 0;

UPDATE "Order"."Orders" 
SET "Tax" = "TaxTotal" 
WHERE "Tax" = 0 AND "TaxTotal" IS NOT NULL AND "TaxTotal" > 0;

UPDATE "Order"."Orders" 
SET "Discount" = "DiscountTotal" 
WHERE "Discount" = 0 AND "DiscountTotal" IS NOT NULL AND "DiscountTotal" > 0;

-- Now drop the old columns to avoid confusion
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "Subtotal";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "TaxTotal";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "DiscountTotal";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "ShippingCost";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "PaymentMethod";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "PaymentStatus";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "TrackingNumber";
ALTER TABLE "Order"."Orders" DROP COLUMN IF EXISTS "EstimatedDeliveryDate";

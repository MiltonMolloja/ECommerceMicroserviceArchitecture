-- Fix PaymentDetails table - completely different structure

-- Drop old columns and add new ones to match entity
ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "CardLast4Digits" VARCHAR(4) NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "CardBrand" VARCHAR(50) NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "ExpiryMonth" INTEGER NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "ExpiryYear" INTEGER NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "BillingAddress" VARCHAR(500) NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "BillingCity" VARCHAR(100) NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "BillingCountry" VARCHAR(100) NULL;

ALTER TABLE "Payment"."PaymentDetails"
ADD COLUMN IF NOT EXISTS "BillingZipCode" VARCHAR(20) NULL;

-- Drop old key-value columns
ALTER TABLE "Payment"."PaymentDetails" DROP COLUMN IF EXISTS "Key";
ALTER TABLE "Payment"."PaymentDetails" DROP COLUMN IF EXISTS "Value";

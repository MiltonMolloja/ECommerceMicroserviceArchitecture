-- Fix Payment schema - Add missing columns and fix types

-- Payments table fixes
ALTER TABLE "Payment"."Payments"
ADD COLUMN IF NOT EXISTS "PaymentDate" TIMESTAMP NULL;

-- Rename ClientId to UserId if needed (or add UserId column)
ALTER TABLE "Payment"."Payments"
ADD COLUMN IF NOT EXISTS "UserId" INTEGER NOT NULL DEFAULT 0;

-- Copy ClientId to UserId
UPDATE "Payment"."Payments" SET "UserId" = "ClientId" WHERE "UserId" = 0;

-- Fix Status column - convert from varchar to int
UPDATE "Payment"."Payments" SET "Status" = '0' WHERE "Status" = 'Pending';
UPDATE "Payment"."Payments" SET "Status" = '1' WHERE "Status" = 'Processing';
UPDATE "Payment"."Payments" SET "Status" = '2' WHERE "Status" = 'Completed';
UPDATE "Payment"."Payments" SET "Status" = '3' WHERE "Status" = 'Failed';
UPDATE "Payment"."Payments" SET "Status" = '4' WHERE "Status" = 'Refunded';
UPDATE "Payment"."Payments" SET "Status" = '5' WHERE "Status" = 'Cancelled';

ALTER TABLE "Payment"."Payments"
ALTER COLUMN "Status" TYPE INTEGER USING "Status"::INTEGER;

-- Fix PaymentMethod column - convert from varchar to int
UPDATE "Payment"."Payments" SET "PaymentMethod" = '0' WHERE "PaymentMethod" = 'CreditCard';
UPDATE "Payment"."Payments" SET "PaymentMethod" = '1' WHERE "PaymentMethod" = 'DebitCard';
UPDATE "Payment"."Payments" SET "PaymentMethod" = '2' WHERE "PaymentMethod" = 'MercadoPago';
UPDATE "Payment"."Payments" SET "PaymentMethod" = '3' WHERE "PaymentMethod" = 'PayPal';
UPDATE "Payment"."Payments" SET "PaymentMethod" = '4' WHERE "PaymentMethod" = 'BankTransfer';
UPDATE "Payment"."Payments" SET "PaymentMethod" = '5' WHERE "PaymentMethod" = 'Cash';

ALTER TABLE "Payment"."Payments"
ALTER COLUMN "PaymentMethod" TYPE INTEGER USING "PaymentMethod"::INTEGER;

-- Drop ClientId if UserId exists
ALTER TABLE "Payment"."Payments" DROP COLUMN IF EXISTS "ClientId";

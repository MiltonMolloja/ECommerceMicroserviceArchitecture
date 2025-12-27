-- Fix PaymentTransactions table

-- Add missing columns
ALTER TABLE "Payment"."PaymentTransactions"
ADD COLUMN IF NOT EXISTS "ErrorMessage" TEXT NULL;

ALTER TABLE "Payment"."PaymentTransactions"
ADD COLUMN IF NOT EXISTS "TransactionDate" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;

ALTER TABLE "Payment"."PaymentTransactions"
ADD COLUMN IF NOT EXISTS "IPAddress" VARCHAR(50) NULL;

ALTER TABLE "Payment"."PaymentTransactions"
ADD COLUMN IF NOT EXISTS "TransactionType" INTEGER NOT NULL DEFAULT 0;

-- Copy CreatedAt to TransactionDate
UPDATE "Payment"."PaymentTransactions" 
SET "TransactionDate" = "CreatedAt" 
WHERE "TransactionDate" = CURRENT_TIMESTAMP;

-- Convert Type from varchar to int (TransactionType)
UPDATE "Payment"."PaymentTransactions" SET "TransactionType" = 0 WHERE "Type" = 'Charge';
UPDATE "Payment"."PaymentTransactions" SET "TransactionType" = 1 WHERE "Type" = 'Refund';
UPDATE "Payment"."PaymentTransactions" SET "TransactionType" = 2 WHERE "Type" = 'Void';
UPDATE "Payment"."PaymentTransactions" SET "TransactionType" = 3 WHERE "Type" = 'Capture';

-- Convert Status from varchar to int
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'Payment' 
        AND table_name = 'PaymentTransactions' 
        AND column_name = 'Status' 
        AND data_type = 'character varying'
    ) THEN
        UPDATE "Payment"."PaymentTransactions" SET "Status" = '0' WHERE "Status" = 'Pending';
        UPDATE "Payment"."PaymentTransactions" SET "Status" = '1' WHERE "Status" = 'Success';
        UPDATE "Payment"."PaymentTransactions" SET "Status" = '2' WHERE "Status" = 'Failed';
        UPDATE "Payment"."PaymentTransactions" SET "Status" = '0' WHERE "Status" NOT IN ('0','1','2');
        ALTER TABLE "Payment"."PaymentTransactions" ALTER COLUMN "Status" TYPE INTEGER USING "Status"::INTEGER;
    END IF;
END $$;

-- Drop old columns
ALTER TABLE "Payment"."PaymentTransactions" DROP COLUMN IF EXISTS "Type";
ALTER TABLE "Payment"."PaymentTransactions" DROP COLUMN IF EXISTS "GatewayTransactionId";
ALTER TABLE "Payment"."PaymentTransactions" DROP COLUMN IF EXISTS "CreatedAt";

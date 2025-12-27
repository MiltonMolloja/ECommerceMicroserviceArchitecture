-- Fix Customer schema - Add missing columns

-- Clients missing columns
ALTER TABLE "Customer"."Clients"
ADD COLUMN IF NOT EXISTS "DateOfBirth" TIMESTAMP NULL;

ALTER TABLE "Customer"."Clients"
ADD COLUMN IF NOT EXISTS "LastLoginAt" TIMESTAMP NULL;

-- ClientAddresses missing columns
ALTER TABLE "Customer"."ClientAddresses"
ADD COLUMN IF NOT EXISTS "AddressName" VARCHAR(100) NULL;

ALTER TABLE "Customer"."ClientAddresses"
ADD COLUMN IF NOT EXISTS "RecipientPhone" VARCHAR(20) NULL;

ALTER TABLE "Customer"."ClientAddresses"
ADD COLUMN IF NOT EXISTS "IsDefaultShipping" BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE "Customer"."ClientAddresses"
ADD COLUMN IF NOT EXISTS "IsDefaultBilling" BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE "Customer"."ClientAddresses"
ADD COLUMN IF NOT EXISTS "IsActive" BOOLEAN NOT NULL DEFAULT TRUE;

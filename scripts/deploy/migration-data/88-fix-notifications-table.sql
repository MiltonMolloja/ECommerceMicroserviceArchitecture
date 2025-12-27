-- Fix Notifications table to match entity

-- Check current state and add missing columns
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "UserId" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "Title" VARCHAR(200) NULL;

ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "Data" TEXT NOT NULL DEFAULT '{}';

ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "IsRead" BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "Priority" INTEGER NOT NULL DEFAULT 0;

ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "ExpiresAt" TIMESTAMP NULL;

-- Set IsRead based on ReadAt
UPDATE "Notification"."Notifications" SET "IsRead" = TRUE WHERE "ReadAt" IS NOT NULL AND "IsRead" = FALSE;

-- Drop columns that are not in entity (if they still exist)
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Channel";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Status";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "SentAt";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "FailedAt";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "FailureReason";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Subject";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Metadata";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "ClientId";

-- Convert Type from varchar to int if still varchar
DO $$
BEGIN
    IF EXISTS (
        SELECT 1 FROM information_schema.columns 
        WHERE table_schema = 'Notification' 
        AND table_name = 'Notifications' 
        AND column_name = 'Type' 
        AND data_type = 'character varying'
    ) THEN
        UPDATE "Notification"."Notifications" SET "Type" = '0' WHERE "Type" NOT SIMILAR TO '[0-9]+';
        ALTER TABLE "Notification"."Notifications" ALTER COLUMN "Type" TYPE INTEGER USING "Type"::INTEGER;
    END IF;
END $$;

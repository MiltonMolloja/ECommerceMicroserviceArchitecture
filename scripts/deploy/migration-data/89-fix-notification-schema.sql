-- Fix Notification schema - Add missing columns

-- NotificationPreferences - Add UserId column (entity uses UserId, DB has ClientId)
ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "UserId" INTEGER NOT NULL DEFAULT 0;

-- Copy ClientId to UserId
UPDATE "Notification"."NotificationPreferences" SET "UserId" = "ClientId" WHERE "UserId" = 0;

-- Add notification channel preferences
ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "EmailNotifications" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "PushNotifications" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "SMSNotifications" BOOLEAN NOT NULL DEFAULT FALSE;

-- Add notification type preferences
ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "OrderUpdates" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "Promotions" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "Newsletter" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "PriceAlerts" BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE "Notification"."NotificationPreferences"
ADD COLUMN IF NOT EXISTS "StockAlerts" BOOLEAN NOT NULL DEFAULT FALSE;

-- Copy existing values
UPDATE "Notification"."NotificationPreferences" 
SET "EmailNotifications" = "EmailEnabled",
    "PushNotifications" = "PushEnabled",
    "SMSNotifications" = "SmsEnabled"
WHERE "EmailNotifications" = TRUE;

-- Drop old columns
ALTER TABLE "Notification"."NotificationPreferences" DROP COLUMN IF EXISTS "ClientId";
ALTER TABLE "Notification"."NotificationPreferences" DROP COLUMN IF EXISTS "NotificationType";
ALTER TABLE "Notification"."NotificationPreferences" DROP COLUMN IF EXISTS "EmailEnabled";
ALTER TABLE "Notification"."NotificationPreferences" DROP COLUMN IF EXISTS "SmsEnabled";
ALTER TABLE "Notification"."NotificationPreferences" DROP COLUMN IF EXISTS "PushEnabled";

-- =============================================
-- Fix Notifications table
-- =============================================

-- Add UserId column
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "UserId" INTEGER NOT NULL DEFAULT 0;

-- Copy ClientId to UserId
UPDATE "Notification"."Notifications" SET "UserId" = "ClientId" WHERE "UserId" = 0;

-- Add Title column (was Subject)
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "Title" VARCHAR(200) NULL;

-- Copy Subject to Title
UPDATE "Notification"."Notifications" SET "Title" = "Subject" WHERE "Title" IS NULL;

-- Add Data column (was Metadata)
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "Data" TEXT NOT NULL DEFAULT '{}';

-- Copy Metadata to Data
UPDATE "Notification"."Notifications" SET "Data" = COALESCE("Metadata", '{}') WHERE "Data" = '{}';

-- Add IsRead column
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "IsRead" BOOLEAN NOT NULL DEFAULT FALSE;

-- Set IsRead based on ReadAt
UPDATE "Notification"."Notifications" SET "IsRead" = TRUE WHERE "ReadAt" IS NOT NULL;

-- Add Priority column (enum as int)
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "Priority" INTEGER NOT NULL DEFAULT 0;

-- Add ExpiresAt column
ALTER TABLE "Notification"."Notifications"
ADD COLUMN IF NOT EXISTS "ExpiresAt" TIMESTAMP NULL;

-- Convert Type from varchar to int
UPDATE "Notification"."Notifications" SET "Type" = '0' WHERE "Type" = 'OrderPlaced';
UPDATE "Notification"."Notifications" SET "Type" = '1' WHERE "Type" = 'OrderShipped';
UPDATE "Notification"."Notifications" SET "Type" = '2' WHERE "Type" = 'OrderDelivered';
UPDATE "Notification"."Notifications" SET "Type" = '3' WHERE "Type" = 'OrderCancelled';
UPDATE "Notification"."Notifications" SET "Type" = '4' WHERE "Type" = 'PaymentCompleted';
UPDATE "Notification"."Notifications" SET "Type" = '5' WHERE "Type" = 'PaymentFailed';
UPDATE "Notification"."Notifications" SET "Type" = '6' WHERE "Type" = 'Promotion';
UPDATE "Notification"."Notifications" SET "Type" = '7' WHERE "Type" = 'LowStock';
UPDATE "Notification"."Notifications" SET "Type" = '8' WHERE "Type" = 'PriceDrop';
UPDATE "Notification"."Notifications" SET "Type" = '9' WHERE "Type" = 'Newsletter';
UPDATE "Notification"."Notifications" SET "Type" = '10' WHERE "Type" = 'WelcomeEmail';
UPDATE "Notification"."Notifications" SET "Type" = '11' WHERE "Type" = 'PasswordReset';
UPDATE "Notification"."Notifications" SET "Type" = '0' WHERE "Type" NOT IN ('0','1','2','3','4','5','6','7','8','9','10','11');

ALTER TABLE "Notification"."Notifications"
ALTER COLUMN "Type" TYPE INTEGER USING "Type"::INTEGER;

-- Drop old columns that are not in entity
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "ClientId";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Subject";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Metadata";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Channel";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "Status";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "SentAt";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "FailedAt";
ALTER TABLE "Notification"."Notifications" DROP COLUMN IF EXISTS "FailureReason";

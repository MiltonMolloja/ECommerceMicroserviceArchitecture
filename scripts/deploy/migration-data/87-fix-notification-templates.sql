-- Fix NotificationTemplates table to match entity

-- Add Type column (enum as int)
ALTER TABLE "Notification"."NotificationTemplates"
ADD COLUMN IF NOT EXISTS "Type" INTEGER NOT NULL DEFAULT 0;

-- Add TemplateKey column
ALTER TABLE "Notification"."NotificationTemplates"
ADD COLUMN IF NOT EXISTS "TemplateKey" VARCHAR(100) NOT NULL DEFAULT 'default';

-- Add TitleTemplate column
ALTER TABLE "Notification"."NotificationTemplates"
ADD COLUMN IF NOT EXISTS "TitleTemplate" VARCHAR(500) NOT NULL DEFAULT '';

-- Add MessageTemplate column  
ALTER TABLE "Notification"."NotificationTemplates"
ADD COLUMN IF NOT EXISTS "MessageTemplate" TEXT NOT NULL DEFAULT '';

-- Add Channel column (enum as int)
ALTER TABLE "Notification"."NotificationTemplates"
ADD COLUMN IF NOT EXISTS "Channel" INTEGER NOT NULL DEFAULT 0;

-- Copy existing data if columns exist
UPDATE "Notification"."NotificationTemplates" 
SET "TitleTemplate" = COALESCE("Subject", ''),
    "MessageTemplate" = COALESCE("Body", '')
WHERE "TitleTemplate" = '' OR "MessageTemplate" = '';

-- Convert TemplateType to Type (int)
UPDATE "Notification"."NotificationTemplates" SET "Type" = 0 WHERE "TemplateType" = 'OrderPlaced';
UPDATE "Notification"."NotificationTemplates" SET "Type" = 1 WHERE "TemplateType" = 'OrderShipped';
UPDATE "Notification"."NotificationTemplates" SET "Type" = 2 WHERE "TemplateType" = 'OrderDelivered';
UPDATE "Notification"."NotificationTemplates" SET "Type" = 3 WHERE "TemplateType" = 'OrderCancelled';
UPDATE "Notification"."NotificationTemplates" SET "Type" = 4 WHERE "TemplateType" = 'PaymentCompleted';
UPDATE "Notification"."NotificationTemplates" SET "Type" = 5 WHERE "TemplateType" = 'PaymentFailed';

-- Copy TemplateName to TemplateKey
UPDATE "Notification"."NotificationTemplates" 
SET "TemplateKey" = "TemplateName"
WHERE "TemplateKey" = 'default' AND "TemplateName" IS NOT NULL;

-- Drop old columns
ALTER TABLE "Notification"."NotificationTemplates" DROP COLUMN IF EXISTS "TemplateName";
ALTER TABLE "Notification"."NotificationTemplates" DROP COLUMN IF EXISTS "TemplateType";
ALTER TABLE "Notification"."NotificationTemplates" DROP COLUMN IF EXISTS "Subject";
ALTER TABLE "Notification"."NotificationTemplates" DROP COLUMN IF EXISTS "Body";

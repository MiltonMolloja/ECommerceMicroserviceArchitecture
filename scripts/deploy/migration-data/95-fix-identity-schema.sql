-- Fix Identity schema - Add missing columns

-- AspNetUsers missing columns
ALTER TABLE "Identity"."AspNetUsers"
ADD COLUMN IF NOT EXISTS "FirstName" VARCHAR(100) NULL;

ALTER TABLE "Identity"."AspNetUsers"
ADD COLUMN IF NOT EXISTS "LastName" VARCHAR(100) NULL;

ALTER TABLE "Identity"."AspNetUsers"
ADD COLUMN IF NOT EXISTS "PasswordChangedAt" TIMESTAMP NULL;

ALTER TABLE "Identity"."AspNetUsers"
ADD COLUMN IF NOT EXISTS "Discriminator" VARCHAR(256) NOT NULL DEFAULT 'ApplicationUser';

-- AspNetRoles missing columns (for inheritance TPH)
ALTER TABLE "Identity"."AspNetRoles"
ADD COLUMN IF NOT EXISTS "Discriminator" VARCHAR(256) NOT NULL DEFAULT 'IdentityRole';

-- AspNetUserRoles missing columns (for ApplicationUserRole inheritance)
ALTER TABLE "Identity"."AspNetUserRoles"
ADD COLUMN IF NOT EXISTS "Discriminator" VARCHAR(256) NOT NULL DEFAULT 'ApplicationUserRole';

-- Update existing rows
UPDATE "Identity"."AspNetUsers" SET "Discriminator" = 'ApplicationUser' WHERE "Discriminator" IS NULL OR "Discriminator" = '';
UPDATE "Identity"."AspNetRoles" SET "Discriminator" = 'IdentityRole' WHERE "Discriminator" IS NULL OR "Discriminator" = '';
UPDATE "Identity"."AspNetUserRoles" SET "Discriminator" = 'ApplicationUserRole' WHERE "Discriminator" IS NULL OR "Discriminator" = '';

-- RefreshTokens missing columns
ALTER TABLE "Identity"."RefreshTokens"
ADD COLUMN IF NOT EXISTS "CreatedByIp" VARCHAR(50) NULL;

ALTER TABLE "Identity"."RefreshTokens"
ADD COLUMN IF NOT EXISTS "RevokedByIp" VARCHAR(50) NULL;

ALTER TABLE "Identity"."RefreshTokens"
ADD COLUMN IF NOT EXISTS "UserAgent" VARCHAR(500) NULL;

-- Make JwtId nullable (not used in current implementation)
ALTER TABLE "Identity"."RefreshTokens"
ALTER COLUMN "JwtId" DROP NOT NULL;

-- UserAuditLogs missing columns
ALTER TABLE "Identity"."UserAuditLogs"
ADD COLUMN IF NOT EXISTS "Timestamp" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP;

ALTER TABLE "Identity"."UserAuditLogs"
ADD COLUMN IF NOT EXISTS "Success" BOOLEAN NOT NULL DEFAULT TRUE;

ALTER TABLE "Identity"."UserAuditLogs"
ADD COLUMN IF NOT EXISTS "FailureReason" VARCHAR(500) NULL;

-- Copy CreatedAt to Timestamp if Timestamp is default
UPDATE "Identity"."UserAuditLogs" 
SET "Timestamp" = "CreatedAt" 
WHERE "Timestamp" = CURRENT_TIMESTAMP OR "Timestamp" IS NULL;

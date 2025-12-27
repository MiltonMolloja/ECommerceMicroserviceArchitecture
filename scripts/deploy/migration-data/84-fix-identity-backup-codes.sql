-- Fix UserBackupCodes table

-- Rename Code to CodeHash
ALTER TABLE "Identity"."UserBackupCodes"
RENAME COLUMN "Code" TO "CodeHash";

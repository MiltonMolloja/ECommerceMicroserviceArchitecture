-- Fix Discriminator values in Identity tables

-- Ensure all AspNetRoles have Discriminator value
UPDATE "Identity"."AspNetRoles" 
SET "Discriminator" = 'IdentityRole' 
WHERE "Discriminator" IS NULL OR "Discriminator" = '';

-- Ensure all AspNetUsers have Discriminator value  
UPDATE "Identity"."AspNetUsers" 
SET "Discriminator" = 'ApplicationUser' 
WHERE "Discriminator" IS NULL OR "Discriminator" = '';

-- Verify the data
SELECT "Id", "Name", "Discriminator" FROM "Identity"."AspNetRoles";

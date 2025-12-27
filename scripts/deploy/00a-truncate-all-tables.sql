-- ============================================
-- Truncate all tables for fresh import
-- ============================================

-- Disable triggers temporarily
SET session_replication_role = 'replica';

-- Truncate tables in reverse dependency order
TRUNCATE TABLE "Notification"."NotificationPreferences" CASCADE;
TRUNCATE TABLE "Notification"."Notifications" CASCADE;
TRUNCATE TABLE "Notification"."NotificationTemplates" CASCADE;

TRUNCATE TABLE "Payment"."PaymentTransactions" CASCADE;
TRUNCATE TABLE "Payment"."PaymentDetails" CASCADE;
TRUNCATE TABLE "Payment"."Payments" CASCADE;

TRUNCATE TABLE "Cart"."CartItems" CASCADE;
TRUNCATE TABLE "Cart"."Carts" CASCADE;

TRUNCATE TABLE "Order"."OrderDetails" CASCADE;
TRUNCATE TABLE "Order"."Orders" CASCADE;
TRUNCATE TABLE "Order"."OrderStatuses" CASCADE;
TRUNCATE TABLE "Order"."PaymentTypes" CASCADE;

TRUNCATE TABLE "Customer"."ClientAddresses" CASCADE;
TRUNCATE TABLE "Customer"."Clients" CASCADE;

TRUNCATE TABLE "Catalog"."ProductAttributeValues" CASCADE;
TRUNCATE TABLE "Catalog"."AttributeValues" CASCADE;
TRUNCATE TABLE "Catalog"."ProductAttributes" CASCADE;
TRUNCATE TABLE "Catalog"."ProductReviews" CASCADE;
TRUNCATE TABLE "Catalog"."ProductRatings" CASCADE;
TRUNCATE TABLE "Catalog"."ProductInStock" CASCADE;
TRUNCATE TABLE "Catalog"."ProductCategories" CASCADE;
TRUNCATE TABLE "Catalog"."Products" CASCADE;
TRUNCATE TABLE "Catalog"."Categories" CASCADE;
TRUNCATE TABLE "Catalog"."Brands" CASCADE;
TRUNCATE TABLE "Catalog"."Banners" CASCADE;

TRUNCATE TABLE "Identity"."UserAuditLogs" CASCADE;
TRUNCATE TABLE "Identity"."UserBackupCodes" CASCADE;
TRUNCATE TABLE "Identity"."RefreshTokens" CASCADE;
TRUNCATE TABLE "Identity"."AspNetUserTokens" CASCADE;
TRUNCATE TABLE "Identity"."AspNetUserLogins" CASCADE;
TRUNCATE TABLE "Identity"."AspNetUserClaims" CASCADE;
TRUNCATE TABLE "Identity"."AspNetRoleClaims" CASCADE;
TRUNCATE TABLE "Identity"."AspNetUserRoles" CASCADE;
TRUNCATE TABLE "Identity"."AspNetUsers" CASCADE;
TRUNCATE TABLE "Identity"."AspNetRoles" CASCADE;

-- Re-enable triggers
SET session_replication_role = 'origin';

DO $$
BEGIN
    RAISE NOTICE '============================================';
    RAISE NOTICE 'All tables truncated successfully!';
    RAISE NOTICE 'Ready for fresh data import';
    RAISE NOTICE '============================================';
END $$;

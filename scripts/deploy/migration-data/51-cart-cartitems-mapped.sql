-- Data for "Cart"."CartItems"
-- Migrated from SQL Server [Cart].[CartItems]

INSERT INTO "Cart"."CartItems" ("CartItemId", "CartId", "ProductId", "ProductName", "UnitPrice", "Quantity", "AddedAt") VALUES (1, 1, 15, 'ss', 10.00, 10, '2025-10-24 14:49:53.244');
INSERT INTO "Cart"."CartItems" ("CartItemId", "CartId", "ProductId", "ProductName", "UnitPrice", "Quantity", "AddedAt") VALUES (2, 1, 17, 'ss', 10.00, 1, '2025-10-24 14:50:44.480');

SELECT setval('"Cart"."CartItems_CartItemId_seq"', 3);

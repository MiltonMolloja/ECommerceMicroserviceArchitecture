-- Fix remaining data migration
-- This script fixes data that failed to import due to schema differences

-- First, insert AttributeValues (after ProductAttributes exists)
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1, 1, 'Negro', 'Black', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (2, 1, 'Blanco', 'White', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (3, 1, 'Azul', 'Blue', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (4, 1, 'Rojo', 'Red', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (5, 1, 'Gris', 'Gray', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (6, 1, 'Plateado', 'Silver', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (7, 2, '64 GB', '64GB', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (8, 2, '128 GB', '128GB', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (9, 2, '256 GB', '256GB', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (10, 2, '512 GB', '512GB', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (11, 2, '1 TB', '1TB', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (12, 3, '4 GB', '4GB', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (13, 3, '8 GB', '8GB', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (14, 3, '16 GB', '16GB', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (15, 3, '32 GB', '32GB', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (23, 5, '38', '38', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (24, 5, '39', '39', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (25, 5, '40', '40', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (26, 5, '41', '41', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (27, 5, '42', '42', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (28, 5, '43', '43', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (29, 6, 'Intel Core i5', 'i5', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (30, 6, 'Intel Core i7', 'i7', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (31, 6, 'Intel Core i9', 'i9', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (32, 6, 'Apple M1', 'M1', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (33, 6, 'Apple M2', 'M2', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (34, 6, 'Apple M3', 'M3', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (35, 6, 'AMD Ryzen 5', 'Ryzen 5', 7) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (36, 7, 'Full HD 1920x1080', 'FHD', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (37, 7, '4K Ultra HD', '4K', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (38, 7, '8K Ultra HD', '8K', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (39, 8, 'Wi-Fi', 'WiFi', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (40, 8, 'Bluetooth', 'Bluetooth', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (41, 8, 'USB-C', 'USB-C', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (42, 8, 'Inalambrico', 'Wireless', 4) ON CONFLICT DO NOTHING;
-- AttributeValues for IDs 101-112
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1001, 101, 'Negro', 'Black', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1002, 101, 'Blanco', 'White', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1003, 101, 'Azul', 'Blue', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1004, 101, 'Rojo', 'Red', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1005, 101, 'Gris', 'Gray', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1006, 101, 'Plateado', 'Silver', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1007, 102, '64 GB', '64GB', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1008, 102, '128 GB', '128GB', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1009, 102, '256 GB', '256GB', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1010, 102, '512 GB', '512GB', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1011, 102, '1 TB', '1TB', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1012, 103, '4 GB', '4GB', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1013, 103, '8 GB', '8GB', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1014, 103, '16 GB', '16GB', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1015, 103, '32 GB', '32GB', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1023, 105, '38', '38', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1024, 105, '39', '39', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1025, 105, '40', '40', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1026, 105, '41', '41', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1027, 105, '42', '42', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1028, 105, '43', '43', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1029, 106, 'Intel Core i5', 'i5', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1030, 106, 'Intel Core i7', 'i7', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1031, 106, 'Intel Core i9', 'i9', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1032, 106, 'Apple M2', 'M2', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1033, 106, 'Apple M3', 'M3', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1034, 106, 'AMD Ryzen 5', 'Ryzen 5', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1035, 107, 'Full HD 1920x1080', 'FHD', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1036, 107, '4K Ultra HD', '4K', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1037, 107, '8K Ultra HD', '8K', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1038, 108, 'Wi-Fi', 'WiFi', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1039, 108, 'Bluetooth', 'Bluetooth', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1040, 108, 'USB', 'USB', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1041, 108, 'Inalambrico', 'Wireless', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1042, 109, '2024', '2024', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1043, 109, '2023', '2023', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1044, 109, '2022', '2022', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1045, 109, '2021', '2021', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1046, 109, '2020', '2020', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1047, 109, '2019', '2019', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1048, 109, '2018', '2018', 7) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1049, 110, 'Nuevo', 'New', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1050, 110, 'Renovado', 'Refurbished', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1051, 110, 'Usado', 'Used', 3) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1052, 111, 'Montaje en Mesa', 'Desk Mount', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1053, 111, 'Montaje en Pared', 'Wall Mount', 2) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1054, 108, 'HDMI', 'HDMI', 5) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1055, 108, 'Ethernet', 'Ethernet', 6) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1056, 107, 'HD 720p', 'HD 720p', 4) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1057, 106, 'Apple M1', 'Apple M1', 20) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1058, 106, 'Apple M4', 'Apple M4', 23) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1059, 112, 'Nuevo', 'New', 1) ON CONFLICT DO NOTHING;
INSERT INTO "Catalog"."AttributeValues" ("ValueId", "AttributeId", "ValueText", "ValueTextEnglish", "DisplayOrder") VALUES (1060, 112, 'Usado', 'Used', 2) ON CONFLICT DO NOTHING;

-- Customer.Clients (fixed schema - removed DateOfBirth and LastLoginAt columns that don't exist in PostgreSQL schema)
INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (15, '51b5c0dc-ae00-4915-8b7a-bc93c80c651e', '+1-555-0001', '+1-555-1001', 'M', NULL, 'es', 'USD', true, true, true, true, true, true, '2025-09-24 12:43:49.753', '2025-10-23 12:43:49.753') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (16, '65812A2A-2B92-4357-A48F-E3C9FD5755BE', '+1-555-0002', '+1-555-1002', 'F', NULL, 'en', 'USD', false, true, true, true, true, false, '2025-09-29 12:43:49.753', '2025-10-22 12:43:49.753') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (17, '7e562353-6610-4e36-86fa-57760d3547d6', '+1-555-0003', NULL, 'M', NULL, 'es', 'USD', true, true, true, true, false, false, '2025-10-19 12:43:49.753', '2025-10-19 12:43:49.753') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (18, 'b4174a7f-edd3-48fd-8a05-936590297cbd', '+1-555-0004', '+1-555-1004', 'F', 'https://example.com/avatars/sofia.jpg', 'es', 'USD', true, true, true, true, true, true, '2025-07-26 12:43:49.753', '2025-10-24 00:43:49.753') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (19, 'd1812ea1-de01-4f42-b5ee-6c0bfe205f23', '+1-555-0005', '+1-555-1005', 'M', NULL, 'en', 'USD', false, false, true, true, true, true, '2025-08-25 12:43:49.753', '2025-10-14 12:43:49.753') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (20, 'f7e70051-f838-41a8-8cdb-e5b06902b7cb', NULL, NULL, NULL, NULL, 'es', 'USD', false, false, false, true, false, false, '2025-10-26 22:49:13.084', '2025-10-26 22:49:13.084') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."Clients" ("ClientId", "UserId", "Phone", "MobilePhone", "Gender", "ProfileImageUrl", "PreferredLanguage", "PreferredCurrency", "NewsletterSubscribed", "SmsNotificationsEnabled", "EmailNotificationsEnabled", "IsActive", "IsEmailVerified", "IsPhoneVerified", "CreatedAt", "UpdatedAt") 
VALUES (21, '9795d654-b2f9-44f9-9451-6bedd0fb7052', NULL, NULL, NULL, NULL, 'es', 'USD', false, false, false, true, false, false, '2025-10-26 23:12:54.585', '2025-10-26 23:12:54.585') ON CONFLICT DO NOTHING;

-- Customer.ClientAddresses (fixed schema - removed columns that don't exist)
INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (1, 15, 'Shipping', 'Juan Perez', '+1-555-0001', '123 Main Street', 'Apt 4B', 'New York', 'NY', '10001', 'USA', true, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (2, 15, 'Billing', 'Juan Perez', '+1-555-0001', '456 Business Ave', 'Suite 200', 'New York', 'NY', '10002', 'USA', false, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (3, 16, 'Both', 'Mary Johnson', '+1-555-0002', '789 Oak Street', NULL, 'Los Angeles', 'CA', '90001', 'USA', true, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (4, 16, 'Shipping', 'Mary Johnson', '+1-555-1002', '321 Corporate Blvd', 'Floor 5', 'Los Angeles', 'CA', '90002', 'USA', false, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (5, 17, 'Shipping', 'Carlos Garcia', '+1-555-0003', '555 Elm Street', NULL, 'Chicago', 'IL', '60601', 'USA', true, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (6, 18, 'Shipping', 'Sofia Martinez', '+1-555-0004', '888 Maple Drive', 'House', 'Miami', 'FL', '33101', 'USA', true, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (7, 18, 'Billing', 'Sofia Martinez', '+1-555-0004', '999 Finance Street', 'Suite 1500', 'Miami', 'FL', '33102', 'USA', false, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (8, 18, 'Shipping', 'Sofia Martinez', '+1-555-0004', '111 Beach Road', NULL, 'Key West', 'FL', '33040', 'USA', false, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

INSERT INTO "Customer"."ClientAddresses" ("AddressId", "ClientId", "AddressType", "RecipientName", "Phone", "AddressLine1", "AddressLine2", "City", "State", "PostalCode", "Country", "IsDefault", "CreatedAt", "UpdatedAt") 
VALUES (9, 19, 'Both', 'David Smith', '+1-555-0005', '777 Pine Street', 'Unit 8C', 'Seattle', 'WA', '98101', 'USA', true, '2025-10-24 12:43:49.760', '2025-10-24 12:43:49.760') ON CONFLICT DO NOTHING;

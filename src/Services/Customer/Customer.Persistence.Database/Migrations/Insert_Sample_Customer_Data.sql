-- =====================================================
-- Script: Insert Sample Customer Data
-- Description: Inserta 5 clientes de ejemplo con direcciones
-- Schema: Customer
-- Date: 2025-10-24
-- =====================================================

USE ECommerceDb;
GO

-- Verificar que las tablas estén vacías antes de insertar
IF EXISTS (SELECT 1 FROM Customer.Clients)
BEGIN
    PRINT 'WARNING: Customer.Clients table already contains data. Skipping insert.';
    RETURN;
END
GO

-- Insertar 5 clientes de ejemplo
PRINT 'Inserting sample clients...';

INSERT INTO Customer.Clients
    (UserId, FirstName, LastName, Email, Phone, MobilePhone,
     DateOfBirth, Gender, ProfileImageUrl,
     PreferredLanguage, PreferredCurrency,
     NewsletterSubscribed, SmsNotificationsEnabled, EmailNotificationsEnabled,
     IsActive, IsEmailVerified, IsPhoneVerified,
     CreatedAt, UpdatedAt, LastLoginAt)
VALUES
    -- Cliente 1: Usuario completo verificado
    (NULL, 'Juan', 'Pérez', 'juan.perez@example.com', '+1-555-0001', '+1-555-1001',
     '1990-05-15', 'M', NULL,
     'es', 'USD',
     1, 1, 1,
     1, 1, 1,
     DATEADD(day, -30, GETUTCDATE()), DATEADD(day, -1, GETUTCDATE()), DATEADD(hour, -2, GETUTCDATE())),

    -- Cliente 2: Usuario en inglés
    (NULL, 'Mary', 'Johnson', 'mary.johnson@example.com', '+1-555-0002', '+1-555-1002',
     '1985-08-22', 'F', NULL,
     'en', 'USD',
     0, 1, 1,
     1, 1, 0,
     DATEADD(day, -25, GETUTCDATE()), DATEADD(day, -2, GETUTCDATE()), DATEADD(hour, -5, GETUTCDATE())),

    -- Cliente 3: Usuario nuevo sin verificar
    (NULL, 'Carlos', 'García', 'carlos.garcia@example.com', '+1-555-0003', NULL,
     '1995-12-10', 'M', NULL,
     'es', 'USD',
     1, 1, 1,
     1, 0, 0,
     DATEADD(day, -5, GETUTCDATE()), DATEADD(day, -5, GETUTCDATE()), NULL),

    -- Cliente 4: Usuario premium
    (NULL, 'Sofia', 'Martinez', 'sofia.martinez@example.com', '+1-555-0004', '+1-555-1004',
     '1988-03-18', 'F', 'https://example.com/avatars/sofia.jpg',
     'es', 'USD',
     1, 1, 1,
     1, 1, 1,
     DATEADD(day, -90, GETUTCDATE()), DATEADD(hour, -12, GETUTCDATE()), DATEADD(hour, -1, GETUTCDATE())),

    -- Cliente 5: Usuario bilingüe
    (NULL, 'David', 'Smith', 'david.smith@example.com', '+1-555-0005', '+1-555-1005',
     '1992-11-05', 'M', NULL,
     'en', 'USD',
     0, 0, 1,
     1, 1, 1,
     DATEADD(day, -60, GETUTCDATE()), DATEADD(day, -10, GETUTCDATE()), DATEADD(day, -3, GETUTCDATE()));

PRINT 'Sample clients inserted successfully.';
GO

-- Insertar direcciones de ejemplo
PRINT 'Inserting sample addresses...';

DECLARE @ClientId1 INT = (SELECT ClientId FROM Customer.Clients WHERE Email = 'juan.perez@example.com');
DECLARE @ClientId2 INT = (SELECT ClientId FROM Customer.Clients WHERE Email = 'mary.johnson@example.com');
DECLARE @ClientId3 INT = (SELECT ClientId FROM Customer.Clients WHERE Email = 'carlos.garcia@example.com');
DECLARE @ClientId4 INT = (SELECT ClientId FROM Customer.Clients WHERE Email = 'sofia.martinez@example.com');
DECLARE @ClientId5 INT = (SELECT ClientId FROM Customer.Clients WHERE Email = 'david.smith@example.com');

INSERT INTO Customer.ClientAddresses
    (ClientId, AddressType, AddressName, RecipientName, RecipientPhone,
     AddressLine1, AddressLine2, City, State, PostalCode, Country,
     IsDefaultShipping, IsDefaultBilling, IsActive, CreatedAt, UpdatedAt)
VALUES
    -- Direcciones de Juan Pérez
    (@ClientId1, 'Shipping', 'Casa', 'Juan Pérez', '+1-555-0001',
     '123 Main Street', 'Apt 4B', 'New York', 'NY', '10001', 'USA',
     1, 0, 1, GETUTCDATE(), GETUTCDATE()),

    (@ClientId1, 'Billing', 'Oficina', 'Juan Pérez', '+1-555-0001',
     '456 Business Ave', 'Suite 200', 'New York', 'NY', '10002', 'USA',
     0, 1, 1, GETUTCDATE(), GETUTCDATE()),

    -- Direcciones de Mary Johnson
    (@ClientId2, 'Both', 'Home', 'Mary Johnson', '+1-555-0002',
     '789 Oak Street', NULL, 'Los Angeles', 'CA', '90001', 'USA',
     1, 1, 1, GETUTCDATE(), GETUTCDATE()),

    (@ClientId2, 'Shipping', 'Work', 'Mary Johnson', '+1-555-1002',
     '321 Corporate Blvd', 'Floor 5', 'Los Angeles', 'CA', '90002', 'USA',
     0, 0, 1, GETUTCDATE(), GETUTCDATE()),

    -- Dirección de Carlos García
    (@ClientId3, 'Shipping', 'Casa', 'Carlos García', '+1-555-0003',
     '555 Elm Street', NULL, 'Chicago', 'IL', '60601', 'USA',
     1, 1, 1, GETUTCDATE(), GETUTCDATE()),

    -- Direcciones de Sofia Martinez
    (@ClientId4, 'Shipping', 'Casa Principal', 'Sofia Martinez', '+1-555-0004',
     '888 Maple Drive', 'House', 'Miami', 'FL', '33101', 'USA',
     1, 0, 1, GETUTCDATE(), GETUTCDATE()),

    (@ClientId4, 'Billing', 'Oficina', 'Sofia Martinez', '+1-555-0004',
     '999 Finance Street', 'Suite 1500', 'Miami', 'FL', '33102', 'USA',
     0, 1, 1, GETUTCDATE(), GETUTCDATE()),

    (@ClientId4, 'Shipping', 'Casa de Playa', 'Sofia Martinez', '+1-555-0004',
     '111 Beach Road', NULL, 'Key West', 'FL', '33040', 'USA',
     0, 0, 1, GETUTCDATE(), GETUTCDATE()),

    -- Direcciones de David Smith
    (@ClientId5, 'Both', 'Home Address', 'David Smith', '+1-555-0005',
     '777 Pine Street', 'Unit 8C', 'Seattle', 'WA', '98101', 'USA',
     1, 1, 1, GETUTCDATE(), GETUTCDATE());

PRINT 'Sample addresses inserted successfully.';
GO

-- Verificar los datos insertados
PRINT '========================================';
PRINT 'Summary of inserted data:';
PRINT '========================================';

SELECT
    COUNT(*) AS TotalClients,
    SUM(CASE WHEN IsEmailVerified = 1 THEN 1 ELSE 0 END) AS VerifiedClients,
    SUM(CASE WHEN PreferredLanguage = 'es' THEN 1 ELSE 0 END) AS SpanishSpeaking,
    SUM(CASE WHEN PreferredLanguage = 'en' THEN 1 ELSE 0 END) AS EnglishSpeaking
FROM Customer.Clients;

SELECT
    COUNT(*) AS TotalAddresses,
    SUM(CASE WHEN IsDefaultShipping = 1 THEN 1 ELSE 0 END) AS DefaultShippingAddresses,
    SUM(CASE WHEN IsDefaultBilling = 1 THEN 1 ELSE 0 END) AS DefaultBillingAddresses
FROM Customer.ClientAddresses;

PRINT '========================================';
PRINT 'Sample data insertion completed!';
PRINT '========================================';
GO

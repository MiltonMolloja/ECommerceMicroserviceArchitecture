-- Create and populate OrderStatuses and PaymentTypes tables
-- These tables don't exist in the PostgreSQL schema but are needed for reference data

-- Drop and recreate OrderStatuses with correct structure
DROP TABLE IF EXISTS "Order"."OrderStatuses" CASCADE;
CREATE TABLE "Order"."OrderStatuses" (
    "StatusId" INTEGER PRIMARY KEY,
    "StatusName" VARCHAR(100) NOT NULL,
    "StatusNameEs" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "ColorCode" VARCHAR(50) NULL,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Drop and recreate PaymentTypes with correct structure
DROP TABLE IF EXISTS "Order"."PaymentTypes" CASCADE;
CREATE TABLE "Order"."PaymentTypes" (
    "PaymentTypeId" INTEGER PRIMARY KEY,
    "PaymentTypeName" VARCHAR(100) NOT NULL,
    "PaymentTypeNameEs" VARCHAR(100) NOT NULL,
    "Description" VARCHAR(500) NULL,
    "IconName" VARCHAR(100) NULL,
    "IsActive" BOOLEAN NOT NULL DEFAULT TRUE,
    "DisplayOrder" INTEGER NOT NULL DEFAULT 0,
    "CreatedAt" TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

-- Insert OrderStatuses data
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (0, 'AwaitingPayment', 'Esperando Pago', 'Orden creada, esperando confirmacion de pago', 'warning', 1, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (1, 'PaymentProcessing', 'Procesando Pago', 'Pago en proceso de verificacion', 'info', 2, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (2, 'PaymentFailed', 'Pago Fallido', 'El pago no pudo ser procesado', 'danger', 3, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (3, 'Paid', 'Pagado', 'Pago confirmado exitosamente', 'success', 4, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (4, 'Processing', 'Procesando', 'Orden en proceso de preparacion', 'info', 5, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (5, 'ReadyToShip', 'Listo para Enviar', 'Orden lista para ser enviada', 'primary', 6, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (6, 'Shipped', 'Enviado', 'Orden enviada al courier', 'primary', 7, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (7, 'InTransit', 'En Transito', 'Orden en camino al destino', 'primary', 8, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (8, 'OutForDelivery', 'En Reparto', 'Orden en reparto final', 'primary', 9, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (9, 'Delivered', 'Entregado', 'Orden entregada al cliente', 'success', 10, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (10, 'Cancelled', 'Cancelado', 'Orden cancelada', 'danger', 11, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (11, 'Refunded', 'Reembolsado', 'Pago reembolsado al cliente', 'warning', 12, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (12, 'PartiallyRefunded', 'Reembolso Parcial', 'Reembolso parcial realizado', 'warning', 13, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (13, 'ReturnRequested', 'Devolucion Solicitada', 'Cliente solicito devolucion', 'warning', 14, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (14, 'Returned', 'Devuelto', 'Producto devuelto', 'secondary', 15, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (15, 'OnHold', 'En Espera', 'Orden en espera por problema', 'warning', 16, true, '2025-11-26 00:28:02.296');
INSERT INTO "Order"."OrderStatuses" ("StatusId", "StatusName", "StatusNameEs", "Description", "ColorCode", "DisplayOrder", "IsActive", "CreatedAt") VALUES (16, 'PaymentDisputed', 'Pago Disputado', 'Pago en disputa', 'danger', 17, true, '2025-11-26 00:28:02.296');

-- Insert PaymentTypes data
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (0, 'CreditCard', 'Tarjeta de Credito', 'Pago con tarjeta de credito', 'credit_card', true, 1, '2025-11-26 00:28:02.300');
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (1, 'DebitCard', 'Tarjeta de Debito', 'Pago con tarjeta de debito', 'payment', true, 2, '2025-11-26 00:28:02.300');
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (2, 'MercadoPago', 'MercadoPago', 'Pago a traves de MercadoPago', 'account_balance_wallet', true, 3, '2025-11-26 00:28:02.300');
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (3, 'PayPal', 'PayPal', 'Pago a traves de PayPal', 'account_balance', true, 4, '2025-11-26 00:28:02.300');
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (4, 'BankTransfer', 'Transferencia Bancaria', 'Transferencia bancaria directa', 'account_balance', true, 5, '2025-11-26 00:28:02.300');
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (5, 'Cash', 'Efectivo', 'Pago en efectivo', 'local_atm', true, 6, '2025-11-26 00:28:02.300');
INSERT INTO "Order"."PaymentTypes" ("PaymentTypeId", "PaymentTypeName", "PaymentTypeNameEs", "Description", "IconName", "IsActive", "DisplayOrder", "CreatedAt") VALUES (99, 'Other', 'Otro', 'Otro metodo de pago', 'more_horiz', true, 7, '2025-11-26 00:28:02.300');

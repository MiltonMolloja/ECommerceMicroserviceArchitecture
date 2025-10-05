-- Insertar usuario administrador en Identity
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE KodotiCommerceDb;
GO

-- Eliminar si existe (para evitar duplicados)
DELETE FROM [Identity].[AspNetUsers] WHERE Email = 'admin@kodoti.com';
GO

-- Insertar usuario administrador
-- Password: Pa$$w0rd!
-- Hash generado para la contraseña Pa$$w0rd!
INSERT INTO [Identity].[AspNetUsers] 
    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName)
VALUES 
    (NEWID(), 
     'admin@kodoti.com', 
     'ADMIN@KODOTI.COM', 
     'admin@kodoti.com', 
     'ADMIN@KODOTI.COM', 
     1,
     'AQAAAAIAAYagAAAAELFyZ7qZxqxYmYlQjFqFa6Q0Z3RQGJvfZ6cIGNnZ3pYn2xGlX8aZaIQJvYqQ0Z3R==',
     NEWID(),
     NEWID(),
     0,
     0,
     0,
     0,
     'Admin',
     'Administrator');
GO

PRINT 'Usuario administrador insertado exitosamente';
PRINT 'Email: admin@kodoti.com';
PRINT 'Password: Pa$$w0rd!';
GO

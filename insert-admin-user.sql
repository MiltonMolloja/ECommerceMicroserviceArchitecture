-- Insertar usuario administrador en Identity
SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

USE ECommerceDb;
GO

-- Eliminar si existe (para evitar duplicados)
DELETE FROM [Identity].[AspNetUsers] WHERE Email = 'admin@gmail.com';
GO

-- Insertar usuario administrador
-- Password: Pa$$w0rd!
-- Hash generado con ASP.NET Core Identity PasswordHasher
INSERT INTO [Identity].[AspNetUsers] 
    (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount, FirstName, LastName)
VALUES 
    (NEWID(), 
     'admin@gmail.com', 
     'ADMIN@GMAIL.COM', 
     'admin@gmail.com', 
     'ADMIN@GMAIL.COM', 
     1,
     'AQAAAAEAACcQAAAAEEXGXvSiWS3bZAJR4OjR/l7TG2Rjcs3aDwYBUZ/fEhcf45P+GrZ7L0WAvdlWW5ku6A==',
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
PRINT 'Email: admin@gmail.com';
PRINT 'Password: Pa$$w0rd!';
GO

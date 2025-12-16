-- =============================================
-- Script: Agregar campo TotalSold a Products
-- Fecha: 2025-12-10
-- Descripción: Campo para rastrear ventas y ordenar por bestsellers
-- =============================================

USE ECommerceDb;
GO

SET QUOTED_IDENTIFIER ON;
GO

-- Verificar si la columna ya existe
IF NOT EXISTS (
    SELECT * FROM sys.columns 
    WHERE object_id = OBJECT_ID('[Catalog].[Products]') 
    AND name = 'TotalSold'
)
BEGIN
    PRINT 'Agregando columna TotalSold a [Catalog].[Products]...';
    
    ALTER TABLE [Catalog].[Products]
    ADD [TotalSold] INT NOT NULL DEFAULT 0;
    
    PRINT 'Columna TotalSold agregada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La columna TotalSold ya existe en [Catalog].[Products].';
END
GO

-- Crear índice para ordenamiento por ventas
IF NOT EXISTS (
    SELECT * FROM sys.indexes 
    WHERE name = 'IX_Products_TotalSold' 
    AND object_id = OBJECT_ID('[Catalog].[Products]')
)
BEGIN
    PRINT 'Creando índice IX_Products_TotalSold...';
    
    CREATE NONCLUSTERED INDEX [IX_Products_TotalSold]
    ON [Catalog].[Products] ([TotalSold] DESC)
    WHERE [IsActive] = 1;
    
    PRINT 'Índice IX_Products_TotalSold creado exitosamente.';
END
ELSE
BEGIN
    PRINT 'El índice IX_Products_TotalSold ya existe.';
END
GO

-- Actualizar TotalSold basado en órdenes existentes (si aplica)
PRINT 'Actualizando TotalSold basado en órdenes completadas...';

-- Verificar si existe la tabla Orders
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Orders' AND schema_id = SCHEMA_ID('Order'))
BEGIN
    UPDATE p
    SET p.TotalSold = ISNULL(sales.TotalQuantity, 0)
    FROM [Catalog].[Products] p
    LEFT JOIN (
        SELECT 
            oi.ProductId, 
            SUM(oi.Quantity) as TotalQuantity
        FROM [Order].[OrderItems] oi
        INNER JOIN [Order].[Orders] o ON oi.OrderId = o.OrderId
        WHERE o.Status IN ('Completed', 'Shipped', 'Delivered')
        GROUP BY oi.ProductId
    ) sales ON p.ProductId = sales.ProductId;
    
    PRINT 'TotalSold actualizado para productos con órdenes existentes.';
    
    -- Mostrar estadísticas
    SELECT 
        COUNT(*) as TotalProducts,
        SUM(CASE WHEN TotalSold > 0 THEN 1 ELSE 0 END) as ProductsWithSales,
        MAX(TotalSold) as MaxSold,
        AVG(TotalSold) as AvgSold
    FROM [Catalog].[Products];
END
ELSE
BEGIN
    PRINT 'Tabla Orders no encontrada. TotalSold inicializado en 0 para todos los productos.';
END
GO

-- Verificar resultados
PRINT 'Verificando productos con más ventas...';
SELECT TOP 10
    ProductId,
    NameSpanish,
    TotalSold,
    Price,
    IsFeatured
FROM [Catalog].[Products]
ORDER BY TotalSold DESC;
GO

PRINT 'Script completado exitosamente.';
GO

-- =============================================
-- Script para asignar atributos a productos de TV
-- Fecha: 2025-12-03
-- Descripción: Asigna atributos de Resolución, Año, Condición, etc. a productos de TV
-- =============================================

USE ECommerceDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '========================================';
PRINT 'ASIGNANDO ATRIBUTOS A PRODUCTOS DE TV';
PRINT '========================================';
PRINT '';

-- Variables para IDs de atributos
DECLARE @ResolutionAttrId INT;
DECLARE @YearAttrId INT;
DECLARE @ConditionAttrId INT;
DECLARE @MountTypeAttrId INT;
DECLARE @ConnectivityAttrId INT;

-- Obtener IDs de atributos
SELECT @ResolutionAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Resolution';
SELECT @YearAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Model Year';
SELECT @ConditionAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Condition';
SELECT @MountTypeAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Mount Type';
SELECT @ConnectivityAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Connectivity';

PRINT 'IDs de atributos:';
PRINT '  Resolution: ' + ISNULL(CAST(@ResolutionAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Model Year: ' + ISNULL(CAST(@YearAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Condition: ' + ISNULL(CAST(@ConditionAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Mount Type: ' + ISNULL(CAST(@MountTypeAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Connectivity: ' + ISNULL(CAST(@ConnectivityAttrId AS VARCHAR), 'NOT FOUND');
PRINT '';

-- =============================================
-- 1. ASIGNAR RESOLUCIÓN A TVs
-- =============================================
PRINT '1. Asignando Resolución a productos de TV...';

IF @ResolutionAttrId IS NOT NULL
BEGIN
    -- Obtener IDs de valores de resolución
    DECLARE @Resolution8K INT, @Resolution4K INT, @Resolution1080p INT, @Resolution720p INT;
    
    SELECT @Resolution8K = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ResolutionAttrId AND ValueTextEnglish LIKE '%8K%';
    
    SELECT @Resolution4K = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ResolutionAttrId AND ValueTextEnglish LIKE '%4K%';
    
    SELECT @Resolution1080p = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ResolutionAttrId AND ValueTextEnglish LIKE '%1080%';
    
    SELECT @Resolution720p = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ResolutionAttrId AND ValueTextEnglish LIKE '%720%';

    -- Asignar 4K a TVs que contengan "4K" en el nombre
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ResolutionAttrId, @Resolution4K
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%4K%' OR p.NameEnglish LIKE '%4K%' OR p.NameSpanish LIKE '%UHD%' OR p.NameEnglish LIKE '%UHD%' OR p.NameSpanish LIKE '%Ultra HD%' OR p.NameEnglish LIKE '%Ultra HD%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ResolutionAttrId
    );
    
    PRINT '  - Asignado 4K a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar 8K a TVs que contengan "8K" en el nombre
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ResolutionAttrId, @Resolution8K
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%8K%' OR p.NameEnglish LIKE '%8K%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ResolutionAttrId
    );
    
    PRINT '  - Asignado 8K a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar 1080p a TVs que contengan "1080" o "Full HD" en el nombre
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ResolutionAttrId, @Resolution1080p
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%1080%' OR p.NameEnglish LIKE '%1080%' OR p.NameSpanish LIKE '%Full HD%' OR p.NameEnglish LIKE '%Full HD%' OR p.NameSpanish LIKE '%FHD%' OR p.NameEnglish LIKE '%FHD%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ResolutionAttrId
    );
    
    PRINT '  - Asignado 1080p a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar 720p a TVs que contengan "720" en el nombre
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ResolutionAttrId, @Resolution720p
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%720%' OR p.NameEnglish LIKE '%720%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ResolutionAttrId
    );
    
    PRINT '  - Asignado 720p a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
END
ELSE
BEGIN
    PRINT '  ⚠️ Atributo Resolución no encontrado';
END
PRINT '';

-- =============================================
-- 2. ASIGNAR AÑO DEL MODELO
-- =============================================
PRINT '2. Asignando Año del Modelo a productos de TV...';

IF @YearAttrId IS NOT NULL
BEGIN
    -- Asignar años basándose en el nombre del producto
    DECLARE @Year INT = 2025;
    DECLARE @YearValueId INT;
    
    WHILE @Year >= 2018
    BEGIN
        SELECT @YearValueId = ValueId FROM Catalog.AttributeValues 
        WHERE AttributeId = @YearAttrId AND ValueText = CAST(@Year AS VARCHAR);
        
        IF @YearValueId IS NOT NULL
        BEGIN
            INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
            SELECT DISTINCT p.ProductId, @YearAttrId, @YearValueId
            FROM Catalog.Products p
            WHERE (p.NameSpanish LIKE '%' + CAST(@Year AS VARCHAR) + '%' OR p.NameEnglish LIKE '%' + CAST(@Year AS VARCHAR) + '%')
            AND (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
            AND NOT EXISTS (
                SELECT 1 FROM Catalog.ProductAttributeValues pav 
                WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @YearAttrId
            );
            
            PRINT '  - Asignado año ' + CAST(@Year AS VARCHAR) + ' a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
        END
        
        SET @Year = @Year - 1;
    END
END
ELSE
BEGIN
    PRINT '  ⚠️ Atributo Año del Modelo no encontrado';
END
PRINT '';

-- =============================================
-- 3. ASIGNAR CONDICIÓN (por defecto "Nuevo")
-- =============================================
PRINT '3. Asignando Condición a productos de TV...';

IF @ConditionAttrId IS NOT NULL
BEGIN
    DECLARE @NewConditionId INT, @RefurbishedId INT, @UsedId INT;
    
    SELECT @NewConditionId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConditionAttrId AND ValueTextEnglish = 'New';
    
    SELECT @RefurbishedId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConditionAttrId AND ValueTextEnglish = 'Refurbished';
    
    SELECT @UsedId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConditionAttrId AND ValueTextEnglish = 'Used';

    -- Asignar "Renovado" a productos que lo mencionen
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConditionAttrId, @RefurbishedId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%Renovado%' OR p.NameEnglish LIKE '%Renovado%' OR p.NameSpanish LIKE '%Refurbished%' OR p.NameEnglish LIKE '%Refurbished%' OR p.NameSpanish LIKE '%Renewed%' OR p.NameEnglish LIKE '%Renewed%')
    AND (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConditionAttrId
    );
    
    PRINT '  - Asignado Renovado a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar "Nuevo" al resto de TVs
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConditionAttrId, @NewConditionId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConditionAttrId
    );
    
    PRINT '  - Asignado Nuevo a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
END
ELSE
BEGIN
    PRINT '  ⚠️ Atributo Condición no encontrado';
END
PRINT '';

-- =============================================
-- 4. ASIGNAR TIPO DE MONTAJE
-- =============================================
PRINT '4. Asignando Tipo de Montaje a productos de TV...';

IF @MountTypeAttrId IS NOT NULL
BEGIN
    DECLARE @DeskMountId INT, @WallMountId INT;
    
    SELECT @DeskMountId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @MountTypeAttrId AND ValueTextEnglish = 'Desk Mount';
    
    SELECT @WallMountId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @MountTypeAttrId AND ValueTextEnglish = 'Wall Mount';

    -- Asignar ambos tipos a todos los TVs (la mayoría soportan ambos)
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @MountTypeAttrId, @DeskMountId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @MountTypeAttrId AND pav.ValueId = @DeskMountId
    );
    
    PRINT '  - Asignado Montaje en Mesa a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @MountTypeAttrId, @WallMountId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @MountTypeAttrId AND pav.ValueId = @WallMountId
    );
    
    PRINT '  - Asignado Montaje en Pared a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
END
ELSE
BEGIN
    PRINT '  ⚠️ Atributo Tipo de Montaje no encontrado';
END
PRINT '';

-- =============================================
-- 5. ASIGNAR CONECTIVIDAD
-- =============================================
PRINT '5. Asignando Conectividad a productos de TV...';

IF @ConnectivityAttrId IS NOT NULL
BEGIN
    DECLARE @HDMIId INT, @WiFiId INT, @BluetoothId INT, @USBId INT, @EthernetId INT;
    
    SELECT @HDMIId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'HDMI';
    
    SELECT @WiFiId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'Wi-Fi';
    
    SELECT @BluetoothId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'Bluetooth';
    
    SELECT @USBId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'USB';
    
    SELECT @EthernetId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'Ethernet';

    -- Asignar HDMI a todos los TVs (estándar)
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @HDMIId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @HDMIId
    );
    
    PRINT '  - Asignado HDMI a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar Wi-Fi a Smart TVs
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @WiFiId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%Smart%' OR p.NameEnglish LIKE '%Smart%' OR p.NameSpanish LIKE '%WiFi%' OR p.NameEnglish LIKE '%WiFi%' OR p.NameSpanish LIKE '%Wi-Fi%' OR p.NameEnglish LIKE '%Wi-Fi%')
    AND (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @WiFiId
    );
    
    PRINT '  - Asignado Wi-Fi a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar Bluetooth a Smart TVs
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @BluetoothId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%Smart%' OR p.NameEnglish LIKE '%Smart%' OR p.NameSpanish LIKE '%Bluetooth%' OR p.NameEnglish LIKE '%Bluetooth%')
    AND (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @BluetoothId
    );
    
    PRINT '  - Asignado Bluetooth a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar USB a la mayoría de TVs
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @USBId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @USBId
    );
    
    PRINT '  - Asignado USB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar Ethernet a Smart TVs
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @EthernetId
    FROM Catalog.Products p
    WHERE (p.NameSpanish LIKE '%Smart%' OR p.NameEnglish LIKE '%Smart%')
    AND (p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%' OR p.NameSpanish LIKE '%television%' OR p.NameEnglish LIKE '%television%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @EthernetId
    );
    
    PRINT '  - Asignado Ethernet a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
END
ELSE
BEGIN
    PRINT '  ⚠️ Atributo Conectividad no encontrado';
END
PRINT '';

-- =============================================
-- 6. VERIFICACIÓN FINAL
-- =============================================
PRINT '========================================';
PRINT 'VERIFICACIÓN FINAL';
PRINT '========================================';

SELECT 
    pa.AttributeName,
    COUNT(DISTINCT pav.ProductId) AS ProductsWithAttribute
FROM Catalog.ProductAttributes pa
LEFT JOIN Catalog.ProductAttributeValues pav ON pa.AttributeId = pav.AttributeId
WHERE pa.IsFilterable = 1
GROUP BY pa.AttributeName
ORDER BY pa.AttributeName;

PRINT '';
PRINT '========================================';
PRINT 'ASIGNACIÓN COMPLETADA';
PRINT '========================================';
GO

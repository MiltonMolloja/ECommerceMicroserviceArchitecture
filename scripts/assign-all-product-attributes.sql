-- =============================================
-- Script para asignar atributos a TODOS los productos
-- Fecha: 2025-12-03
-- Descripción: Asigna atributos inteligentes basándose en el nombre del producto
-- =============================================

USE ECommerceDb;
GO

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

PRINT '========================================';
PRINT 'ASIGNANDO ATRIBUTOS A TODOS LOS PRODUCTOS';
PRINT '========================================';
PRINT '';

-- Variables para IDs de atributos
DECLARE @ResolutionAttrId INT;
DECLARE @YearAttrId INT;
DECLARE @ConditionAttrId INT;
DECLARE @MountTypeAttrId INT;
DECLARE @ConnectivityAttrId INT;
DECLARE @RAMAttrId INT;
DECLARE @StorageAttrId INT;
DECLARE @ProcessorAttrId INT;
DECLARE @ColorAttrId INT;
DECLARE @ScreenSizeAttrId INT;

-- Obtener IDs de atributos
SELECT @ResolutionAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Resolution' AND AttributeType = 'Select';
SELECT @YearAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Model Year';
SELECT @ConditionAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Condition';
SELECT @MountTypeAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Mount Type';
SELECT @ConnectivityAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Connectivity' AND AttributeType = 'MultiSelect';
SELECT @RAMAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'RAM' AND AttributeType = 'Select';
SELECT @StorageAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Storage' AND AttributeType = 'Select';
SELECT @ProcessorAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Processor' AND AttributeType = 'Select';
SELECT @ColorAttrId = AttributeId FROM Catalog.ProductAttributes WHERE AttributeNameEnglish = 'Color' AND AttributeType = 'Select';

PRINT 'IDs de atributos encontrados:';
PRINT '  Resolution: ' + ISNULL(CAST(@ResolutionAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Model Year: ' + ISNULL(CAST(@YearAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Condition: ' + ISNULL(CAST(@ConditionAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Connectivity: ' + ISNULL(CAST(@ConnectivityAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  RAM: ' + ISNULL(CAST(@RAMAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Storage: ' + ISNULL(CAST(@StorageAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Processor: ' + ISNULL(CAST(@ProcessorAttrId AS VARCHAR), 'NOT FOUND');
PRINT '  Color: ' + ISNULL(CAST(@ColorAttrId AS VARCHAR), 'NOT FOUND');
PRINT '';

-- =============================================
-- 1. ASIGNAR CONDICIÓN A TODOS LOS PRODUCTOS
-- =============================================
PRINT '1. Asignando Condición a todos los productos...';

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
    WHERE (p.NameSpanish LIKE '%Renovado%' OR p.NameEnglish LIKE '%Renovado%' 
           OR p.NameSpanish LIKE '%Refurbished%' OR p.NameEnglish LIKE '%Refurbished%' 
           OR p.NameSpanish LIKE '%Renewed%' OR p.NameEnglish LIKE '%Renewed%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConditionAttrId
    );
    
    PRINT '  - Asignado Renovado a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';

    -- Asignar "Nuevo" al resto
    INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
    SELECT DISTINCT p.ProductId, @ConditionAttrId, @NewConditionId
    FROM Catalog.Products p
    WHERE NOT EXISTS (
        SELECT 1 FROM Catalog.ProductAttributeValues pav 
        WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConditionAttrId
    );
    
    PRINT '  - Asignado Nuevo a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
END
PRINT '';

-- =============================================
-- 2. ASIGNAR AÑO DEL MODELO
-- =============================================
PRINT '2. Asignando Año del Modelo...';

IF @YearAttrId IS NOT NULL
BEGIN
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
            WHERE (p.NameSpanish LIKE '%' + CAST(@Year AS VARCHAR) + '%' 
                   OR p.NameEnglish LIKE '%' + CAST(@Year AS VARCHAR) + '%'
                   OR p.NameSpanish LIKE '% ' + CAST(@Year AS VARCHAR) + ' %'
                   OR p.NameEnglish LIKE '% ' + CAST(@Year AS VARCHAR) + ' %')
            AND NOT EXISTS (
                SELECT 1 FROM Catalog.ProductAttributeValues pav 
                WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @YearAttrId
            );
            
            IF @@ROWCOUNT > 0
                PRINT '  - Asignado año ' + CAST(@Year AS VARCHAR) + ' a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
        END
        
        SET @Year = @Year - 1;
    END
END
PRINT '';

-- =============================================
-- 3. ASIGNAR MEMORIA RAM (Laptops principalmente)
-- =============================================
PRINT '3. Asignando Memoria RAM...';

IF @RAMAttrId IS NOT NULL
BEGIN
    DECLARE @RAM4GB INT, @RAM8GB INT, @RAM16GB INT, @RAM32GB INT;
    
    SELECT @RAM4GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @RAMAttrId AND (ValueText LIKE '%4GB%' OR ValueText LIKE '%4 GB%');
    
    SELECT @RAM8GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @RAMAttrId AND (ValueText LIKE '%8GB%' OR ValueText LIKE '%8 GB%');
    
    SELECT @RAM16GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @RAMAttrId AND (ValueText LIKE '%16GB%' OR ValueText LIKE '%16 GB%');
    
    SELECT @RAM32GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @RAMAttrId AND (ValueText LIKE '%32GB%' OR ValueText LIKE '%32 GB%');

    -- 32GB RAM
    IF @RAM32GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @RAMAttrId, @RAM32GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%32GB%' OR p.NameEnglish LIKE '%32GB%' 
               OR p.NameSpanish LIKE '%32 GB%' OR p.NameEnglish LIKE '%32 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @RAMAttrId
        );
        PRINT '  - Asignado 32GB RAM a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 16GB RAM
    IF @RAM16GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @RAMAttrId, @RAM16GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%16GB%' OR p.NameEnglish LIKE '%16GB%' 
               OR p.NameSpanish LIKE '%16 GB%' OR p.NameEnglish LIKE '%16 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @RAMAttrId
        );
        PRINT '  - Asignado 16GB RAM a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 8GB RAM
    IF @RAM8GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @RAMAttrId, @RAM8GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%8GB%' OR p.NameEnglish LIKE '%8GB%' 
               OR p.NameSpanish LIKE '%8 GB%' OR p.NameEnglish LIKE '%8 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @RAMAttrId
        );
        PRINT '  - Asignado 8GB RAM a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 4GB RAM
    IF @RAM4GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @RAMAttrId, @RAM4GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%4GB%' OR p.NameEnglish LIKE '%4GB%' 
               OR p.NameSpanish LIKE '%4 GB%' OR p.NameEnglish LIKE '%4 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @RAMAttrId
        );
        PRINT '  - Asignado 4GB RAM a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END
END
PRINT '';

-- =============================================
-- 4. ASIGNAR ALMACENAMIENTO (Storage)
-- =============================================
PRINT '4. Asignando Almacenamiento...';

IF @StorageAttrId IS NOT NULL
BEGIN
    DECLARE @Storage128GB INT, @Storage256GB INT, @Storage512GB INT, @Storage1TB INT, @Storage2TB INT;
    
    SELECT @Storage128GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @StorageAttrId AND (ValueText LIKE '%128GB%' OR ValueText LIKE '%128 GB%');
    
    SELECT @Storage256GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @StorageAttrId AND (ValueText LIKE '%256GB%' OR ValueText LIKE '%256 GB%');
    
    SELECT @Storage512GB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @StorageAttrId AND (ValueText LIKE '%512GB%' OR ValueText LIKE '%512 GB%');
    
    SELECT @Storage1TB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @StorageAttrId AND (ValueText LIKE '%1TB%' OR ValueText LIKE '%1 TB%');
    
    SELECT @Storage2TB = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @StorageAttrId AND (ValueText LIKE '%2TB%' OR ValueText LIKE '%2 TB%');

    -- 2TB
    IF @Storage2TB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @StorageAttrId, @Storage2TB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%2TB%' OR p.NameEnglish LIKE '%2TB%' 
               OR p.NameSpanish LIKE '%2 TB%' OR p.NameEnglish LIKE '%2 TB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @StorageAttrId
        );
        PRINT '  - Asignado 2TB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 1TB
    IF @Storage1TB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @StorageAttrId, @Storage1TB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%1TB%' OR p.NameEnglish LIKE '%1TB%' 
               OR p.NameSpanish LIKE '%1 TB%' OR p.NameEnglish LIKE '%1 TB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @StorageAttrId
        );
        PRINT '  - Asignado 1TB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 512GB
    IF @Storage512GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @StorageAttrId, @Storage512GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%512GB%' OR p.NameEnglish LIKE '%512GB%' 
               OR p.NameSpanish LIKE '%512 GB%' OR p.NameEnglish LIKE '%512 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @StorageAttrId
        );
        PRINT '  - Asignado 512GB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 256GB
    IF @Storage256GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @StorageAttrId, @Storage256GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%256GB%' OR p.NameEnglish LIKE '%256GB%' 
               OR p.NameSpanish LIKE '%256 GB%' OR p.NameEnglish LIKE '%256 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @StorageAttrId
        );
        PRINT '  - Asignado 256GB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- 128GB
    IF @Storage128GB IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @StorageAttrId, @Storage128GB
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%128GB%' OR p.NameEnglish LIKE '%128GB%' 
               OR p.NameSpanish LIKE '%128 GB%' OR p.NameEnglish LIKE '%128 GB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @StorageAttrId
        );
        PRINT '  - Asignado 128GB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END
END
PRINT '';

-- =============================================
-- 5. ASIGNAR PROCESADOR (Laptops)
-- =============================================
PRINT '5. Asignando Procesador...';

IF @ProcessorAttrId IS NOT NULL
BEGIN
    DECLARE @IntelI3 INT, @IntelI5 INT, @IntelI7 INT, @IntelI9 INT, @AMDRYZEN3 INT, @AMDRYZEN5 INT, @AMDRYZEN7 INT;
    DECLARE @AppleM1 INT, @AppleM2 INT, @AppleM3 INT, @AppleM4 INT;
    
    -- Intel Processors
    SELECT @IntelI3 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Intel i3%' OR ValueText LIKE '%Core i3%');
    
    SELECT @IntelI5 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Intel i5%' OR ValueText LIKE '%Core i5%');
    
    SELECT @IntelI7 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Intel i7%' OR ValueText LIKE '%Core i7%');
    
    SELECT @IntelI9 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Intel i9%' OR ValueText LIKE '%Core i9%');
    
    -- AMD Processors
    SELECT @AMDRYZEN3 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Ryzen 3%' OR ValueText LIKE '%AMD Ryzen 3%');
    
    SELECT @AMDRYZEN5 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Ryzen 5%' OR ValueText LIKE '%AMD Ryzen 5%');
    
    SELECT @AMDRYZEN7 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND (ValueText LIKE '%Ryzen 7%' OR ValueText LIKE '%AMD Ryzen 7%');

    -- Apple M-series (necesitamos crearlos si no existen)
    SELECT @AppleM1 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND ValueText LIKE '%M1%';
    
    SELECT @AppleM2 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND ValueText LIKE '%M2%';
    
    SELECT @AppleM3 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND ValueText LIKE '%M3%';
    
    SELECT @AppleM4 = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ProcessorAttrId AND ValueText LIKE '%M4%';

    -- Crear valores de Apple M-series si no existen
    IF @AppleM1 IS NULL AND EXISTS (SELECT 1 FROM Catalog.Products WHERE NameSpanish LIKE '%M1%' OR NameEnglish LIKE '%M1%')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ProcessorAttrId, 'Apple M1', 'Apple M1', 20);
        SET @AppleM1 = SCOPE_IDENTITY();
        PRINT '  - Creado valor: Apple M1';
    END

    IF @AppleM2 IS NULL AND EXISTS (SELECT 1 FROM Catalog.Products WHERE NameSpanish LIKE '%M2%' OR NameEnglish LIKE '%M2%')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ProcessorAttrId, 'Apple M2', 'Apple M2', 21);
        SET @AppleM2 = SCOPE_IDENTITY();
        PRINT '  - Creado valor: Apple M2';
    END

    IF @AppleM3 IS NULL AND EXISTS (SELECT 1 FROM Catalog.Products WHERE NameSpanish LIKE '%M3%' OR NameEnglish LIKE '%M3%')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ProcessorAttrId, 'Apple M3', 'Apple M3', 22);
        SET @AppleM3 = SCOPE_IDENTITY();
        PRINT '  - Creado valor: Apple M3';
    END

    IF @AppleM4 IS NULL AND EXISTS (SELECT 1 FROM Catalog.Products WHERE NameSpanish LIKE '%M4%' OR NameEnglish LIKE '%M4%')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ProcessorAttrId, 'Apple M4', 'Apple M4', 23);
        SET @AppleM4 = SCOPE_IDENTITY();
        PRINT '  - Creado valor: Apple M4';
    END

    -- Asignar Intel i9
    IF @IntelI9 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @IntelI9
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Intel Core i9%' OR p.NameEnglish LIKE '%Intel Core i9%' 
               OR p.NameSpanish LIKE '%Core i9%' OR p.NameEnglish LIKE '%Core i9%'
               OR p.NameSpanish LIKE '%i9-%' OR p.NameEnglish LIKE '%i9-%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Intel i9 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Intel i7
    IF @IntelI7 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @IntelI7
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Intel Core i7%' OR p.NameEnglish LIKE '%Intel Core i7%' 
               OR p.NameSpanish LIKE '%Core i7%' OR p.NameEnglish LIKE '%Core i7%'
               OR p.NameSpanish LIKE '%i7-%' OR p.NameEnglish LIKE '%i7-%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Intel i7 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Intel i5
    IF @IntelI5 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @IntelI5
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Intel Core i5%' OR p.NameEnglish LIKE '%Intel Core i5%' 
               OR p.NameSpanish LIKE '%Core i5%' OR p.NameEnglish LIKE '%Core i5%'
               OR p.NameSpanish LIKE '%i5-%' OR p.NameEnglish LIKE '%i5-%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Intel i5 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Intel i3
    IF @IntelI3 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @IntelI3
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Intel Core i3%' OR p.NameEnglish LIKE '%Intel Core i3%' 
               OR p.NameSpanish LIKE '%Core i3%' OR p.NameEnglish LIKE '%Core i3%'
               OR p.NameSpanish LIKE '%i3-%' OR p.NameEnglish LIKE '%i3-%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Intel i3 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar AMD Ryzen 7
    IF @AMDRYZEN7 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AMDRYZEN7
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Ryzen 7%' OR p.NameEnglish LIKE '%Ryzen 7%' 
               OR p.NameSpanish LIKE '%AMD Ryzen 7%' OR p.NameEnglish LIKE '%AMD Ryzen 7%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado AMD Ryzen 7 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar AMD Ryzen 5
    IF @AMDRYZEN5 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AMDRYZEN5
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Ryzen 5%' OR p.NameEnglish LIKE '%Ryzen 5%' 
               OR p.NameSpanish LIKE '%AMD Ryzen 5%' OR p.NameEnglish LIKE '%AMD Ryzen 5%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado AMD Ryzen 5 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar AMD Ryzen 3
    IF @AMDRYZEN3 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AMDRYZEN3
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Ryzen 3%' OR p.NameEnglish LIKE '%Ryzen 3%' 
               OR p.NameSpanish LIKE '%AMD Ryzen 3%' OR p.NameEnglish LIKE '%AMD Ryzen 3%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado AMD Ryzen 3 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Apple M4
    IF @AppleM4 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AppleM4
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%M4 chip%' OR p.NameEnglish LIKE '%M4 chip%' 
               OR p.NameSpanish LIKE '%Chip M4%' OR p.NameEnglish LIKE '%Chip M4%'
               OR p.NameSpanish LIKE '%con M4%' OR p.NameEnglish LIKE '%with M4%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Apple M4 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Apple M3
    IF @AppleM3 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AppleM3
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%M3 chip%' OR p.NameEnglish LIKE '%M3 chip%' 
               OR p.NameSpanish LIKE '%Chip M3%' OR p.NameEnglish LIKE '%Chip M3%'
               OR p.NameSpanish LIKE '%con M3%' OR p.NameEnglish LIKE '%with M3%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Apple M3 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Apple M2
    IF @AppleM2 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AppleM2
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%M2 chip%' OR p.NameEnglish LIKE '%M2 chip%' 
               OR p.NameSpanish LIKE '%Chip M2%' OR p.NameEnglish LIKE '%Chip M2%'
               OR p.NameSpanish LIKE '%con M2%' OR p.NameEnglish LIKE '%with M2%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Apple M2 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Apple M1
    IF @AppleM1 IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ProcessorAttrId, @AppleM1
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%M1 chip%' OR p.NameEnglish LIKE '%M1 chip%' 
               OR p.NameSpanish LIKE '%Chip M1%' OR p.NameEnglish LIKE '%Chip M1%'
               OR p.NameSpanish LIKE '%con M1%' OR p.NameEnglish LIKE '%with M1%'
               OR p.NameSpanish LIKE '%Apple M1%' OR p.NameEnglish LIKE '%Apple M1%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ProcessorAttrId
        );
        PRINT '  - Asignado Apple M1 a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END
END
PRINT '';

-- =============================================
-- 6. ASIGNAR CONECTIVIDAD (Productos electrónicos)
-- =============================================
PRINT '6. Asignando Conectividad...';

IF @ConnectivityAttrId IS NOT NULL
BEGIN
    DECLARE @HDMIId INT, @WiFiId INT, @BluetoothId INT, @USBId INT, @EthernetId INT, @WirelessId INT;
    
    SELECT @HDMIId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'HDMI';
    
    SELECT @WiFiId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND (ValueTextEnglish = 'WiFi' OR ValueTextEnglish = 'Wi-Fi');
    
    SELECT @BluetoothId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'Bluetooth';
    
    SELECT @USBId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'USB';
    
    SELECT @EthernetId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'Ethernet';
    
    SELECT @WirelessId = ValueId FROM Catalog.AttributeValues 
    WHERE AttributeId = @ConnectivityAttrId AND ValueTextEnglish = 'Wireless';

    -- Asignar USB a laptops y monitores
    IF @USBId IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @USBId
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%laptop%' OR p.NameEnglish LIKE '%laptop%' 
               OR p.NameSpanish LIKE '%monitor%' OR p.NameEnglish LIKE '%monitor%'
               OR p.NameSpanish LIKE '%USB%' OR p.NameEnglish LIKE '%USB%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @USBId
        );
        PRINT '  - Asignado USB a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Wi-Fi a productos que lo mencionen
    IF @WiFiId IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @WiFiId
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Wi-Fi%' OR p.NameEnglish LIKE '%Wi-Fi%' 
               OR p.NameSpanish LIKE '%WiFi%' OR p.NameEnglish LIKE '%WiFi%'
               OR p.NameSpanish LIKE '%wireless%' OR p.NameEnglish LIKE '%wireless%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @WiFiId
        );
        PRINT '  - Asignado Wi-Fi a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar Bluetooth a productos que lo mencionen
    IF @BluetoothId IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @BluetoothId
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%Bluetooth%' OR p.NameEnglish LIKE '%Bluetooth%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @BluetoothId
        );
        PRINT '  - Asignado Bluetooth a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END

    -- Asignar HDMI a monitores y TVs
    IF @HDMIId IS NOT NULL
    BEGIN
        INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
        SELECT DISTINCT p.ProductId, @ConnectivityAttrId, @HDMIId
        FROM Catalog.Products p
        WHERE (p.NameSpanish LIKE '%monitor%' OR p.NameEnglish LIKE '%monitor%' 
               OR p.NameSpanish LIKE '%TV%' OR p.NameEnglish LIKE '%TV%'
               OR p.NameSpanish LIKE '%HDMI%' OR p.NameEnglish LIKE '%HDMI%')
        AND NOT EXISTS (
            SELECT 1 FROM Catalog.ProductAttributeValues pav 
            WHERE pav.ProductId = p.ProductId AND pav.AttributeId = @ConnectivityAttrId AND pav.ValueId = @HDMIId
        );
        PRINT '  - Asignado HDMI a ' + CAST(@@ROWCOUNT AS VARCHAR) + ' productos';
    END
END
PRINT '';

-- =============================================
-- 7. VERIFICACIÓN FINAL
-- =============================================
PRINT '========================================';
PRINT 'VERIFICACIÓN FINAL';
PRINT '========================================';
PRINT '';

-- Contar productos con atributos por tipo
SELECT 
    pa.AttributeName,
    pa.AttributeNameEnglish,
    COUNT(DISTINCT pav.ProductId) AS ProductsWithAttribute,
    COUNT(*) AS TotalValues
FROM Catalog.ProductAttributes pa
LEFT JOIN Catalog.ProductAttributeValues pav ON pa.AttributeId = pav.AttributeId
WHERE pa.IsFilterable = 1
GROUP BY pa.AttributeId, pa.AttributeName, pa.AttributeNameEnglish
ORDER BY ProductsWithAttribute DESC, pa.AttributeName;

PRINT '';
PRINT '========================================';
PRINT 'ASIGNACIÓN COMPLETADA';
PRINT '========================================';
GO

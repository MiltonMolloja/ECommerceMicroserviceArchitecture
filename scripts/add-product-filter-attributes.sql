-- =============================================
-- Script para agregar atributos de filtros de productos
-- Fecha: 2025-12-02
-- Descripción: Agrega atributos para Year, Condition, MountType y actualiza Connectivity
-- =============================================

USE [ECommerceDb]
GO

PRINT 'Iniciando inserción de atributos de filtros...'

-- =============================================
-- 1. ATRIBUTO: Año del Modelo (ModelYear)
-- =============================================
PRINT 'Insertando atributo: Año del Modelo'

IF NOT EXISTS (SELECT 1 FROM Catalog.ProductAttributes WHERE AttributeName = 'Año del Modelo')
BEGIN
    INSERT INTO Catalog.ProductAttributes (AttributeName, AttributeNameEnglish, AttributeType, Unit, IsFilterable, IsSearchable, DisplayOrder)
    VALUES ('Año del Modelo', 'Model Year', 'Select', NULL, 1, 0, 9)

    DECLARE @YearAttributeId INT = SCOPE_IDENTITY()
    PRINT 'Atributo Año del Modelo creado con ID: ' + CAST(@YearAttributeId AS VARCHAR)

    -- Insertar valores de años (2018-2024)
    INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES 
        (@YearAttributeId, '2024', '2024', 1),
        (@YearAttributeId, '2023', '2023', 2),
        (@YearAttributeId, '2022', '2022', 3),
        (@YearAttributeId, '2021', '2021', 4),
        (@YearAttributeId, '2020', '2020', 5),
        (@YearAttributeId, '2019', '2019', 6),
        (@YearAttributeId, '2018', '2018', 7)

    PRINT 'Valores de años insertados correctamente'
END
ELSE
BEGIN
    PRINT 'El atributo Año del Modelo ya existe'
END
GO

-- =============================================
-- 2. ATRIBUTO: Condición (Condition)
-- =============================================
PRINT 'Insertando atributo: Condición'

IF NOT EXISTS (SELECT 1 FROM Catalog.ProductAttributes WHERE AttributeName = 'Condición')
BEGIN
    INSERT INTO Catalog.ProductAttributes (AttributeName, AttributeNameEnglish, AttributeType, Unit, IsFilterable, IsSearchable, DisplayOrder)
    VALUES ('Condición', 'Condition', 'Select', NULL, 1, 1, 10)

    DECLARE @ConditionAttributeId INT = SCOPE_IDENTITY()
    PRINT 'Atributo Condición creado con ID: ' + CAST(@ConditionAttributeId AS VARCHAR)

    -- Insertar valores de condición
    INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES 
        (@ConditionAttributeId, 'Nuevo', 'New', 1),
        (@ConditionAttributeId, 'Renovado', 'Refurbished', 2),
        (@ConditionAttributeId, 'Usado', 'Used', 3)

    PRINT 'Valores de condición insertados correctamente'
END
ELSE
BEGIN
    PRINT 'El atributo Condición ya existe'
END
GO

-- =============================================
-- 3. ATRIBUTO: Tipo de Montaje (MountType)
-- =============================================
PRINT 'Insertando atributo: Tipo de Montaje'

IF NOT EXISTS (SELECT 1 FROM Catalog.ProductAttributes WHERE AttributeName = 'Tipo de Montaje')
BEGIN
    INSERT INTO Catalog.ProductAttributes (AttributeName, AttributeNameEnglish, AttributeType, Unit, IsFilterable, IsSearchable, DisplayOrder)
    VALUES ('Tipo de Montaje', 'Mount Type', 'MultiSelect', NULL, 1, 0, 11)

    DECLARE @MountTypeAttributeId INT = SCOPE_IDENTITY()
    PRINT 'Atributo Tipo de Montaje creado con ID: ' + CAST(@MountTypeAttributeId AS VARCHAR)

    -- Insertar valores de tipo de montaje
    INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES 
        (@MountTypeAttributeId, 'Montaje en Mesa', 'Desk Mount', 1),
        (@MountTypeAttributeId, 'Montaje en Pared', 'Wall Mount', 2)

    PRINT 'Valores de tipo de montaje insertados correctamente'
END
ELSE
BEGIN
    PRINT 'El atributo Tipo de Montaje ya existe'
END
GO

-- =============================================
-- 4. ACTUALIZAR ATRIBUTO: Conectividad (Connectivity)
-- Agregar HDMI y Ethernet si no existen
-- =============================================
PRINT 'Actualizando atributo: Conectividad'

-- Buscar el ID del atributo Conectividad
DECLARE @ConnectivityAttributeId INT
SELECT @ConnectivityAttributeId = AttributeId 
FROM Catalog.ProductAttributes 
WHERE AttributeName = 'Conectividad' OR AttributeNameEnglish = 'Connectivity'

IF @ConnectivityAttributeId IS NOT NULL
BEGIN
    PRINT 'Atributo Conectividad encontrado con ID: ' + CAST(@ConnectivityAttributeId AS VARCHAR)

    -- Insertar HDMI si no existe
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ConnectivityAttributeId AND ValueTextEnglish = 'HDMI')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ConnectivityAttributeId, 'HDMI', 'HDMI', 5)
        PRINT 'Valor HDMI agregado a Conectividad'
    END
    ELSE
    BEGIN
        PRINT 'Valor HDMI ya existe'
    END

    -- Insertar Ethernet si no existe
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ConnectivityAttributeId AND ValueTextEnglish = 'Ethernet')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ConnectivityAttributeId, 'Ethernet', 'Ethernet', 6)
        PRINT 'Valor Ethernet agregado a Conectividad'
    END
    ELSE
    BEGIN
        PRINT 'Valor Ethernet ya existe'
    END

    -- Actualizar USB-C a USB si existe
    IF EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ConnectivityAttributeId AND ValueTextEnglish = 'USB-C')
    BEGIN
        UPDATE Catalog.AttributeValues 
        SET ValueText = 'USB', ValueTextEnglish = 'USB'
        WHERE AttributeId = @ConnectivityAttributeId AND ValueTextEnglish = 'USB-C'
        PRINT 'Valor USB-C actualizado a USB'
    END
    
    -- Insertar USB si no existe (por si no había USB-C)
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ConnectivityAttributeId AND ValueTextEnglish = 'USB')
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ConnectivityAttributeId, 'USB', 'USB', 7)
        PRINT 'Valor USB agregado a Conectividad'
    END
END
ELSE
BEGIN
    PRINT 'ADVERTENCIA: No se encontró el atributo Conectividad'
END
GO

-- =============================================
-- 5. ACTUALIZAR ATRIBUTO: Resolución
-- Asegurar que existan 8K, 4K, 1080p, 720p
-- =============================================
PRINT 'Verificando valores de Resolución'

DECLARE @ResolutionAttributeId INT
SELECT @ResolutionAttributeId = AttributeId 
FROM Catalog.ProductAttributes 
WHERE AttributeName = 'Resolución' OR AttributeNameEnglish = 'Resolution'

IF @ResolutionAttributeId IS NOT NULL
BEGIN
    PRINT 'Atributo Resolución encontrado con ID: ' + CAST(@ResolutionAttributeId AS VARCHAR)

    -- Insertar 8K si no existe
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ResolutionAttributeId AND (ValueText LIKE '%8K%' OR ValueTextEnglish LIKE '%8K%'))
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ResolutionAttributeId, '8K Ultra HD', '8K Ultra HD', 1)
        PRINT 'Valor 8K agregado a Resolución'
    END

    -- Insertar 4K si no existe
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ResolutionAttributeId AND (ValueText LIKE '%4K%' OR ValueTextEnglish LIKE '%4K%'))
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ResolutionAttributeId, '4K Ultra HD', '4K Ultra HD', 2)
        PRINT 'Valor 4K agregado a Resolución'
    END

    -- Insertar 1080p si no existe
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ResolutionAttributeId AND (ValueText LIKE '%1080%' OR ValueTextEnglish LIKE '%1080%'))
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ResolutionAttributeId, 'Full HD 1080p', 'Full HD 1080p', 3)
        PRINT 'Valor 1080p agregado a Resolución'
    END

    -- Insertar 720p si no existe
    IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE AttributeId = @ResolutionAttributeId AND (ValueText LIKE '%720%' OR ValueTextEnglish LIKE '%720%'))
    BEGIN
        INSERT INTO Catalog.AttributeValues (AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
        VALUES (@ResolutionAttributeId, 'HD 720p', 'HD 720p', 4)
        PRINT 'Valor 720p agregado a Resolución'
    END
END
GO

-- =============================================
-- 6. VERIFICACIÓN FINAL
-- =============================================
PRINT ''
PRINT '========================================='
PRINT 'VERIFICACIÓN DE ATRIBUTOS CREADOS'
PRINT '========================================='

SELECT 
    pa.AttributeId,
    pa.AttributeName,
    pa.AttributeNameEnglish,
    pa.AttributeType,
    pa.IsFilterable,
    COUNT(av.ValueId) AS TotalValues
FROM Catalog.ProductAttributes pa
LEFT JOIN AttributeValues av ON pa.AttributeId = av.AttributeId
WHERE pa.AttributeName IN ('Año del Modelo', 'Condición', 'Tipo de Montaje', 'Conectividad', 'Resolución')
   OR pa.AttributeNameEnglish IN ('Model Year', 'Condition', 'Mount Type', 'Connectivity', 'Resolution')
GROUP BY pa.AttributeId, pa.AttributeName, pa.AttributeNameEnglish, pa.AttributeType, pa.IsFilterable
ORDER BY pa.DisplayOrder

PRINT ''
PRINT '========================================='
PRINT 'VALORES DE CADA ATRIBUTO'
PRINT '========================================='

SELECT 
    pa.AttributeName,
    av.ValueText,
    av.ValueTextEnglish,
    av.DisplayOrder
FROM Catalog.ProductAttributes pa
INNER JOIN AttributeValues av ON pa.AttributeId = av.AttributeId
WHERE pa.AttributeName IN ('Año del Modelo', 'Condición', 'Tipo de Montaje', 'Conectividad', 'Resolución')
   OR pa.AttributeNameEnglish IN ('Model Year', 'Condition', 'Mount Type', 'Connectivity', 'Resolution')
ORDER BY pa.AttributeName, av.DisplayOrder

PRINT ''
PRINT 'Script completado exitosamente!'
GO

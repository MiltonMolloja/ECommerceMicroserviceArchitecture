-- ===============================================
-- Script de Datos de Prueba para Catalog Service
-- Ejecutar en SQL Server Management Studio
-- ===============================================

USE ECommerceDb;
GO

PRINT 'Iniciando inserción de datos de prueba...';
GO

-- ====================
-- 1. INSERTAR MARCAS
-- ====================
PRINT '1/5 Insertando marcas...';

IF NOT EXISTS (SELECT 1 FROM Catalog.Brands WHERE BrandId BETWEEN 101 AND 110)
BEGIN
    SET IDENTITY_INSERT Catalog.Brands ON;

    INSERT INTO Catalog.Brands (BrandId, [Name], [Description], IsActive)
    VALUES
    (101, 'Samsung', 'Electrónica y tecnología de alta calidad', 1),
    (102, 'Apple', 'Innovación y diseño premium', 1),
    (103, 'Sony', 'Entretenimiento y electrónica', 1),
    (104, 'LG', 'Electrodomésticos y electrónica', 1),
    (105, 'Nike', 'Ropa y calzado deportivo', 1),
    (106, 'Adidas', 'Ropa y accesorios deportivos', 1),
    (107, 'HP', 'Computadoras y accesorios', 1),
    (108, 'Dell', 'Soluciones tecnológicas', 1),
    (109, 'Logitech', 'Periféricos y accesorios', 1),
    (110, 'Canon', 'Cámaras e impresoras', 1);

    SET IDENTITY_INSERT Catalog.Brands OFF;

    PRINT '   ✓ 10 marcas insertadas';
END
ELSE
BEGIN
    PRINT '   • Marcas ya existen, saltando...';
END

GO

-- ==========================
-- 2. INSERTAR PRODUCTOS
-- ==========================
PRINT '2/5 Insertando productos...';

IF NOT EXISTS (SELECT 1 FROM Catalog.Products WHERE ProductId BETWEEN 201 AND 250)
BEGIN
    SET IDENTITY_INSERT Catalog.Products ON;

    -- Smartphones (201-210)
    INSERT INTO Catalog.Products (ProductId, NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Price, Stock, BrandId, IsActive, IsFeatured, SKU, Brand, Slug)
    VALUES
    (201, 'Samsung Galaxy A54', 'Samsung Galaxy A54', 'Smartphone con pantalla AMOLED 6.4" y cámara de 50MP', 'Smartphone with AMOLED 6.4" display and 50MP camera', 599.99, 25, 101, 1, 1, 'SM-A54-BLK', 'Samsung', 'samsung-galaxy-a54'),
    (202, 'Samsung Galaxy S23', 'Samsung Galaxy S23', 'Flagship con Snapdragon 8 Gen 2', 'Flagship with Snapdragon 8 Gen 2', 899.99, 15, 101, 1, 1, 'SM-S23-BLK', 'Samsung', 'samsung-galaxy-s23'),
    (203, 'iPhone 14', 'iPhone 14', 'iPhone con chip A15 Bionic', 'iPhone with A15 Bionic chip', 799.99, 30, 102, 1, 1, 'IPHONE-14', 'Apple', 'iphone-14'),
    (204, 'iPhone 14 Pro', 'iPhone 14 Pro', 'iPhone Pro con Dynamic Island', 'iPhone Pro with Dynamic Island', 1099.99, 20, 102, 1, 1, 'IPHONE-14PRO', 'Apple', 'iphone-14-pro'),
    (205, 'iPhone 15 Pro Max', 'iPhone 15 Pro Max', 'iPhone más avanzado con titanio', 'Most advanced iPhone with titanium', 1399.99, 12, 102, 1, 1, 'IPHONE-15PM', 'Apple', 'iphone-15-pro-max'),
    (206, 'Sony Xperia 5 V', 'Sony Xperia 5 V', 'Smartphone compacto profesional', 'Compact professional smartphone', 699.99, 18, 103, 1, 0, 'XPERIA-5V', 'Sony', 'sony-xperia-5v'),
    (207, 'Sony Xperia 1 V', 'Sony Xperia 1 V', 'Flagship con pantalla 4K HDR OLED', 'Flagship with 4K HDR OLED display', 1199.99, 10, 103, 1, 1, 'XPERIA-1V', 'Sony', 'sony-xperia-1v'),
    (208, 'Samsung Galaxy M34', 'Samsung Galaxy M34', 'Smartphone económico batería 6000mAh', 'Budget smartphone with 6000mAh battery', 349.99, 40, 101, 1, 0, 'SM-M34', 'Samsung', 'samsung-galaxy-m34'),
    (209, 'iPhone 13', 'iPhone 13', 'iPhone con chip A15', 'iPhone with A15 chip', 649.99, 35, 102, 1, 0, 'IPHONE-13', 'Apple', 'iphone-13'),
    (210, 'Samsung Galaxy Z Fold 5', 'Samsung Galaxy Z Fold 5', 'Smartphone plegable premium', 'Premium foldable smartphone', 1799.99, 8, 101, 1, 1, 'SM-ZFOLD5', 'Samsung', 'samsung-zfold5');

    -- Laptops (211-220)
    INSERT INTO Catalog.Products (ProductId, NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Price, Stock, BrandId, IsActive, IsFeatured, SKU, Brand, Slug)
    VALUES
    (211, 'HP Pavilion 15', 'HP Pavilion 15', 'Laptop versátil Core i5 16GB RAM', 'Versatile laptop Core i5 16GB RAM', 849.99, 20, 107, 1, 0, 'HP-PAV15', 'HP', 'hp-pavilion-15'),
    (212, 'HP Envy x360', 'HP Envy x360', 'Laptop convertible 2-en-1', 'Convertible 2-in-1 laptop', 1099.99, 15, 107, 1, 1, 'HP-ENVY360', 'HP', 'hp-envy-x360'),
    (213, 'Dell Inspiron 15', 'Dell Inspiron 15', 'Laptop uso diario AMD Ryzen 5', 'Daily use laptop AMD Ryzen 5', 749.99, 25, 108, 1, 0, 'DELL-INS15', 'Dell', 'dell-inspiron-15'),
    (214, 'Dell XPS 13', 'Dell XPS 13', 'Ultrabook premium InfinityEdge', 'Premium InfinityEdge ultrabook', 1299.99, 12, 108, 1, 1, 'DELL-XPS13', 'Dell', 'dell-xps-13'),
    (215, 'Dell Precision 5570', 'Dell Precision 5570', 'Workstation móvil profesional', 'Professional mobile workstation', 1899.99, 8, 108, 1, 1, 'DELL-PREC5570', 'Dell', 'dell-precision-5570'),
    (216, 'HP Omen 16', 'HP Omen 16', 'Laptop gaming RTX 4060', 'Gaming laptop RTX 4060', 1499.99, 10, 107, 1, 1, 'HP-OMEN16', 'HP', 'hp-omen-16'),
    (217, 'MacBook Air M2', 'MacBook Air M2', 'Laptop ultradelgada chip M2', 'Ultra-thin laptop M2 chip', 1199.99, 18, 102, 1, 1, 'MBA-M2', 'Apple', 'macbook-air-m2'),
    (218, 'MacBook Pro 14" M3', 'MacBook Pro 14" M3', 'Laptop profesional chip M3 Pro', 'Professional laptop M3 Pro chip', 1999.99, 10, 102, 1, 1, 'MBP-14M3', 'Apple', 'macbook-pro-14-m3'),
    (219, 'HP ProBook 450', 'HP ProBook 450', 'Laptop empresarial resistente', 'Rugged business laptop', 949.99, 22, 107, 1, 0, 'HP-PRO450', 'HP', 'hp-probook-450'),
    (220, 'Dell Latitude 5540', 'Dell Latitude 5540', 'Laptop corporativa seguridad avanzada', 'Corporate laptop advanced security', 1149.99, 16, 108, 1, 0, 'DELL-LAT5540', 'Dell', 'dell-latitude-5540');

    -- Zapatillas (221-230)
    INSERT INTO Catalog.Products (ProductId, NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Price, Stock, BrandId, IsActive, IsFeatured, SKU, Brand, Slug)
    VALUES
    (221, 'Nike Air Max 270', 'Nike Air Max 270', 'Zapatillas con unidad Air visible', 'Sneakers with visible Air unit', 129.99, 45, 105, 1, 1, 'NIKE-AM270', 'Nike', 'nike-air-max-270'),
    (222, 'Nike React Infinity Run', 'Nike React Infinity Run', 'Zapatillas running amortiguación React', 'Running shoes React cushioning', 149.99, 38, 105, 1, 1, 'NIKE-REACT', 'Nike', 'nike-react-infinity'),
    (223, 'Nike Pegasus 40', 'Nike Pegasus 40', 'Zapatillas versátiles running', 'Versatile running shoes', 139.99, 42, 105, 1, 0, 'NIKE-PEG40', 'Nike', 'nike-pegasus-40'),
    (224, 'Adidas Ultraboost 23', 'Adidas Ultraboost 23', 'Zapatillas tecnología Boost', 'Sneakers with Boost technology', 179.99, 30, 106, 1, 1, 'ADIDAS-UB23', 'Adidas', 'adidas-ultraboost-23'),
    (225, 'Adidas Solarboost 5', 'Adidas Solarboost 5', 'Zapatillas running soporte', 'Support running shoes', 159.99, 35, 106, 1, 0, 'ADIDAS-SB5', 'Adidas', 'adidas-solarboost-5'),
    (226, 'Adidas Supernova+', 'Adidas Supernova+', 'Zapatillas entrenamiento diario', 'Daily training shoes', 119.99, 48, 106, 1, 0, 'ADIDAS-SN', 'Adidas', 'adidas-supernova'),
    (227, 'Nike ZoomX Vaporfly', 'Nike ZoomX Vaporfly', 'Zapatillas competición ultraligeras', 'Ultra-light competition shoes', 249.99, 20, 105, 1, 1, 'NIKE-VAPOR', 'Nike', 'nike-vaporfly'),
    (228, 'Adidas Adizero Boston 12', 'Adidas Adizero Boston 12', 'Zapatillas tempo placa carbono', 'Tempo shoes carbon plate', 139.99, 28, 106, 1, 0, 'ADIDAS-BOST12', 'Adidas', 'adidas-boston-12'),
    (229, 'Nike Air Zoom Pegasus', 'Nike Air Zoom Pegasus', 'Zapatillas clásicas Zoom Air', 'Classic shoes Zoom Air', 124.99, 40, 105, 1, 0, 'NIKE-AZPEG', 'Nike', 'nike-zoom-pegasus'),
    (230, 'Adidas 4DFWD 3', 'Adidas 4DFWD 3', 'Zapatillas tecnología impresión 3D', 'Shoes with 3D printing tech', 219.99, 22, 106, 1, 1, 'ADIDAS-4D3', 'Adidas', 'adidas-4dfwd-3');

    -- TVs (231-240)
    INSERT INTO Catalog.Products (ProductId, NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Price, Stock, BrandId, IsActive, IsFeatured, SKU, Brand, Slug)
    VALUES
    (231, 'Samsung Crystal UHD 43"', 'Samsung Crystal UHD 43"', 'Smart TV 4K procesador Crystal 4K', 'Smart TV 4K Crystal 4K processor', 449.99, 30, 101, 1, 0, 'SAM-CU43', 'Samsung', 'samsung-crystal-43'),
    (232, 'Samsung QLED 55"', 'Samsung QLED 55"', 'Smart TV Quantum Dot HDR', 'Smart TV Quantum Dot HDR', 799.99, 20, 101, 1, 1, 'SAM-QLED55', 'Samsung', 'samsung-qled-55'),
    (233, 'LG NanoCell 50"', 'LG NanoCell 50"', 'Smart TV tecnología NanoCell', 'Smart TV NanoCell technology', 599.99, 25, 104, 1, 0, 'LG-NANO50', 'LG', 'lg-nanocell-50'),
    (234, 'LG OLED 55"', 'LG OLED 55"', 'Smart TV OLED negros perfectos', 'Smart TV OLED perfect blacks', 1299.99, 12, 104, 1, 1, 'LG-OLED55', 'LG', 'lg-oled-55'),
    (235, 'Sony Bravia X75K 50"', 'Sony Bravia X75K 50"', 'Smart TV 4K Google TV', 'Smart TV 4K Google TV', 649.99, 22, 103, 1, 0, 'SONY-X75K50', 'Sony', 'sony-bravia-x75k'),
    (236, 'Sony Bravia XR A80L 55"', 'Sony Bravia XR A80L 55"', 'TV OLED premium Cognitive XR', 'Premium OLED TV Cognitive XR', 1499.99, 10, 103, 1, 1, 'SONY-A80L55', 'Sony', 'sony-bravia-a80l'),
    (237, 'Samsung Neo QLED 65"', 'Samsung Neo QLED 65"', 'Mini LED Quantum Matrix', 'Mini LED Quantum Matrix', 1699.99, 8, 101, 1, 1, 'SAM-NQLED65', 'Samsung', 'samsung-neo-qled-65'),
    (238, 'LG UHD 43"', 'LG UHD 43"', 'Smart TV 4K económica webOS', 'Budget Smart TV 4K webOS', 399.99, 35, 104, 1, 0, 'LG-UHD43', 'LG', 'lg-uhd-43'),
    (239, 'Sony Bravia XR X90K 65"', 'Sony Bravia XR X90K 65"', 'Full Array LED XR Triluminos', 'Full Array LED XR Triluminos', 1199.99, 14, 103, 1, 1, 'SONY-X90K65', 'Sony', 'sony-bravia-x90k'),
    (240, 'Samsung The Frame 55"', 'Samsung The Frame 55"', 'TV estilo cuadro modo Arte', 'Art-style TV with Art mode', 1099.99, 16, 101, 1, 1, 'SAM-FRAME55', 'Samsung', 'samsung-frame-55');

    -- Accesorios (241-250)
    INSERT INTO Catalog.Products (ProductId, NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Price, Stock, BrandId, IsActive, IsFeatured, SKU, Brand, Slug)
    VALUES
    (241, 'Logitech MX Master 3S', 'Logitech MX Master 3S', 'Mouse inalámbrico premium 8K DPI', 'Premium wireless mouse 8K DPI', 99.99, 50, 109, 1, 1, 'LOGI-MXM3S', 'Logitech', 'logitech-mx-master-3s'),
    (242, 'Logitech G502 HERO', 'Logitech G502 HERO', 'Mouse gaming sensor HERO 25K', 'Gaming mouse HERO 25K sensor', 79.99, 45, 109, 1, 1, 'LOGI-G502', 'Logitech', 'logitech-g502'),
    (243, 'Logitech Lift Vertical', 'Logitech Lift Vertical', 'Mouse ergonómico vertical', 'Vertical ergonomic mouse', 69.99, 40, 109, 1, 0, 'LOGI-LIFT', 'Logitech', 'logitech-lift'),
    (244, 'Logitech MX Anywhere 3', 'Logitech MX Anywhere 3', 'Mouse compacto portátil', 'Compact portable mouse', 79.99, 48, 109, 1, 0, 'LOGI-MXA3', 'Logitech', 'logitech-mx-anywhere'),
    (245, 'Logitech G203 Lightsync', 'Logitech G203 Lightsync', 'Mouse gaming RGB económico', 'Budget RGB gaming mouse', 39.99, 60, 109, 1, 0, 'LOGI-G203', 'Logitech', 'logitech-g203'),
    (246, 'Logitech MX Keys', 'Logitech MX Keys', 'Teclado mecánico inalámbrico', 'Wireless mechanical keyboard', 119.99, 35, 109, 1, 1, 'LOGI-MXKEYS', 'Logitech', 'logitech-mx-keys'),
    (247, 'Logitech G Pro X', 'Logitech G Pro X', 'Teclado mecánico gaming', 'Gaming mechanical keyboard', 149.99, 28, 109, 1, 1, 'LOGI-GPROX', 'Logitech', 'logitech-gpro-x'),
    (248, 'Logitech K380', 'Logitech K380', 'Teclado inalámbrico compacto', 'Compact wireless keyboard', 44.99, 55, 109, 1, 0, 'LOGI-K380', 'Logitech', 'logitech-k380'),
    (249, 'Logitech G915 TKL', 'Logitech G915 TKL', 'Teclado mecánico gaming wireless', 'Wireless gaming mechanical keyboard', 229.99, 20, 109, 1, 1, 'LOGI-G915TKL', 'Logitech', 'logitech-g915-tkl'),
    (250, 'Logitech MX Mechanical', 'Logitech MX Mechanical', 'Teclado mecánico productividad', 'Mechanical keyboard productivity', 159.99, 30, 109, 1, 0, 'LOGI-MXMECH', 'Logitech', 'logitech-mx-mechanical');

    SET IDENTITY_INSERT Catalog.Products OFF;

    PRINT '   ✓ 50 productos insertados';
END
ELSE
BEGIN
    PRINT '   • Productos ya existen, saltando...';
END

GO

-- =====================
-- 3. INSERTAR ATRIBUTOS
-- =====================
PRINT '3/5 Insertando atributos...';

SET QUOTED_IDENTIFIER ON;

IF NOT EXISTS (SELECT 1 FROM Catalog.ProductAttributes WHERE AttributeId BETWEEN 101 AND 108)
BEGIN
    SET IDENTITY_INSERT Catalog.ProductAttributes ON;

    INSERT INTO Catalog.ProductAttributes (AttributeId, AttributeName, AttributeNameEnglish, AttributeType, Unit, IsFilterable, IsSearchable, DisplayOrder)
    VALUES
    (101, 'Color', 'Color', 'Select', NULL, 1, 0, 1),
    (102, 'Almacenamiento', 'Storage', 'Select', 'GB', 1, 0, 2),
    (103, 'Memoria RAM', 'RAM', 'Select', 'GB', 1, 0, 3),
    (104, 'Tamaño de Pantalla', 'Screen Size', 'Numeric', 'pulgadas', 1, 0, 4),
    (105, 'Talla', 'Size', 'Select', NULL, 1, 0, 5),
    (106, 'Procesador', 'Processor', 'Select', NULL, 1, 0, 6),
    (107, 'Resolución', 'Resolution', 'Select', NULL, 1, 0, 7),
    (108, 'Conectividad', 'Connectivity', 'MultiSelect', NULL, 1, 0, 8);

    SET IDENTITY_INSERT Catalog.ProductAttributes OFF;

    PRINT '   ✓ 8 atributos insertados';
END
ELSE
BEGIN
    PRINT '   • Atributos ya existen, saltando...';
END

GO

-- ============================
-- 4. INSERTAR VALORES ATRIBUTOS
-- ============================
PRINT '4/5 Insertando valores de atributos...';

IF NOT EXISTS (SELECT 1 FROM Catalog.AttributeValues WHERE ValueId BETWEEN 1001 AND 1050)
BEGIN
    SET IDENTITY_INSERT Catalog.AttributeValues ON;

    -- Color (101)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1001, 101, 'Negro', 'Black', 1),
    (1002, 101, 'Blanco', 'White', 2),
    (1003, 101, 'Azul', 'Blue', 3),
    (1004, 101, 'Rojo', 'Red', 4),
    (1005, 101, 'Gris', 'Gray', 5),
    (1006, 101, 'Plateado', 'Silver', 6);

    -- Almacenamiento (102)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1007, 102, '64 GB', '64GB', 1),
    (1008, 102, '128 GB', '128GB', 2),
    (1009, 102, '256 GB', '256GB', 3),
    (1010, 102, '512 GB', '512GB', 4),
    (1011, 102, '1 TB', '1TB', 5);

    -- RAM (103)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1012, 103, '4 GB', '4GB', 1),
    (1013, 103, '8 GB', '8GB', 2),
    (1014, 103, '16 GB', '16GB', 3),
    (1015, 103, '32 GB', '32GB', 4);

    -- Talla (105)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1023, 105, '38', '38', 1),
    (1024, 105, '39', '39', 2),
    (1025, 105, '40', '40', 3),
    (1026, 105, '41', '41', 4),
    (1027, 105, '42', '42', 5),
    (1028, 105, '43', '43', 6);

    -- Procesador (106)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1029, 106, 'Intel Core i5', 'i5', 1),
    (1030, 106, 'Intel Core i7', 'i7', 2),
    (1031, 106, 'Intel Core i9', 'i9', 3),
    (1032, 106, 'Apple M2', 'M2', 4),
    (1033, 106, 'Apple M3', 'M3', 5),
    (1034, 106, 'AMD Ryzen 5', 'Ryzen 5', 6);

    -- Resolución (107)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1035, 107, 'Full HD 1920x1080', 'FHD', 1),
    (1036, 107, '4K Ultra HD', '4K', 2),
    (1037, 107, '8K Ultra HD', '8K', 3);

    -- Conectividad (108)
    INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
    VALUES
    (1038, 108, 'Wi-Fi', 'WiFi', 1),
    (1039, 108, 'Bluetooth', 'Bluetooth', 2),
    (1040, 108, 'USB-C', 'USB-C', 3),
    (1041, 108, 'Inalámbrico', 'Wireless', 4);

    SET IDENTITY_INSERT Catalog.AttributeValues OFF;

    PRINT '   ✓ 35 valores de atributos insertados';
END
ELSE
BEGIN
    PRINT '   • Valores ya existen, saltando...';
END

GO

-- ==============================
-- 5. INSERTAR REVIEWS Y RATINGS
-- ==============================
PRINT '5/5 Insertando reviews y ratings...';

-- Insertar reviews para cada producto (201-250)
DECLARE @ProdId INT = 201;
DECLARE @ReviewCount INT;
DECLARE @i INT;

WHILE @ProdId <= 250
BEGIN
    SET @ReviewCount = 3 + (ABS(CHECKSUM(NEWID())) % 5); -- 3-7 reviews por producto
    SET @i = 0;

    WHILE @i < @ReviewCount
    BEGIN
        DECLARE @Rating DECIMAL(2,1) = CASE
            WHEN @i % 5 < 3 THEN 5.0
            WHEN @i % 5 = 3 THEN 4.0
            WHEN @i % 5 = 4 THEN 3.0
            ELSE 2.0
        END;

        INSERT INTO Catalog.ProductReviews (ProductId, UserId, Rating, Title, Comment, IsVerifiedPurchase, IsApproved, CreatedAt)
        VALUES (
            @ProdId,
            ABS(CHECKSUM(NEWID())) % 500 + 1,
            @Rating,
            CASE WHEN @Rating >= 4.5 THEN 'Excelente producto'
                 WHEN @Rating >= 3.5 THEN 'Buen producto'
                 ELSE 'Regular' END,
            CASE WHEN @Rating >= 4.5 THEN 'Muy satisfecho con la compra. Excelente calidad y rendimiento.'
                 WHEN @Rating >= 3.5 THEN 'Buen producto en general. Cumple con lo esperado.'
                 ELSE 'Producto regular. Esperaba un poco más.' END,
            CASE WHEN @i % 2 = 0 THEN 1 ELSE 0 END,
            1,
            DATEADD(DAY, -1 * (ABS(CHECKSUM(NEWID())) % 90), GETDATE())
        );

        SET @i = @i + 1;
    END

    SET @ProdId = @ProdId + 1;
END

PRINT '   ✓ Reviews insertadas';

-- Calcular y guardar ratings agregados
INSERT INTO Catalog.ProductRatings (ProductId, AverageRating, TotalReviews, Rating5Star, Rating4Star, Rating3Star, Rating2Star, Rating1Star)
SELECT
    ProductId,
    CAST(AVG(Rating) AS DECIMAL(3,2)),
    COUNT(*),
    SUM(CASE WHEN Rating = 5.0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN Rating = 4.0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN Rating = 3.0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN Rating = 2.0 THEN 1 ELSE 0 END),
    SUM(CASE WHEN Rating = 1.0 THEN 1 ELSE 0 END)
FROM Catalog.ProductReviews
WHERE ProductId BETWEEN 201 AND 250
GROUP BY ProductId;

PRINT '   ✓ Ratings calculados';

GO

-- =================
-- RESUMEN FINAL
-- =================
PRINT '';
PRINT '==========================================';
PRINT 'RESUMEN DE DATOS INSERTADOS:';
PRINT '==========================================';

SELECT 'Marcas' AS [Tipo], COUNT(*) AS [Total]
FROM Catalog.Brands WHERE BrandId BETWEEN 101 AND 110
UNION ALL
SELECT 'Productos', COUNT(*)
FROM Catalog.Products WHERE ProductId BETWEEN 201 AND 250
UNION ALL
SELECT 'Atributos', COUNT(*)
FROM Catalog.ProductAttributes WHERE AttributeId BETWEEN 101 AND 108
UNION ALL
SELECT 'Valores Atributos', COUNT(*)
FROM Catalog.AttributeValues WHERE ValueId BETWEEN 1001 AND 1050
UNION ALL
SELECT 'Reviews', COUNT(*)
FROM Catalog.ProductReviews WHERE ProductId BETWEEN 201 AND 250
UNION ALL
SELECT 'Ratings', COUNT(*)
FROM Catalog.ProductRatings WHERE ProductId BETWEEN 201 AND 250;

PRINT '';
PRINT '✓✓✓ Script completado exitosamente! ✓✓✓';
PRINT '✓ Puedes probar las funcionalidades de filtrado avanzado';
PRINT '✓ Productos ID: 201-250';
PRINT '==========================================';
GO

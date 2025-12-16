-- Script para agregar datos de prueba aleatorios para el filtrado avanzado
-- Ejecutar después de aplicar todas las migraciones

USE ECommerceDb;
GO

-- Limpiar datos de prueba existentes (opcional - descomentar si quieres borrar)
-- DELETE FROM Catalog.ProductReviews;
-- DELETE FROM Catalog.ProductRatings;
-- DELETE FROM Catalog.ProductAttributeValues;
-- DELETE FROM Catalog.AttributeValues;
-- DELETE FROM Catalog.ProductAttributes;
-- DELETE FROM Catalog.Products WHERE ProductId > 100;
-- DELETE FROM Catalog.Brands WHERE BrandId > 10;

-- Insertar Marcas
SET IDENTITY_INSERT Catalog.Brands ON;

IF NOT EXISTS (SELECT 1 FROM Catalog.Brands WHERE BrandId = 1)
BEGIN
    INSERT INTO Catalog.Brands (BrandId, [Name], [Description], LogoUrl, IsActive) VALUES
    (1, 'Samsung', 'Electrónica y tecnología de alta calidad', NULL, 1),
    (2, 'Apple', 'Innovación y diseño premium', NULL, 1),
    (3, 'Sony', 'Entretenimiento y electrónica', NULL, 1),
    (4, 'LG', 'Electrodomésticos y electrónica', NULL, 1),
    (5, 'Nike', 'Ropa y calzado deportivo', NULL, 1),
    (6, 'Adidas', 'Ropa y accesorios deportivos', NULL, 1),
    (7, 'HP', 'Computadoras y accesorios', NULL, 1),
    (8, 'Dell', 'Soluciones tecnológicas', NULL, 1),
    (9, 'Logitech', 'Periféricos y accesorios', NULL, 1),
    (10, 'Canon', 'Cámaras e impresoras', NULL, 1);
END

SET IDENTITY_INSERT Catalog.Brands OFF;
GO

-- Insertar Productos variados
SET IDENTITY_INSERT Catalog.Products ON;

DECLARE @StartId INT = 101;

-- Smartphones (10 productos)
INSERT INTO Catalog.Products (ProductId, [Name], [Description], Price, Stock, BrandId)
VALUES
(@StartId, 'Smartphone Samsung Galaxy A54', 'Smartphone de última generación con pantalla AMOLED 6.4" y cámara de 50MP', 599.99, 25, 1),
(@StartId + 1, 'Smartphone Samsung Galaxy S23', 'Flagship con procesador Snapdragon 8 Gen 2 y cámara triple', 899.99, 15, 1),
(@StartId + 2, 'Smartphone Samsung Galaxy Z Fold 5', 'Smartphone plegable premium con pantalla Dynamic AMOLED 2X', 1799.99, 8, 1),
(@StartId + 3, 'iPhone 14', 'iPhone con chip A15 Bionic y cámara dual mejorada', 799.99, 30, 2),
(@StartId + 4, 'iPhone 14 Pro', 'iPhone Pro con Dynamic Island y cámara de 48MP', 1099.99, 20, 2),
(@StartId + 5, 'iPhone 15 Pro Max', 'El iPhone más avanzado con titanio y zoom óptico 5x', 1399.99, 12, 2),
(@StartId + 6, 'Sony Xperia 5 V', 'Smartphone compacto con tecnología de cámara profesional', 699.99, 18, 3),
(@StartId + 7, 'Sony Xperia 1 V', 'Flagship con pantalla 4K HDR OLED y audio Hi-Res', 1199.99, 10, 3),
(@StartId + 8, 'Samsung Galaxy M34', 'Smartphone económico con batería de 6000mAh', 349.99, 40, 1),
(@StartId + 9, 'iPhone 13', 'iPhone con chip A15 y modo Cinemático', 649.99, 35, 2);

-- Laptops (10 productos)
INSERT INTO Catalog.Products (ProductId, [Name], [Description], Price, Stock, BrandId)
VALUES
(@StartId + 10, 'HP Pavilion 15', 'Laptop versátil con Intel Core i5 y 16GB RAM', 849.99, 20, 7),
(@StartId + 11, 'HP Envy x360', 'Laptop convertible 2-en-1 con pantalla táctil', 1099.99, 15, 7),
(@StartId + 12, 'Dell Inspiron 15', 'Laptop para uso diario con AMD Ryzen 5', 749.99, 25, 8),
(@StartId + 13, 'Dell XPS 13', 'Ultrabook premium con pantalla InfinityEdge', 1299.99, 12, 8),
(@StartId + 14, 'Dell Precision 5570', 'Workstation móvil con gráficos NVIDIA profesionales', 1899.99, 8, 8),
(@StartId + 15, 'HP Omen 16', 'Laptop gaming con RTX 4060 y pantalla 165Hz', 1499.99, 10, 7),
(@StartId + 16, 'MacBook Air M2', 'Laptop ultradelgada con chip M2 de Apple', 1199.99, 18, 2),
(@StartId + 17, 'MacBook Pro 14" M3', 'Laptop profesional con chip M3 Pro', 1999.99, 10, 2),
(@StartId + 18, 'HP ProBook 450', 'Laptop empresarial resistente y segura', 949.99, 22, 7),
(@StartId + 19, 'Dell Latitude 5540', 'Laptop corporativa con seguridad avanzada', 1149.99, 16, 8);

-- Zapatillas deportivas (10 productos)
INSERT INTO Catalog.Products (ProductId, [Name], [Description], Price, Stock, BrandId)
VALUES
(@StartId + 20, 'Nike Air Max 270', 'Zapatillas con unidad Air visible y diseño moderno', 129.99, 45, 5),
(@StartId + 21, 'Nike React Infinity Run', 'Zapatillas de running con amortiguación React', 149.99, 38, 5),
(@StartId + 22, 'Nike Pegasus 40', 'Zapatillas versátiles para todo tipo de corredor', 139.99, 42, 5),
(@StartId + 23, 'Adidas Ultraboost 23', 'Zapatillas con tecnología Boost para máximo retorno de energía', 179.99, 30, 6),
(@StartId + 24, 'Adidas Solarboost 5', 'Zapatillas de running con soporte y estabilidad', 159.99, 35, 6),
(@StartId + 25, 'Adidas Supernova+', 'Zapatillas de entrenamiento diario cómodas', 119.99, 48, 6),
(@StartId + 26, 'Nike ZoomX Vaporfly', 'Zapatillas de competición ultraligeras', 249.99, 20, 5),
(@StartId + 27, 'Adidas Adizero Boston 12', 'Zapatillas tempo con placa de carbono', 139.99, 28, 6),
(@StartId + 28, 'Nike Air Zoom Pegasus', 'Zapatillas clásicas renovadas con Zoom Air', 124.99, 40, 5),
(@StartId + 29, 'Adidas 4DFWD 3', 'Zapatillas con tecnología de impresión 3D', 219.99, 22, 6);

-- TVs (10 productos)
INSERT INTO Catalog.Products (ProductId, [Name], [Description], Price, Stock, BrandId)
VALUES
(@StartId + 30, 'Samsung Crystal UHD 43"', 'Smart TV 4K con procesador Crystal 4K', 449.99, 30, 1),
(@StartId + 31, 'Samsung QLED 55"', 'Smart TV con tecnología Quantum Dot y HDR', 799.99, 20, 1),
(@StartId + 32, 'LG NanoCell 50"', 'Smart TV con tecnología NanoCell y AI ThinQ', 599.99, 25, 4),
(@StartId + 33, 'LG OLED 55"', 'Smart TV OLED con negros perfectos y α9 Gen 6 AI', 1299.99, 12, 4),
(@StartId + 34, 'Sony Bravia X75K 50"', 'Smart TV 4K con Google TV y TRILUMINOS PRO', 649.99, 22, 3),
(@StartId + 35, 'Sony Bravia XR A80L 55"', 'TV OLED premium con procesador Cognitive XR', 1499.99, 10, 3),
(@StartId + 36, 'Samsung Neo QLED 65"', 'Mini LED con Quantum Matrix Technology', 1699.99, 8, 1),
(@StartId + 37, 'LG UHD 43"', 'Smart TV 4K económica con webOS', 399.99, 35, 4),
(@StartId + 38, 'Sony Bravia XR X90K 65"', 'Full Array LED con XR Triluminos Pro', 1199.99, 14, 3),
(@StartId + 39, 'Samsung The Frame 55"', 'TV estilo cuadro con modo Arte', 1099.99, 16, 1);

-- Accesorios (10 productos)
INSERT INTO Catalog.Products (ProductId, [Name], [Description], Price, Stock, BrandId)
VALUES
(@StartId + 40, 'Logitech MX Master 3S', 'Mouse inalámbrico premium con 8K DPI', 99.99, 50, 9),
(@StartId + 41, 'Logitech G502 HERO', 'Mouse gaming con sensor HERO 25K', 79.99, 45, 9),
(@StartId + 42, 'Logitech Lift Vertical', 'Mouse ergonómico vertical para reducir tensión', 69.99, 40, 9),
(@StartId + 43, 'Logitech MX Anywhere 3', 'Mouse compacto para trabajar en cualquier lugar', 79.99, 48, 9),
(@StartId + 44, 'Logitech G203 Lightsync', 'Mouse gaming RGB económico', 39.99, 60, 9),
(@StartId + 45, 'Logitech MX Keys', 'Teclado mecánico inalámbrico retroiluminado', 119.99, 35, 9),
(@StartId + 46, 'Logitech G Pro X', 'Teclado mecánico gaming con switches intercambiables', 149.99, 28, 9),
(@StartId + 47, 'Logitech K380', 'Teclado inalámbrico compacto multi-dispositivo', 44.99, 55, 9),
(@StartId + 48, 'Logitech G915 TKL', 'Teclado mecánico gaming inalámbrico low-profile', 229.99, 20, 9),
(@StartId + 49, 'Logitech MX Mechanical', 'Teclado mecánico para productividad', 159.99, 30, 9);

SET IDENTITY_INSERT Catalog.Products OFF;
GO

-- Crear Atributos
SET IDENTITY_INSERT Catalog.ProductAttributes ON;

INSERT INTO Catalog.ProductAttributes (AttributeId, AttributeName, AttributeNameEnglish, AttributeType, Unit, IsFilterable, IsSearchable, DisplayOrder, CategoryId)
VALUES
(1, 'Color', 'Color', 'Select', NULL, 1, 0, 1, NULL),
(2, 'Almacenamiento', 'Storage', 'Select', 'GB', 1, 0, 2, NULL),
(3, 'Memoria RAM', 'RAM', 'Select', 'GB', 1, 0, 3, NULL),
(4, 'Tamaño de Pantalla', 'Screen Size', 'Numeric', 'pulgadas', 1, 0, 4, NULL),
(5, 'Talla', 'Size', 'Select', NULL, 1, 0, 5, NULL),
(6, 'Procesador', 'Processor', 'Select', NULL, 1, 0, 6, NULL),
(7, 'Resolución', 'Resolution', 'Select', NULL, 1, 0, 7, NULL),
(8, 'Conectividad', 'Connectivity', 'MultiSelect', NULL, 1, 0, 8, NULL);

SET IDENTITY_INSERT Catalog.ProductAttributes OFF;
GO

-- Valores de atributos
SET IDENTITY_INSERT Catalog.AttributeValues ON;

-- Color
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(1, 1, 'Negro', 'Black', 1),
(2, 1, 'Blanco', 'White', 2),
(3, 1, 'Azul', 'Blue', 3),
(4, 1, 'Rojo', 'Red', 4),
(5, 1, 'Gris', 'Gray', 5),
(6, 1, 'Plateado', 'Silver', 6);

-- Almacenamiento
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(7, 2, '64 GB', '64GB', 1),
(8, 2, '128 GB', '128GB', 2),
(9, 2, '256 GB', '256GB', 3),
(10, 2, '512 GB', '512GB', 4),
(11, 2, '1 TB', '1TB', 5);

-- RAM
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(12, 3, '4 GB', '4GB', 1),
(13, 3, '8 GB', '8GB', 2),
(14, 3, '16 GB', '16GB', 3),
(15, 3, '32 GB', '32GB', 4);

-- Talla
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(23, 5, '38', '38', 1),
(24, 5, '39', '39', 2),
(25, 5, '40', '40', 3),
(26, 5, '41', '41', 4),
(27, 5, '42', '42', 5),
(28, 5, '43', '43', 6);

-- Procesador
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(29, 6, 'Intel Core i5', 'i5', 1),
(30, 6, 'Intel Core i7', 'i7', 2),
(31, 6, 'Intel Core i9', 'i9', 3),
(32, 6, 'Apple M1', 'M1', 4),
(33, 6, 'Apple M2', 'M2', 5),
(34, 6, 'Apple M3', 'M3', 6),
(35, 6, 'AMD Ryzen 5', 'Ryzen 5', 7);

-- Resolución
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(36, 7, 'Full HD 1920x1080', 'FHD', 1),
(37, 7, '4K Ultra HD', '4K', 2),
(38, 7, '8K Ultra HD', '8K', 3);

-- Conectividad
INSERT INTO Catalog.AttributeValues (ValueId, AttributeId, ValueText, ValueTextEnglish, DisplayOrder)
VALUES
(39, 8, 'Wi-Fi', 'WiFi', 1),
(40, 8, 'Bluetooth', 'Bluetooth', 2),
(41, 8, 'USB-C', 'USB-C', 3),
(42, 8, 'Inalámbrico', 'Wireless', 4);

SET IDENTITY_INSERT Catalog.AttributeValues OFF;
GO

-- Asignar atributos a productos

-- Smartphones (101-110) - Color, Almacenamiento, RAM
INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId, TextValue, NumericValue, BooleanValue)
VALUES
-- Samsung Galaxy A54
(101, 1, 3, NULL, NULL, NULL), -- Azul
(101, 2, 8, NULL, 128, NULL), -- 128GB
(101, 3, 13, NULL, 8, NULL), -- 8GB RAM
(101, 4, 1, NULL, 6.4, NULL), -- 6.4"

-- Samsung Galaxy S23
(102, 1, 1, NULL, NULL, NULL), -- Negro
(102, 2, 9, NULL, 256, NULL), -- 256GB
(102, 3, 13, NULL, 8, NULL), -- 8GB RAM
(102, 4, 1, NULL, 6.1, NULL), -- 6.1"

-- Samsung Z Fold 5
(103, 1, 1, NULL, NULL, NULL), -- Negro
(103, 2, 10, NULL, 512, NULL), -- 512GB
(103, 3, 14, NULL, 12, NULL), -- 12GB RAM
(103, 4, 1, NULL, 7.6, NULL), -- 7.6"

-- iPhone 14
(104, 1, 2, NULL, NULL, NULL), -- Blanco
(104, 2, 8, NULL, 128, NULL), -- 128GB
(104, 3, 12, NULL, 6, NULL), -- 6GB RAM
(104, 4, 1, NULL, 6.1, NULL), -- 6.1"

-- iPhone 14 Pro
(105, 1, 6, NULL, NULL, NULL), -- Plateado
(105, 2, 9, NULL, 256, NULL), -- 256GB
(105, 3, 12, NULL, 6, NULL), -- 6GB RAM
(105, 4, 1, NULL, 6.1, NULL), -- 6.1"

-- iPhone 15 Pro Max
(106, 1, 5, NULL, NULL, NULL), -- Gris (Titanio)
(106, 2, 10, NULL, 512, NULL), -- 512GB
(106, 3, 13, NULL, 8, NULL), -- 8GB RAM
(106, 4, 1, NULL, 6.7, NULL), -- 6.7"

-- Sony Xperia 5 V
(107, 1, 3, NULL, NULL, NULL), -- Azul
(107, 2, 8, NULL, 128, NULL), -- 128GB
(107, 3, 13, NULL, 8, NULL), -- 8GB RAM
(107, 4, 1, NULL, 6.1, NULL), -- 6.1"

-- Sony Xperia 1 V
(108, 1, 1, NULL, NULL, NULL), -- Negro
(108, 2, 9, NULL, 256, NULL), -- 256GB
(108, 3, 14, NULL, 12, NULL), -- 12GB RAM
(108, 4, 1, NULL, 6.5, NULL), -- 6.5"

-- Samsung M34
(109, 1, 3, NULL, NULL, NULL), -- Azul
(109, 2, 8, NULL, 128, NULL), -- 128GB
(109, 3, 12, NULL, 6, NULL), -- 6GB RAM
(109, 4, 1, NULL, 6.6, NULL), -- 6.6"

-- iPhone 13
(110, 1, 4, NULL, NULL, NULL), -- Rojo
(110, 2, 8, NULL, 128, NULL), -- 128GB
(110, 3, 12, NULL, 4, NULL), -- 4GB RAM
(110, 4, 1, NULL, 6.1, NULL); -- 6.1"

-- Laptops (111-120) - Color, Almacenamiento, RAM, Procesador, Pantalla
INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId, TextValue, NumericValue, BooleanValue)
VALUES
-- HP Pavilion 15
(111, 1, 6, NULL, NULL, NULL), -- Plateado
(111, 2, 10, NULL, 512, NULL), -- 512GB
(111, 3, 14, NULL, 16, NULL), -- 16GB RAM
(111, 6, 29, NULL, NULL, NULL), -- i5
(111, 4, 1, NULL, 15.6, NULL), -- 15.6"

-- HP Envy x360
(112, 1, 5, NULL, NULL, NULL), -- Gris
(112, 2, 10, NULL, 512, NULL), -- 512GB
(112, 3, 14, NULL, 16, NULL), -- 16GB RAM
(112, 6, 30, NULL, NULL, NULL), -- i7
(112, 4, 1, NULL, 15.6, NULL), -- 15.6"

-- Dell Inspiron 15
(113, 1, 6, NULL, NULL, NULL), -- Plateado
(113, 2, 9, NULL, 256, NULL), -- 256GB
(113, 3, 13, NULL, 8, NULL), -- 8GB RAM
(113, 6, 35, NULL, NULL, NULL), -- Ryzen 5
(113, 4, 1, NULL, 15.6, NULL), -- 15.6"

-- Dell XPS 13
(114, 1, 6, NULL, NULL, NULL), -- Plateado
(114, 2, 10, NULL, 512, NULL), -- 512GB
(114, 3, 14, NULL, 16, NULL), -- 16GB RAM
(114, 6, 30, NULL, NULL, NULL), -- i7
(114, 4, 1, NULL, 13.4, NULL), -- 13.4"

-- Dell Precision 5570
(115, 1, 1, NULL, NULL, NULL), -- Negro
(115, 2, 11, NULL, 1024, NULL), -- 1TB
(115, 3, 15, NULL, 32, NULL), -- 32GB RAM
(115, 6, 31, NULL, NULL, NULL), -- i9
(115, 4, 1, NULL, 15.6, NULL), -- 15.6"

-- HP Omen 16
(116, 1, 1, NULL, NULL, NULL), -- Negro
(116, 2, 11, NULL, 1024, NULL), -- 1TB
(116, 3, 14, NULL, 16, NULL), -- 16GB RAM
(116, 6, 30, NULL, NULL, NULL), -- i7
(116, 4, 1, NULL, 16.1, NULL), -- 16.1"

-- MacBook Air M2
(117, 1, 6, NULL, NULL, NULL), -- Plateado
(117, 2, 9, NULL, 256, NULL), -- 256GB
(117, 3, 13, NULL, 8, NULL), -- 8GB RAM
(117, 6, 33, NULL, NULL, NULL), -- M2
(117, 4, 1, NULL, 13.6, NULL), -- 13.6"

-- MacBook Pro 14" M3
(118, 1, 5, NULL, NULL, NULL), -- Gris
(118, 2, 10, NULL, 512, NULL), -- 512GB
(118, 3, 14, NULL, 18, NULL), -- 18GB RAM (unificada)
(118, 6, 34, NULL, NULL, NULL), -- M3
(118, 4, 1, NULL, 14.2, NULL), -- 14.2"

-- HP ProBook 450
(119, 1, 6, NULL, NULL, NULL), -- Plateado
(119, 2, 9, NULL, 256, NULL), -- 256GB
(119, 3, 13, NULL, 8, NULL), -- 8GB RAM
(119, 6, 29, NULL, NULL, NULL), -- i5
(119, 4, 1, NULL, 15.6, NULL), -- 15.6"

-- Dell Latitude 5540
(120, 1, 1, NULL, NULL, NULL), -- Negro
(120, 2, 10, NULL, 512, NULL), -- 512GB
(120, 3, 14, NULL, 16, NULL), -- 16GB RAM
(120, 6, 30, NULL, NULL, NULL), -- i7
(120, 4, 1, NULL, 15.6, NULL); -- 15.6"

-- Zapatillas (121-130) - Color, Talla
INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
VALUES
(121, 1, 1), (121, 5, 26), -- Nike Air Max 270 - Negro, 41
(122, 1, 3), (122, 5, 25), -- Nike React - Azul, 40
(123, 1, 2), (123, 5, 27), -- Nike Pegasus - Blanco, 42
(124, 1, 1), (124, 5, 26), -- Adidas Ultraboost - Negro, 41
(125, 1, 5), (125, 5, 25), -- Adidas Solarboost - Gris, 40
(126, 1, 3), (126, 5, 27), -- Adidas Supernova - Azul, 42
(127, 1, 4), (127, 5, 28), -- Nike Vaporfly - Rojo, 43
(128, 1, 2), (128, 5, 24), -- Adidas Boston - Blanco, 39
(129, 1, 5), (129, 5, 26), -- Nike Zoom - Gris, 41
(130, 1, 1), (130, 5, 25); -- Adidas 4DFWD - Negro, 40

-- TVs (131-140) - Color, Pantalla, Resolución
INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId, NumericValue)
VALUES
(131, 1, 1, NULL), (131, 4, 1, 43), (131, 7, 37, NULL), -- Samsung Crystal 43" - Negro, 43", 4K
(132, 1, 1, NULL), (132, 4, 1, 55), (132, 7, 37, NULL), -- Samsung QLED 55" - Negro, 55", 4K
(133, 1, 1, NULL), (133, 4, 1, 50), (133, 7, 37, NULL), -- LG NanoCell 50" - Negro, 50", 4K
(134, 1, 1, NULL), (134, 4, 1, 55), (134, 7, 37, NULL), -- LG OLED 55" - Negro, 55", 4K
(135, 1, 1, NULL), (135, 4, 1, 50), (135, 7, 37, NULL), -- Sony X75K 50" - Negro, 50", 4K
(136, 1, 1, NULL), (136, 4, 1, 55), (136, 7, 37, NULL), -- Sony A80L 55" - Negro, 55", 4K
(137, 1, 1, NULL), (137, 4, 1, 65), (137, 7, 38, NULL), -- Samsung Neo QLED 65" - Negro, 65", 8K
(138, 1, 1, NULL), (138, 4, 1, 43), (138, 7, 37, NULL), -- LG UHD 43" - Negro, 43", 4K
(139, 1, 1, NULL), (139, 4, 1, 65), (139, 7, 37, NULL), -- Sony X90K 65" - Negro, 65", 4K
(140, 1, 2, NULL), (140, 4, 1, 55), (140, 7, 37, NULL); -- Samsung Frame 55" - Blanco, 55", 4K

-- Accesorios (141-150) - Color, Conectividad
INSERT INTO Catalog.ProductAttributeValues (ProductId, AttributeId, ValueId)
VALUES
(141, 1, 5), (141, 8, 40), (141, 8, 42), -- Logitech MX Master - Gris, Bluetooth, Wireless
(142, 1, 1), (142, 8, 41), -- Logitech G502 - Negro, USB-C
(143, 1, 5), (143, 8, 40), (143, 8, 42), -- Logitech Lift - Gris, Bluetooth, Wireless
(144, 1, 1), (144, 8, 40), (144, 8, 42), -- Logitech MX Anywhere - Negro, Bluetooth, Wireless
(145, 1, 2), (145, 8, 41), -- Logitech G203 - Blanco, USB-C
(146, 1, 5), (146, 8, 40), (146, 8, 42), -- Logitech MX Keys - Gris, Bluetooth, Wireless
(147, 1, 1), (147, 8, 41), -- Logitech G Pro X - Negro, USB-C
(148, 1, 3), (148, 8, 40), (148, 8, 42), -- Logitech K380 - Azul, Bluetooth, Wireless
(149, 1, 1), (149, 8, 40), (149, 8, 42), -- Logitech G915 - Negro, Bluetooth, Wireless
(150, 1, 5), (150, 8, 40), (150, 8, 42); -- Logitech MX Mechanical - Gris, Bluetooth, Wireless

GO

-- Insertar Reviews con distribución realista
DECLARE @ProductIdReview INT = 101;

WHILE @ProductIdReview <= 150
BEGIN
    DECLARE @ReviewsForProduct INT = 3 + (ABS(CHECKSUM(NEWID())) % 8); -- 3-10 reviews
    DECLARE @CurrentReviewNum INT = 0;

    WHILE @CurrentReviewNum < @ReviewsForProduct
    BEGIN
        DECLARE @Rating DECIMAL(2,1) = CASE
            WHEN @CurrentReviewNum % 10 < 5 THEN 5.0  -- 50% de 5 estrellas
            WHEN @CurrentReviewNum % 10 < 8 THEN 4.0  -- 30% de 4 estrellas
            WHEN @CurrentReviewNum % 10 < 9 THEN 3.0  -- 10% de 3 estrellas
            ELSE 2.0  -- 10% de 2 o menos estrellas
        END;

        DECLARE @ReviewTitle NVARCHAR(200) = CASE CAST(@Rating AS INT)
            WHEN 5 THEN CHOOSE((@CurrentReviewNum % 5) + 1, 'Excelente producto', 'Superó mis expectativas', 'Muy recomendable', 'Calidad premium', 'Totalmente satisfecho')
            WHEN 4 THEN CHOOSE((@CurrentReviewNum % 3) + 1, 'Muy bueno', 'Buena compra', 'Recomendado')
            WHEN 3 THEN CHOOSE((@CurrentReviewNum % 2) + 1, 'Cumple lo básico', 'Decente')
            ELSE 'No cumplió expectativas'
        END;

        DECLARE @ReviewComment NVARCHAR(MAX) = CASE CAST(@Rating AS INT)
            WHEN 5 THEN 'El producto llegó en perfecto estado y funciona excelente. La calidad es superior a lo esperado y el rendimiento es sobresaliente. Definitivamente lo recomiendo.'
            WHEN 4 THEN 'Buen producto en general. Cumple con las características descritas. Algunos detalles menores por mejorar pero estoy satisfecho con la compra.'
            WHEN 3 THEN 'El producto está bien para el precio. Cumple con lo básico pero esperaba un poco más de calidad en algunos aspectos.'
            ELSE 'No cumplió con mis expectativas. Aunque funciona, la calidad podría ser mejor. Consideren otras opciones antes de comprar.'
        END;

        INSERT INTO Catalog.ProductReviews (ProductId, UserId, Rating, Title, Comment, IsVerifiedPurchase, IsApproved, CreatedAt)
        VALUES (
            @ProductIdReview,
            ABS(CHECKSUM(NEWID())) % 1000 + 1, -- UserId aleatorio
            @Rating,
            @ReviewTitle,
            @ReviewComment,
            CASE WHEN @CurrentReviewNum % 3 = 0 THEN 1 ELSE 0 END, -- 33% compras verificadas
            1, -- Aprobadas
            DATEADD(DAY, -1 * (ABS(CHECKSUM(NEWID())) % 180), GETDATE())
        );

        SET @CurrentReviewNum = @CurrentReviewNum + 1;
    END

    SET @ProductIdReview = @ProductIdReview + 1;
END

GO

-- Calcular y actualizar ProductRatings basado en las reviews
INSERT INTO Catalog.ProductRatings (ProductId, AverageRating, TotalReviews, Rating5Star, Rating4Star, Rating3Star, Rating2Star, Rating1Star)
SELECT
    ProductId,
    CAST(AVG(Rating) AS DECIMAL(3,2)) as AverageRating,
    COUNT(*) as TotalReviews,
    SUM(CASE WHEN Rating = 5.0 THEN 1 ELSE 0 END) as Rating5Star,
    SUM(CASE WHEN Rating = 4.0 THEN 1 ELSE 0 END) as Rating4Star,
    SUM(CASE WHEN Rating = 3.0 THEN 1 ELSE 0 END) as Rating3Star,
    SUM(CASE WHEN Rating = 2.0 THEN 1 ELSE 0 END) as Rating2Star,
    SUM(CASE WHEN Rating = 1.0 THEN 1 ELSE 0 END) as Rating1Star
FROM Catalog.ProductReviews
WHERE ProductId >= 101
GROUP BY ProductId;

GO

-- Verificar los datos insertados
SELECT 'Resumen de datos insertados:' AS Info;
SELECT 'Productos nuevos' AS Tabla, COUNT(*) AS Total FROM Catalog.Products WHERE ProductId >= 101
UNION ALL
SELECT 'Marcas nuevas', COUNT(*) FROM Catalog.Brands WHERE BrandId <= 10
UNION ALL
SELECT 'Atributos', COUNT(*) FROM Catalog.ProductAttributes WHERE AttributeId <= 8
UNION ALL
SELECT 'Valores de atributos', COUNT(*) FROM Catalog.AttributeValues WHERE AttributeId <= 8
UNION ALL
SELECT 'Relaciones producto-atributo', COUNT(*) FROM Catalog.ProductAttributeValues WHERE ProductId >= 101
UNION ALL
SELECT 'Reviews', COUNT(*) FROM Catalog.ProductReviews WHERE ProductId >= 101
UNION ALL
SELECT 'Ratings', COUNT(*) FROM Catalog.ProductRatings WHERE ProductId >= 101;

GO

PRINT '✓ Script de datos de prueba ejecutado exitosamente!';
PRINT '✓ 50 productos agregados con atributos, reviews y ratings';
GO

-- Insert 10 sample products with random data
INSERT INTO [Catalog].[Products]
(
    [NameSpanish], [NameEnglish],
    [DescriptionSpanish], [DescriptionEnglish],
    [SKU], [Brand], [Slug],
    [Price], [OriginalPrice], [DiscountPercentage], [TaxRate],
    [Images],
    [MetaTitle], [MetaDescription], [MetaKeywords],
    [IsActive], [IsFeatured],
    [CreatedAt], [UpdatedAt]
)
VALUES
-- Product 1: Laptop Gaming
(
    'Laptop Gaming Pro X1', 'Gaming Laptop Pro X1',
    'Potente laptop gaming con procesador Intel i9, 32GB RAM y RTX 4080', 'Powerful gaming laptop with Intel i9 processor, 32GB RAM and RTX 4080',
    'LAP-GAM-001', 'TechPro', 'laptop-gaming-pro-x1',
    1299.99, 1599.99, 18.75, 16.00,
    'https://example.com/images/laptop1.jpg,https://example.com/images/laptop1-2.jpg',
    'Laptop Gaming Pro X1 - Alta Gama', 'Compra la mejor laptop gaming del mercado con RTX 4080', 'laptop,gaming,rtx,intel,alta gama',
    1, 1,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 2: Mouse Inalambrico
(
    'Mouse Inalambrico Ergonomico', 'Ergonomic Wireless Mouse',
    'Mouse inalambrico con diseno ergonomico y 6 botones programables', 'Wireless mouse with ergonomic design and 6 programmable buttons',
    'MOU-WIR-002', 'ErgoTech', 'mouse-inalambrico-ergonomico',
    29.99, NULL, 0, 16.00,
    'https://example.com/images/mouse1.jpg',
    'Mouse Inalambrico Ergonomico', 'Mouse inalambrico de alta precision con diseno ergonomico', 'mouse,inalambrico,ergonomico,gaming',
    1, 0,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 3: Teclado Mecanico
(
    'Teclado Mecanico RGB', 'RGB Mechanical Keyboard',
    'Teclado mecanico con switches Cherry MX Red e iluminacion RGB personalizable', 'Mechanical keyboard with Cherry MX Red switches and customizable RGB lighting',
    'KEY-MEC-003', 'KeyMaster', 'teclado-mecanico-rgb',
    89.99, 119.99, 25.00, 16.00,
    'https://example.com/images/keyboard1.jpg,https://example.com/images/keyboard1-2.jpg,https://example.com/images/keyboard1-3.jpg',
    'Teclado Mecanico RGB - Cherry MX Red', 'El mejor teclado mecanico para gaming y productividad', 'teclado,mecanico,rgb,cherry mx,gaming',
    1, 1,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 4: Monitor 4K
(
    'Monitor 4K UHD 27 Pulgadas', '27-inch 4K UHD Monitor',
    'Monitor profesional 4K con panel IPS, 144Hz y HDR10', 'Professional 4K monitor with IPS panel, 144Hz and HDR10',
    'MON-4K-004', 'ViewPro', 'monitor-4k-uhd-27',
    449.99, NULL, 0, 16.00,
    'https://example.com/images/monitor1.jpg',
    'Monitor 4K UHD 27" - 144Hz', 'Monitor profesional 4K ideal para diseno y gaming', 'monitor,4k,uhd,144hz,ips,hdr',
    1, 0,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 5: Auriculares Gaming
(
    'Auriculares Gaming 7.1', 'Gaming Headset 7.1',
    'Auriculares gaming con sonido surround 7.1, microfono cancelacion de ruido y RGB', 'Gaming headset with 7.1 surround sound, noise-canceling mic and RGB',
    'AUD-GAM-005', 'SoundWave', 'auriculares-gaming-7-1',
    79.99, 99.99, 20.00, 16.00,
    'https://example.com/images/headset1.jpg,https://example.com/images/headset1-2.jpg',
    'Auriculares Gaming 7.1 Surround', 'Auriculares gaming profesionales con sonido envolvente', 'auriculares,gaming,7.1,rgb,microfono',
    1, 1,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 6: SSD NVMe 1TB
(
    'SSD NVMe 1TB Gen4', 'NVMe SSD 1TB Gen4',
    'Disco solido NVMe Gen4 con velocidades de hasta 7000 MB/s', 'NVMe Gen4 solid state drive with speeds up to 7000 MB/s',
    'SSD-NVM-006', 'StoragePlus', 'ssd-nvme-1tb-gen4',
    119.99, 149.99, 20.00, 16.00,
    'https://example.com/images/ssd1.jpg',
    'SSD NVMe 1TB Gen4 - Ultra Rapido', 'Disco SSD NVMe de alta velocidad para tu PC', 'ssd,nvme,gen4,1tb,almacenamiento',
    1, 0,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 7: Webcam HD
(
    'Webcam Full HD 1080p', 'Full HD 1080p Webcam',
    'Camara web Full HD con microfono incorporado y enfoque automatico', 'Full HD webcam with built-in microphone and autofocus',
    'WEB-CAM-007', 'VisionTech', 'webcam-full-hd-1080p',
    49.99, NULL, 0, 16.00,
    'https://example.com/images/webcam1.jpg',
    'Webcam Full HD 1080p', 'Webcam profesional para videollamadas y streaming', 'webcam,full hd,1080p,streaming',
    1, 0,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 8: Silla Gaming
(
    'Silla Gaming Ergonomica Pro', 'Pro Ergonomic Gaming Chair',
    'Silla gaming ergonomica con soporte lumbar, reposabrazos 4D y reclinable 180 grados', 'Ergonomic gaming chair with lumbar support, 4D armrests and 180 degree recline',
    'CHA-GAM-008', 'ComfortSeats', 'silla-gaming-ergonomica-pro',
    249.99, 299.99, 16.67, 16.00,
    'https://example.com/images/chair1.jpg,https://example.com/images/chair1-2.jpg',
    'Silla Gaming Ergonomica Pro', 'La silla gaming mas comoda con soporte lumbar ajustable', 'silla,gaming,ergonomica,lumbar',
    1, 1,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 9: Fuente de Poder
(
    'Fuente de Poder 850W 80+ Gold', '850W 80+ Gold Power Supply',
    'Fuente modular certificada 80+ Gold con proteccion contra sobretension', 'Modular power supply 80+ Gold certified with overvoltage protection',
    'PSU-850-009', 'PowerMax', 'fuente-poder-850w-gold',
    129.99, NULL, 0, 16.00,
    'https://example.com/images/psu1.jpg',
    'Fuente de Poder 850W 80+ Gold Modular', 'Fuente de poder eficiente y confiable para tu PC', 'fuente,poder,850w,modular,80 plus gold',
    1, 0,
    GETUTCDATE(), GETUTCDATE()
),
-- Product 10: Alfombrilla Gaming XXL
(
    'Alfombrilla Gaming XXL RGB', 'XXL RGB Gaming Mouse Pad',
    'Alfombrilla gaming extra grande con iluminacion RGB y superficie de tela premium', 'Extra large gaming mouse pad with RGB lighting and premium cloth surface',
    'PAD-XXL-010', 'MatrixPad', 'alfombrilla-gaming-xxl-rgb',
    34.99, 44.99, 22.22, 16.00,
    'https://example.com/images/mousepad1.jpg',
    'Alfombrilla Gaming XXL RGB', 'Alfombrilla gaming de gran tamano con iluminacion RGB', 'alfombrilla,mousepad,gaming,rgb,xxl',
    1, 0,
    GETUTCDATE(), GETUTCDATE()
);

-- Insert stock for the products
INSERT INTO [Catalog].[ProductInStock] ([ProductId], [Stock], [MinStock], [MaxStock])
SELECT
    [ProductId],
    CASE
        WHEN [ProductId] % 3 = 0 THEN 5    -- Low stock
        WHEN [ProductId] % 3 = 1 THEN 25   -- Medium stock
        ELSE 50                             -- High stock
    END AS Stock,
    10 AS MinStock,
    100 AS MaxStock
FROM [Catalog].[Products]
WHERE [SKU] IN ('LAP-GAM-001', 'MOU-WIR-002', 'KEY-MEC-003', 'MON-4K-004', 'AUD-GAM-005',
                'SSD-NVM-006', 'WEB-CAM-007', 'CHA-GAM-008', 'PSU-850-009', 'PAD-XXL-010');

PRINT '10 productos insertados exitosamente con sus respectivos stocks';
GO

-- Show inserted products
SELECT
    p.[ProductId],
    p.[NameSpanish],
    p.[Brand],
    p.[Price],
    p.[DiscountPercentage],
    CASE WHEN p.[DiscountPercentage] > 0
         THEN p.[Price] * (1 - p.[DiscountPercentage] / 100)
         ELSE p.[Price]
    END AS FinalPrice,
    s.[Stock],
    CASE
        WHEN s.[Stock] <= s.[MinStock] THEN 'LOW'
        WHEN s.[Stock] > s.[MaxStock] THEN 'OVERSTOCK'
        ELSE 'NORMAL'
    END AS StockStatus
FROM [Catalog].[Products] p
LEFT JOIN [Catalog].[ProductInStock] s ON p.[ProductId] = s.[ProductId]
WHERE p.[SKU] LIKE 'LAP-%' OR p.[SKU] LIKE 'MOU-%' OR p.[SKU] LIKE 'KEY-%'
   OR p.[SKU] LIKE 'MON-%' OR p.[SKU] LIKE 'AUD-%' OR p.[SKU] LIKE 'SSD-%'
   OR p.[SKU] LIKE 'WEB-%' OR p.[SKU] LIKE 'CHA-%' OR p.[SKU] LIKE 'PSU-%'
   OR p.[SKU] LIKE 'PAD-%'
ORDER BY p.[ProductId];

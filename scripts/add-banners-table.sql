-- =============================================
-- Script: Crear tabla Banners para Home Page
-- Fecha: 2025-12-10
-- Descripción: Tabla para gestionar banners del hero section
-- =============================================

USE ECommerceDb;
GO

-- Verificar si la tabla ya existe
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Banners' AND schema_id = SCHEMA_ID('Catalog'))
BEGIN
    PRINT 'Creando tabla [Catalog].[Banners]...';
    
    CREATE TABLE [Catalog].[Banners] (
        [BannerId] INT IDENTITY(1,1) NOT NULL,
        [TitleSpanish] NVARCHAR(200) NOT NULL,
        [TitleEnglish] NVARCHAR(200) NOT NULL,
        [SubtitleSpanish] NVARCHAR(500) NULL,
        [SubtitleEnglish] NVARCHAR(500) NULL,
        [ImageUrl] NVARCHAR(500) NOT NULL,
        [ImageUrlMobile] NVARCHAR(500) NULL,
        [LinkUrl] NVARCHAR(500) NULL,
        [ButtonTextSpanish] NVARCHAR(100) NULL,
        [ButtonTextEnglish] NVARCHAR(100) NULL,
        [DisplayOrder] INT NOT NULL DEFAULT 0,
        [Position] NVARCHAR(50) NOT NULL DEFAULT 'hero',
        [StartDate] DATETIME2 NULL,
        [EndDate] DATETIME2 NULL,
        [IsActive] BIT NOT NULL DEFAULT 1,
        [CreatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        [UpdatedAt] DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        
        CONSTRAINT [PK_Banners] PRIMARY KEY CLUSTERED ([BannerId] ASC)
    );
    
    PRINT 'Tabla [Catalog].[Banners] creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla [Catalog].[Banners] ya existe.';
END
GO

-- Crear índices
PRINT 'Creando índices...';

-- Índice compuesto para búsquedas frecuentes (Position + IsActive)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Position_Active' AND object_id = OBJECT_ID('[Catalog].[Banners]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Banners_Position_Active]
    ON [Catalog].[Banners] ([Position], [IsActive])
    INCLUDE ([DisplayOrder], [StartDate], [EndDate]);
    
    PRINT 'Índice IX_Banners_Position_Active creado.';
END

-- Índice para ordenamiento
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_DisplayOrder' AND object_id = OBJECT_ID('[Catalog].[Banners]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Banners_DisplayOrder]
    ON [Catalog].[Banners] ([DisplayOrder]);
    
    PRINT 'Índice IX_Banners_DisplayOrder creado.';
END

-- Índice para vigencia
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Banners_Dates' AND object_id = OBJECT_ID('[Catalog].[Banners]'))
BEGIN
    CREATE NONCLUSTERED INDEX [IX_Banners_Dates]
    ON [Catalog].[Banners] ([StartDate], [EndDate]);
    
    PRINT 'Índice IX_Banners_Dates creado.';
END
GO

-- Insertar datos de ejemplo
PRINT 'Insertando datos de ejemplo...';

IF NOT EXISTS (SELECT 1 FROM [Catalog].[Banners])
BEGIN
    INSERT INTO [Catalog].[Banners] 
        ([TitleSpanish], [TitleEnglish], [SubtitleSpanish], [SubtitleEnglish], 
         [ImageUrl], [LinkUrl], [ButtonTextSpanish], [ButtonTextEnglish], 
         [DisplayOrder], [Position], [IsActive])
    VALUES
        -- Banner 1: Ofertas de Temporada
        (
            'Ofertas de Temporada',
            'Season Deals',
            'Hasta 40% de descuento en electrónica',
            'Up to 40% off on electronics',
            'https://images.unsplash.com/photo-1607082348824-0a96f2a4b9da?q=80&w=2070',
            '/s?HasDiscount=true',
            'Ver Ofertas',
            'Shop Now',
            1,
            'hero',
            1
        ),
        -- Banner 2: Nuevos Productos
        (
            'Nuevos Productos',
            'New Arrivals',
            'Descubre lo último en tecnología',
            'Discover the latest in tech',
            'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?q=80&w=2070',
            '/s?SortBy=Newest',
            'Explorar',
            'Explore',
            2,
            'hero',
            1
        ),
        -- Banner 3: Ofertas Especiales
        (
            'Ofertas Especiales',
            'Special Offers',
            'Ofertas por tiempo limitado',
            'Limited time deals',
            'https://images.unsplash.com/photo-1607083206968-13611e3d76db?q=80&w=2070',
            '/deals',
            'Ver Más',
            'View More',
            3,
            'hero',
            1
        );
    
    PRINT 'Datos de ejemplo insertados: 3 banners.';
END
ELSE
BEGIN
    PRINT 'Ya existen banners en la tabla.';
END
GO

-- Verificar resultados
PRINT 'Verificando resultados...';
SELECT 
    BannerId,
    TitleSpanish,
    TitleEnglish,
    Position,
    DisplayOrder,
    IsActive,
    CreatedAt
FROM [Catalog].[Banners]
ORDER BY DisplayOrder;
GO

PRINT 'Script completado exitosamente.';
GO

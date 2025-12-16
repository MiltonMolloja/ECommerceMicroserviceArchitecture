-- =============================================
-- Script: Add Sample Reviews and Ratings
-- Purpose: Add sample product reviews and calculate ratings
-- Author: AI Assistant
-- Date: 2025-12-03
-- =============================================

USE [ECommerceDb]
GO

-- =============================================
-- STEP 1: Check current state
-- =============================================
DECLARE @ReviewCount INT, @RatingCount INT

SELECT @ReviewCount = COUNT(*) FROM Catalog.ProductReviews
SELECT @RatingCount = COUNT(*) FROM Catalog.ProductRatings

PRINT '=== CURRENT STATE ==='
PRINT 'ProductReviews count: ' + CAST(@ReviewCount AS VARCHAR(10))
PRINT 'ProductRatings count: ' + CAST(@RatingCount AS VARCHAR(10))
PRINT ''

-- =============================================
-- STEP 2: Insert sample reviews for Product 232 (LG TV)
-- =============================================
PRINT '=== INSERTING SAMPLE REVIEWS FOR PRODUCT 232 (LG TV) ==='

-- Check if reviews already exist
IF NOT EXISTS (SELECT 1 FROM Catalog.ProductReviews WHERE ProductId = 232)
BEGIN
    -- Insert 10 sample reviews with varied ratings
    INSERT INTO Catalog.ProductReviews 
        (ProductId, UserId, Rating, Title, Comment, IsVerifiedPurchase, HelpfulCount, NotHelpfulCount, IsApproved, CreatedAt, UpdatedAt)
    VALUES
        -- 5-star reviews (4 reviews)
        (232, 1, 5.0, 'Excelente TV para el precio', 'La calidad de imagen es increíble. Smart TV funciona perfecto. Muy recomendado!', 1, 15, 2, 1, DATEADD(DAY, -30, GETDATE()), DATEADD(DAY, -30, GETDATE())),
        (232, 2, 5.0, 'Muy buena compra', 'Llegó rápido y bien empacado. La configuración fue muy fácil. Excelente producto.', 1, 12, 1, 1, DATEADD(DAY, -25, GETDATE()), DATEADD(DAY, -25, GETDATE())),
        (232, 3, 5.0, 'Recomendado 100%', 'Por el precio que tiene, es una excelente opción. HDR funciona muy bien.', 1, 8, 0, 1, DATEADD(DAY, -20, GETDATE()), DATEADD(DAY, -20, GETDATE())),
        (232, 4, 5.0, 'Perfecto para mi sala', 'El tamaño es ideal para espacios medianos. Sonido decente, imagen nítida.', 1, 10, 1, 1, DATEADD(DAY, -15, GETDATE()), DATEADD(DAY, -15, GETDATE())),
        
        -- 4-star reviews (3 reviews)
        (232, 5, 4.0, 'Buena TV, algunos detalles menores', 'En general muy buena, pero el sonido podría ser mejor. La imagen es excelente.', 1, 7, 2, 1, DATEADD(DAY, -18, GETDATE()), DATEADD(DAY, -18, GETDATE())),
        (232, 6, 4.0, 'Buen producto LG', 'Cumple con lo esperado. El sistema operativo es intuitivo. Falta Bluetooth.', 1, 5, 1, 1, DATEADD(DAY, -12, GETDATE()), DATEADD(DAY, -12, GETDATE())),
        (232, 7, 4.0, 'Relación calidad-precio correcta', 'Para el precio está bien. No es la mejor TV del mercado pero cumple.', 0, 4, 0, 1, DATEADD(DAY, -8, GETDATE()), DATEADD(DAY, -8, GETDATE())),
        
        -- 3-star reviews (2 reviews)
        (232, 8, 3.0, 'Aceptable', 'Funciona bien pero esperaba más. El HDR no es tan impresionante como pensaba.', 1, 3, 5, 1, DATEADD(DAY, -10, GETDATE()), DATEADD(DAY, -10, GETDATE())),
        (232, 9, 3.0, 'Ni buena ni mala', 'Es una TV promedio. Por el precio está bien pero hay mejores opciones.', 0, 2, 3, 1, DATEADD(DAY, -5, GETDATE()), DATEADD(DAY, -5, GETDATE())),
        
        -- 2-star review (1 review)
        (232, 10, 2.0, 'Decepcionante', 'El sonido es muy malo y la imagen no es tan buena como esperaba. Tuve problemas con el WiFi.', 1, 1, 8, 1, DATEADD(DAY, -3, GETDATE()), DATEADD(DAY, -3, GETDATE()))

    PRINT 'Inserted 10 sample reviews for Product 232'
END
ELSE
BEGIN
    PRINT 'Reviews already exist for Product 232. Skipping insert.'
END
PRINT ''

-- =============================================
-- STEP 3: Calculate and insert/update ratings
-- =============================================
PRINT '=== CALCULATING RATINGS FOR PRODUCT 232 ==='

-- Calculate ratings from reviews
DECLARE @ProductId INT = 232
DECLARE @AverageRating DECIMAL(3,2)
DECLARE @TotalReviews INT
DECLARE @Rating5Star INT
DECLARE @Rating4Star INT
DECLARE @Rating3Star INT
DECLARE @Rating2Star INT
DECLARE @Rating1Star INT

-- Get aggregated data from reviews (only approved reviews)
SELECT 
    @AverageRating = CAST(AVG(Rating) AS DECIMAL(3,2)),
    @TotalReviews = COUNT(*),
    @Rating5Star = SUM(CASE WHEN Rating >= 4.5 THEN 1 ELSE 0 END),
    @Rating4Star = SUM(CASE WHEN Rating >= 3.5 AND Rating < 4.5 THEN 1 ELSE 0 END),
    @Rating3Star = SUM(CASE WHEN Rating >= 2.5 AND Rating < 3.5 THEN 1 ELSE 0 END),
    @Rating2Star = SUM(CASE WHEN Rating >= 1.5 AND Rating < 2.5 THEN 1 ELSE 0 END),
    @Rating1Star = SUM(CASE WHEN Rating < 1.5 THEN 1 ELSE 0 END)
FROM Catalog.ProductReviews
WHERE ProductId = @ProductId AND IsApproved = 1

-- Insert or update ProductRating
IF EXISTS (SELECT 1 FROM Catalog.ProductRatings WHERE ProductId = @ProductId)
BEGIN
    UPDATE Catalog.ProductRatings
    SET 
        AverageRating = @AverageRating,
        TotalReviews = @TotalReviews,
        Rating5Star = @Rating5Star,
        Rating4Star = @Rating4Star,
        Rating3Star = @Rating3Star,
        Rating2Star = @Rating2Star,
        Rating1Star = @Rating1Star,
        LastUpdated = GETDATE()
    WHERE ProductId = @ProductId
    
    PRINT 'Updated ProductRating for Product 232'
END
ELSE
BEGIN
    INSERT INTO Catalog.ProductRatings 
        (ProductId, AverageRating, TotalReviews, Rating5Star, Rating4Star, Rating3Star, Rating2Star, Rating1Star, LastUpdated)
    VALUES 
        (@ProductId, @AverageRating, @TotalReviews, @Rating5Star, @Rating4Star, @Rating3Star, @Rating2Star, @Rating1Star, GETDATE())
    
    PRINT 'Inserted ProductRating for Product 232'
END

PRINT 'Average Rating: ' + CAST(@AverageRating AS VARCHAR(10))
PRINT 'Total Reviews: ' + CAST(@TotalReviews AS VARCHAR(10))
PRINT '5-Star: ' + CAST(@Rating5Star AS VARCHAR(10))
PRINT '4-Star: ' + CAST(@Rating4Star AS VARCHAR(10))
PRINT '3-Star: ' + CAST(@Rating3Star AS VARCHAR(10))
PRINT '2-Star: ' + CAST(@Rating2Star AS VARCHAR(10))
PRINT '1-Star: ' + CAST(@Rating1Star AS VARCHAR(10))
PRINT ''

-- =============================================
-- STEP 4: Verify results
-- =============================================
PRINT '=== VERIFICATION ==='
SELECT 
    p.ProductId,
    p.NameSpanish AS ProductName,
    pr.AverageRating,
    pr.TotalReviews,
    pr.Rating5Star,
    pr.Rating4Star,
    pr.Rating3Star,
    pr.Rating2Star,
    pr.Rating1Star,
    pr.LastUpdated
FROM Catalog.Products p
LEFT JOIN Catalog.ProductRatings pr ON p.ProductId = pr.ProductId
WHERE p.ProductId = 232

PRINT ''
PRINT '=== SAMPLE REVIEWS ==='
SELECT TOP 5
    ReviewId,
    ProductId,
    UserId,
    Rating,
    Title,
    Comment,
    IsVerifiedPurchase,
    HelpfulCount,
    CreatedAt
FROM Catalog.ProductReviews
WHERE ProductId = 232
ORDER BY CreatedAt DESC

PRINT ''
PRINT '=== SCRIPT COMPLETED SUCCESSFULLY ==='
GO

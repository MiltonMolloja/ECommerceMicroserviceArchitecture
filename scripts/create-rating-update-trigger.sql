-- =============================================
-- Script: Create Trigger to Auto-Update Product Ratings
-- Purpose: Automatically recalculate ratings when reviews change
-- Author: AI Assistant
-- Date: 2025-12-03
-- =============================================

USE [ECommerceDb]
GO

-- =============================================
-- Drop existing trigger if exists
-- =============================================
IF OBJECT_ID('Catalog.TR_ProductReviews_UpdateRatings', 'TR') IS NOT NULL
BEGIN
    DROP TRIGGER Catalog.TR_ProductReviews_UpdateRatings
    PRINT 'Dropped existing trigger: TR_ProductReviews_UpdateRatings'
END
GO

-- =============================================
-- Create stored procedure to recalculate ratings
-- =============================================
IF OBJECT_ID('Catalog.SP_RecalculateProductRating', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE Catalog.SP_RecalculateProductRating
    PRINT 'Dropped existing procedure: SP_RecalculateProductRating'
END
GO

CREATE PROCEDURE Catalog.SP_RecalculateProductRating
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @AverageRating DECIMAL(3,2)
    DECLARE @TotalReviews INT
    DECLARE @Rating5Star INT
    DECLARE @Rating4Star INT
    DECLARE @Rating3Star INT
    DECLARE @Rating2Star INT
    DECLARE @Rating1Star INT
    
    -- Calculate aggregated ratings from approved reviews only
    SELECT 
        @AverageRating = ISNULL(CAST(AVG(Rating) AS DECIMAL(3,2)), 0),
        @TotalReviews = COUNT(*),
        @Rating5Star = SUM(CASE WHEN Rating >= 4.5 THEN 1 ELSE 0 END),
        @Rating4Star = SUM(CASE WHEN Rating >= 3.5 AND Rating < 4.5 THEN 1 ELSE 0 END),
        @Rating3Star = SUM(CASE WHEN Rating >= 2.5 AND Rating < 3.5 THEN 1 ELSE 0 END),
        @Rating2Star = SUM(CASE WHEN Rating >= 1.5 AND Rating < 2.5 THEN 1 ELSE 0 END),
        @Rating1Star = SUM(CASE WHEN Rating < 1.5 THEN 1 ELSE 0 END)
    FROM Catalog.ProductReviews
    WHERE ProductId = @ProductId AND IsApproved = 1
    
    -- If no reviews, set all to 0
    IF @TotalReviews IS NULL OR @TotalReviews = 0
    BEGIN
        SET @AverageRating = 0
        SET @TotalReviews = 0
        SET @Rating5Star = 0
        SET @Rating4Star = 0
        SET @Rating3Star = 0
        SET @Rating2Star = 0
        SET @Rating1Star = 0
    END
    
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
    END
    ELSE
    BEGIN
        INSERT INTO Catalog.ProductRatings 
            (ProductId, AverageRating, TotalReviews, Rating5Star, Rating4Star, Rating3Star, Rating2Star, Rating1Star, LastUpdated)
        VALUES 
            (@ProductId, @AverageRating, @TotalReviews, @Rating5Star, @Rating4Star, @Rating3Star, @Rating2Star, @Rating1Star, GETDATE())
    END
END
GO

PRINT 'Created procedure: SP_RecalculateProductRating'
GO

-- =============================================
-- Create trigger for INSERT, UPDATE, DELETE
-- =============================================
CREATE TRIGGER Catalog.TR_ProductReviews_UpdateRatings
ON Catalog.ProductReviews
AFTER INSERT, UPDATE, DELETE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Get affected ProductIds from both inserted and deleted tables
    DECLARE @AffectedProducts TABLE (ProductId INT)
    
    INSERT INTO @AffectedProducts (ProductId)
    SELECT DISTINCT ProductId FROM inserted
    UNION
    SELECT DISTINCT ProductId FROM deleted
    
    -- Recalculate ratings for each affected product
    DECLARE @ProductId INT
    
    DECLARE product_cursor CURSOR FOR
    SELECT ProductId FROM @AffectedProducts
    
    OPEN product_cursor
    FETCH NEXT FROM product_cursor INTO @ProductId
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        EXEC Catalog.SP_RecalculateProductRating @ProductId
        FETCH NEXT FROM product_cursor INTO @ProductId
    END
    
    CLOSE product_cursor
    DEALLOCATE product_cursor
END
GO

PRINT 'Created trigger: TR_ProductReviews_UpdateRatings'
GO

PRINT ''
PRINT '=== TRIGGER AND PROCEDURE CREATED SUCCESSFULLY ==='
PRINT 'Ratings will now be automatically updated when reviews change'
GO

-- =============================================
-- Script: add-cart-abandonment-notified-column.sql
-- Description: Adds AbandonmentNotifiedAt column to Cart.ShoppingCarts table
--              for tracking when abandonment notifications were sent
-- Author: DBA Expert
-- Date: 2024
-- =============================================

USE [ECommerce]
GO

SET NOCOUNT ON;

PRINT '============================================='
PRINT 'Adding AbandonmentNotifiedAt column to Cart.ShoppingCarts'
PRINT '============================================='
PRINT ''

-- Check if column already exists
IF NOT EXISTS (
    SELECT 1 
    FROM sys.columns 
    WHERE object_id = OBJECT_ID(N'[Cart].[ShoppingCarts]') 
    AND name = 'AbandonmentNotifiedAt'
)
BEGIN
    PRINT 'Adding column AbandonmentNotifiedAt...'
    
    ALTER TABLE [Cart].[ShoppingCarts]
    ADD [AbandonmentNotifiedAt] DATETIME2(7) NULL;
    
    PRINT 'Column added successfully.'
    
    -- Add index for querying abandoned carts that haven't been notified
    IF NOT EXISTS (
        SELECT 1 
        FROM sys.indexes 
        WHERE object_id = OBJECT_ID(N'[Cart].[ShoppingCarts]') 
        AND name = 'IX_ShoppingCarts_Abandonment'
    )
    BEGIN
        PRINT 'Creating index IX_ShoppingCarts_Abandonment...'
        
        CREATE NONCLUSTERED INDEX [IX_ShoppingCarts_Abandonment]
        ON [Cart].[ShoppingCarts] ([Status], [UpdatedAt], [AbandonmentNotifiedAt])
        INCLUDE ([CartId], [ClientId], [SessionId])
        WHERE [Status] = 1; -- Active carts only
        
        PRINT 'Index created successfully.'
    END
END
ELSE
BEGIN
    PRINT 'Column AbandonmentNotifiedAt already exists. Skipping...'
END

-- Verify the column was added
PRINT ''
PRINT 'Verification:'
SELECT 
    c.name AS ColumnName,
    t.name AS DataType,
    c.max_length,
    c.is_nullable
FROM sys.columns c
INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
WHERE c.object_id = OBJECT_ID(N'[Cart].[ShoppingCarts]')
AND c.name = 'AbandonmentNotifiedAt';

-- Show index info
PRINT ''
PRINT 'Index Information:'
SELECT 
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique,
    i.filter_definition
FROM sys.indexes i
WHERE i.object_id = OBJECT_ID(N'[Cart].[ShoppingCarts]')
AND i.name = 'IX_ShoppingCarts_Abandonment';

PRINT ''
PRINT '============================================='
PRINT 'Migration completed successfully!'
PRINT '============================================='
GO

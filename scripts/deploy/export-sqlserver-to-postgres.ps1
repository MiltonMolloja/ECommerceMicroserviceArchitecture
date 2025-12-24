# Script para exportar datos de SQL Server a PostgreSQL
# Genera archivos SQL con INSERTs compatibles con PostgreSQL

$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputDir = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data"

# Crear directorio de salida
if (-not (Test-Path $OutputDir)) {
    New-Item -ItemType Directory -Path $OutputDir -Force | Out-Null
}

function Escape-PostgresString {
    param([string]$value)
    if ($null -eq $value) { return "NULL" }
    $escaped = $value -replace "'", "''"
    return "'$escaped'"
}

function Convert-ToPostgresValue {
    param($value, $type)
    if ($null -eq $value -or $value -is [System.DBNull]) { return "NULL" }
    
    switch -Regex ($type) {
        "int|bigint|smallint|decimal|numeric|float|real|money" {
            return $value.ToString()
        }
        "bit|boolean" {
            if ($value -eq $true -or $value -eq 1) { return "true" }
            else { return "false" }
        }
        "datetime|date|time" {
            return "'" + $value.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'"
        }
        "uniqueidentifier|guid" {
            return "'" + $value.ToString() + "'"
        }
        default {
            return Escape-PostgresString $value.ToString()
        }
    }
}

function Export-TableToPostgres {
    param(
        [string]$Schema,
        [string]$Table,
        [string]$OutputFile,
        [string]$PostgresSchema = $Schema,
        [string]$PostgresTable = $Table
    )
    
    Write-Host "Exporting [$Schema].[$Table]..." -ForegroundColor Cyan
    
    try {
        $query = "SELECT * FROM [$Schema].[$Table]"
        $data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535
        
        if ($null -eq $data -or $data.Count -eq 0) {
            Write-Host "  No data found in [$Schema].[$Table]" -ForegroundColor Yellow
            return
        }
        
        $count = @($data).Count
        Write-Host "  Found $count rows" -ForegroundColor Green
        
        # Get column info
        $columnsQuery = @"
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_SCHEMA = '$Schema' AND TABLE_NAME = '$Table'
ORDER BY ORDINAL_POSITION
"@
        $columns = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $columnsQuery
        
        $columnNames = ($columns | ForEach-Object { "`"$($_.COLUMN_NAME)`"" }) -join ", "
        $columnTypes = @{}
        foreach ($col in $columns) {
            $columnTypes[$col.COLUMN_NAME] = $col.DATA_TYPE
        }
        
        $inserts = @()
        $inserts += "-- Data for `"$PostgresSchema`".`"$PostgresTable`""
        $inserts += "-- Exported from SQL Server [$Schema].[$Table]"
        $inserts += ""
        
        foreach ($row in $data) {
            $values = @()
            foreach ($col in $columns) {
                $colName = $col.COLUMN_NAME
                $colType = $col.DATA_TYPE
                $value = $row.$colName
                $values += Convert-ToPostgresValue $value $colType
            }
            $valuesStr = $values -join ", "
            $inserts += "INSERT INTO `"$PostgresSchema`".`"$PostgresTable`" ($columnNames) VALUES ($valuesStr);"
        }
        
        $inserts | Out-File -FilePath $OutputFile -Encoding UTF8
        Write-Host "  Saved to $OutputFile" -ForegroundColor Green
    }
    catch {
        Write-Host "  Error exporting [$Schema].[$Table]: $_" -ForegroundColor Red
    }
}

Write-Host "========================================" -ForegroundColor Magenta
Write-Host "SQL Server to PostgreSQL Data Migration" -ForegroundColor Magenta
Write-Host "========================================" -ForegroundColor Magenta
Write-Host ""

# Export Catalog Schema
Write-Host "`n=== CATALOG SCHEMA ===" -ForegroundColor Yellow
Export-TableToPostgres -Schema "Catalog" -Table "Brands" -OutputFile "$OutputDir\01-catalog-brands.sql"
Export-TableToPostgres -Schema "Catalog" -Table "Categories" -OutputFile "$OutputDir\02-catalog-categories.sql"
Export-TableToPostgres -Schema "Catalog" -Table "Products" -OutputFile "$OutputDir\03-catalog-products.sql"
Export-TableToPostgres -Schema "Catalog" -Table "ProductCategories" -OutputFile "$OutputDir\04-catalog-productcategories.sql"
Export-TableToPostgres -Schema "Catalog" -Table "ProductInStock" -OutputFile "$OutputDir\05-catalog-productinstock.sql"
Export-TableToPostgres -Schema "Catalog" -Table "AttributeValues" -OutputFile "$OutputDir\06-catalog-attributevalues.sql"
Export-TableToPostgres -Schema "Catalog" -Table "ProductAttributes" -OutputFile "$OutputDir\07-catalog-productattributes.sql"
Export-TableToPostgres -Schema "Catalog" -Table "ProductAttributeValues" -OutputFile "$OutputDir\08-catalog-productattributevalues.sql"
Export-TableToPostgres -Schema "Catalog" -Table "ProductRatings" -OutputFile "$OutputDir\09-catalog-productratings.sql"
Export-TableToPostgres -Schema "Catalog" -Table "ProductReviews" -OutputFile "$OutputDir\10-catalog-productreviews.sql"
Export-TableToPostgres -Schema "Catalog" -Table "Banners" -OutputFile "$OutputDir\11-catalog-banners.sql"

# Export Identity Schema
Write-Host "`n=== IDENTITY SCHEMA ===" -ForegroundColor Yellow
Export-TableToPostgres -Schema "Identity" -Table "AspNetRoles" -OutputFile "$OutputDir\20-identity-aspnetroles.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "AspNetUsers" -OutputFile "$OutputDir\21-identity-aspnetusers.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "AspNetUserRoles" -OutputFile "$OutputDir\22-identity-aspnetuserroles.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "AspNetRoleClaims" -OutputFile "$OutputDir\23-identity-aspnetroleclaims.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "AspNetUserClaims" -OutputFile "$OutputDir\24-identity-aspnetuserclaims.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "AspNetUserLogins" -OutputFile "$OutputDir\25-identity-aspnetuserlogins.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "AspNetUserTokens" -OutputFile "$OutputDir\26-identity-aspnetusertokens.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "RefreshTokens" -OutputFile "$OutputDir\27-identity-refreshtokens.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "UserAuditLogs" -OutputFile "$OutputDir\28-identity-userauditlogs.sql" -PostgresSchema "Identity"
Export-TableToPostgres -Schema "Identity" -Table "UserBackupCodes" -OutputFile "$OutputDir\29-identity-userbackupcodes.sql" -PostgresSchema "Identity"

# Export Customer Schema
Write-Host "`n=== CUSTOMER SCHEMA ===" -ForegroundColor Yellow
Export-TableToPostgres -Schema "Customer" -Table "Clients" -OutputFile "$OutputDir\30-customer-clients.sql"
Export-TableToPostgres -Schema "Customer" -Table "ClientAddresses" -OutputFile "$OutputDir\31-customer-clientaddresses.sql"

# Export Order Schema
Write-Host "`n=== ORDER SCHEMA ===" -ForegroundColor Yellow
# Note: PaymentTypes and OrderStatuses don't exist in PostgreSQL schema
# Export-TableToPostgres -Schema "Order" -Table "PaymentTypes" -OutputFile "$OutputDir\40-order-paymenttypes.sql" -PostgresSchema "Order"
# Export-TableToPostgres -Schema "Order" -Table "OrderStatuses" -OutputFile "$OutputDir\41-order-orderstatuses.sql" -PostgresSchema "Order"
Export-TableToPostgres -Schema "Order" -Table "Orders" -OutputFile "$OutputDir\42-order-orders.sql" -PostgresSchema "Order"
Export-TableToPostgres -Schema "Order" -Table "OrderDetail" -OutputFile "$OutputDir\43-order-orderdetail.sql" -PostgresSchema "Order" -PostgresTable "OrderDetails"

# Export Cart Schema
Write-Host "`n=== CART SCHEMA ===" -ForegroundColor Yellow
Export-TableToPostgres -Schema "Cart" -Table "ShoppingCarts" -OutputFile "$OutputDir\50-cart-shoppingcarts.sql" -PostgresTable "Carts"
Export-TableToPostgres -Schema "Cart" -Table "CartItems" -OutputFile "$OutputDir\51-cart-cartitems.sql"

# Export Payment Schema
Write-Host "`n=== PAYMENT SCHEMA ===" -ForegroundColor Yellow
# Note: PostgreSQL has only Transactions table, SQL Server has Payments, PaymentDetails, PaymentTransactions
# These tables have different structures, skipping for now
Write-Host "  Skipping Payment tables - different structure between SQL Server and PostgreSQL" -ForegroundColor Yellow

# Export Notification Schema
Write-Host "`n=== NOTIFICATION SCHEMA ===" -ForegroundColor Yellow
Export-TableToPostgres -Schema "Notification" -Table "NotificationTemplates" -OutputFile "$OutputDir\70-notification-templates.sql"
Export-TableToPostgres -Schema "Notification" -Table "Notifications" -OutputFile "$OutputDir\71-notification-notifications.sql"
Export-TableToPostgres -Schema "Notification" -Table "NotificationPreferences" -OutputFile "$OutputDir\72-notification-preferences.sql"

Write-Host "`n========================================" -ForegroundColor Magenta
Write-Host "Export Complete!" -ForegroundColor Green
Write-Host "Files saved to: $OutputDir" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Magenta

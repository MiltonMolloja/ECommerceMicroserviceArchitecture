# Script to migrate Cart tables from SQL Server to PostgreSQL

$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputDir = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data"

function Escape-String {
    param([string]$value)
    if ([string]::IsNullOrEmpty($value)) { return "NULL" }
    $escaped = $value -replace "'", "''"
    return "'$escaped'"
}

function Format-DateTime {
    param($value)
    if ($null -eq $value -or $value -is [System.DBNull]) { return 'NULL' }
    return "'" + ([datetime]$value).ToString('yyyy-MM-dd HH:mm:ss.fff') + "'"
}

# Migrate Carts (ShoppingCarts -> Carts)
Write-Host "Migrating ShoppingCarts to Carts..."
$cartsQuery = @"
SELECT 
    CartId,
    ClientId,
    CreatedAt,
    UpdatedAt,
    CASE WHEN Status = 0 THEN 1 ELSE 0 END as IsActive,
    0 as AbandonmentNotified
FROM [Cart].[ShoppingCarts]
"@

$cartsData = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $cartsQuery

$cartsInserts = @()
$cartsInserts += '-- Data for "Cart"."Carts"'
$cartsInserts += '-- Migrated from SQL Server [Cart].[ShoppingCarts]'
$cartsInserts += ''

foreach ($row in $cartsData) {
    $cartId = $row.CartId
    $clientId = $row.ClientId
    $createdAt = Format-DateTime $row.CreatedAt
    $updatedAt = Format-DateTime $row.UpdatedAt
    $isActive = if ($row.IsActive) { 'true' } else { 'false' }
    $abandonmentNotified = if ($row.AbandonmentNotified) { 'true' } else { 'false' }
    
    $cartsInserts += "INSERT INTO `"Cart`".`"Carts`" (`"CartId`", `"ClientId`", `"CreatedAt`", `"UpdatedAt`", `"IsActive`", `"AbandonmentNotified`") VALUES ($cartId, $clientId, $createdAt, $updatedAt, $isActive, $abandonmentNotified);"
}

if ($cartsData.Count -gt 0) {
    $maxCartId = ($cartsData | Measure-Object -Property CartId -Maximum).Maximum
    $cartsInserts += ""
    $cartsInserts += "SELECT setval('`"Cart`".`"Carts_CartId_seq`"', $($maxCartId + 1));"
}

$cartsInserts | Out-File -FilePath "$OutputDir\50-cart-carts-mapped.sql" -Encoding UTF8
Write-Host "Saved $($cartsData.Count) carts"

# Migrate CartItems
Write-Host "Migrating CartItems..."
$itemsQuery = @"
SELECT 
    CartItemId,
    CartId,
    ProductId,
    ProductName,
    UnitPrice,
    Quantity,
    AddedAt
FROM [Cart].[CartItems]
"@

$itemsData = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $itemsQuery

$itemsInserts = @()
$itemsInserts += '-- Data for "Cart"."CartItems"'
$itemsInserts += '-- Migrated from SQL Server [Cart].[CartItems]'
$itemsInserts += ''

foreach ($row in $itemsData) {
    $cartItemId = $row.CartItemId
    $cartId = $row.CartId
    $productId = $row.ProductId
    $productName = Escape-String $row.ProductName
    $unitPrice = [math]::Round($row.UnitPrice, 2)
    $quantity = $row.Quantity
    $addedAt = Format-DateTime $row.AddedAt
    
    $itemsInserts += "INSERT INTO `"Cart`".`"CartItems`" (`"CartItemId`", `"CartId`", `"ProductId`", `"ProductName`", `"UnitPrice`", `"Quantity`", `"AddedAt`") VALUES ($cartItemId, $cartId, $productId, $productName, $unitPrice, $quantity, $addedAt);"
}

if ($itemsData.Count -gt 0) {
    $maxItemId = ($itemsData | Measure-Object -Property CartItemId -Maximum).Maximum
    $itemsInserts += ""
    $itemsInserts += "SELECT setval('`"Cart`".`"CartItems_CartItemId_seq`"', $($maxItemId + 1));"
}

$itemsInserts | Out-File -FilePath "$OutputDir\51-cart-cartitems-mapped.sql" -Encoding UTF8
Write-Host "Saved $($itemsData.Count) cart items"

# Script to migrate Order.OrderDetail from SQL Server to PostgreSQL
# Maps to OrderDetails table (with 's')

$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputFile = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\43-order-orderdetails-mapped.sql"

# PostgreSQL columns: OrderDetailId, OrderId, ProductId, ProductName, UnitPrice, Quantity, Total

$query = @"
SELECT 
    od.OrderDetailId,
    od.OrderId,
    od.ProductId,
    ISNULL(p.NameSpanish, 'Product ' + CAST(od.ProductId as varchar)) as ProductName,
    od.UnitPrice,
    od.Quantity,
    od.Total
FROM [Order].[OrderDetail] od
LEFT JOIN [Catalog].[Products] p ON od.ProductId = p.ProductId
"@

$data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535

function Escape-String {
    param([string]$value)
    if ([string]::IsNullOrEmpty($value)) { return "NULL" }
    $escaped = $value -replace "'", "''"
    return "'$escaped'"
}

$inserts = @()
$inserts += '-- Data for "Order"."OrderDetails"'
$inserts += '-- Migrated from SQL Server [Order].[OrderDetail]'
$inserts += ''

foreach ($row in $data) {
    $orderDetailId = $row.OrderDetailId
    $orderId = $row.OrderId
    $productId = $row.ProductId
    $productName = Escape-String $row.ProductName
    $unitPrice = [math]::Round($row.UnitPrice, 2)
    $quantity = $row.Quantity
    $total = [math]::Round($row.Total, 2)
    
    $inserts += "INSERT INTO `"Order`".`"OrderDetails`" (`"OrderDetailId`", `"OrderId`", `"ProductId`", `"ProductName`", `"UnitPrice`", `"Quantity`", `"Total`") VALUES ($orderDetailId, $orderId, $productId, $productName, $unitPrice, $quantity, $total);"
}

# Add sequence reset
$maxId = ($data | Measure-Object -Property OrderDetailId -Maximum).Maximum
$inserts += ""
$inserts += "-- Reset sequence"
$inserts += "SELECT setval('`"Order`".`"OrderDetails_OrderDetailId_seq`"', $($maxId + 1));"

$inserts | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Saved $($data.Count) order details to $OutputFile"

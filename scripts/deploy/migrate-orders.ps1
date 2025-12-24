# Script to migrate Order.Orders from SQL Server to PostgreSQL
# Maps different schema structures

$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputFile = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\42-order-orders-mapped.sql"

# PostgreSQL columns: OrderId, ClientId, OrderDate, Status, SubTotal, Tax, Discount, Total, 
#                     ShippingAddress, ShippingCity, ShippingState, ShippingCountry, ShippingPostalCode, Notes, CreatedAt, UpdatedAt

# Map SQL Server Status (int) to PostgreSQL Status (string)
# SQL Server: 0=Pending, 1=Processing, 2=Shipped, 3=Delivered, 4=Cancelled
$statusMap = @{
    0 = 'Pending'
    1 = 'Processing'
    2 = 'Shipped'
    3 = 'Delivered'
    4 = 'Cancelled'
}

$query = @"
SELECT 
    OrderId,
    ClientId,
    CreatedAt as OrderDate,
    Status,
    Total * 0.84 as SubTotal,  -- Estimate: Total without tax (assuming 16% tax)
    Total * 0.16 as Tax,
    0.00 as Discount,
    Total,
    ShippingAddressLine1 as ShippingAddress,
    ShippingCity,
    ShippingState,
    ShippingCountry,
    ShippingPostalCode,
    CancellationReason as Notes,
    CreatedAt,
    UpdatedAt
FROM [Order].[Orders]
"@

$data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535

function Escape-String {
    param([string]$value)
    if ([string]::IsNullOrEmpty($value)) { return "NULL" }
    $escaped = $value -replace "'", "''"
    return "'$escaped'"
}

$inserts = @()
$inserts += '-- Data for "Order"."Orders"'
$inserts += '-- Migrated from SQL Server with column mapping'
$inserts += ''

foreach ($row in $data) {
    $orderId = $row.OrderId
    $clientId = $row.ClientId
    $orderDate = if ($row.OrderDate) { "'" + $row.OrderDate.ToString('yyyy-MM-dd HH:mm:ss.fff') + "'" } else { 'NOW()' }
    $status = Escape-String $statusMap[[int]$row.Status]
    $subTotal = [math]::Round($row.SubTotal, 2)
    $tax = [math]::Round($row.Tax, 2)
    $discount = 0.00
    $total = [math]::Round($row.Total, 2)
    $shippingAddress = Escape-String $row.ShippingAddress
    $shippingCity = Escape-String $row.ShippingCity
    $shippingState = Escape-String $row.ShippingState
    $shippingCountry = Escape-String $row.ShippingCountry
    $shippingPostalCode = Escape-String $row.ShippingPostalCode
    $notes = Escape-String $row.Notes
    $createdAt = if ($row.CreatedAt -and $row.CreatedAt -isnot [System.DBNull]) { "'" + ([datetime]$row.CreatedAt).ToString('yyyy-MM-dd HH:mm:ss.fff') + "'" } else { 'NOW()' }
    $updatedAt = if ($row.UpdatedAt -and $row.UpdatedAt -isnot [System.DBNull]) { "'" + ([datetime]$row.UpdatedAt).ToString('yyyy-MM-dd HH:mm:ss.fff') + "'" } else { 'NULL' }
    
    $inserts += "INSERT INTO `"Order`".`"Orders`" (`"OrderId`", `"ClientId`", `"OrderDate`", `"Status`", `"SubTotal`", `"Tax`", `"Discount`", `"Total`", `"ShippingAddress`", `"ShippingCity`", `"ShippingState`", `"ShippingCountry`", `"ShippingPostalCode`", `"Notes`", `"CreatedAt`", `"UpdatedAt`") VALUES ($orderId, $clientId, $orderDate, $status, $subTotal, $tax, $discount, $total, $shippingAddress, $shippingCity, $shippingState, $shippingCountry, $shippingPostalCode, $notes, $createdAt, $updatedAt);"
}

# Add sequence reset
$maxId = ($data | Measure-Object -Property OrderId -Maximum).Maximum
$inserts += ""
$inserts += "-- Reset sequence"
$inserts += "SELECT setval('`"Order`".`"Orders_OrderId_seq`"', $($maxId + 1));"

$inserts | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Saved $($data.Count) orders to $OutputFile"

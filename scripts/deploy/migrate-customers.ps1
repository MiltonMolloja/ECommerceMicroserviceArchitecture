# Script to migrate Customer.Clients from SQL Server to PostgreSQL
# Maps different schema structures

$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputFile = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\30-customer-clients-mapped.sql"

$query = @"
SELECT 
    c.ClientId,
    ISNULL(u.FirstName + ' ' + u.LastName, 'Unknown') as Name,
    u.Email,
    ISNULL(c.Phone, c.MobilePhone) as Phone,
    a.AddressLine1 as Address,
    a.City,
    a.State,
    a.Country,
    a.PostalCode,
    c.IsActive,
    c.CreatedAt,
    c.UpdatedAt
FROM [Customer].[Clients] c
LEFT JOIN [Identity].[AspNetUsers] u ON c.UserId = u.Id
LEFT JOIN (
    SELECT ClientId, AddressLine1, City, State, Country, PostalCode,
           ROW_NUMBER() OVER (PARTITION BY ClientId ORDER BY AddressId) as rn
    FROM [Customer].[ClientAddresses]
    WHERE AddressType IN ('Shipping', 'Both')
) a ON c.ClientId = a.ClientId AND a.rn = 1
"@

$data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535

function Escape-String {
    param([string]$value)
    if ([string]::IsNullOrEmpty($value)) { return "NULL" }
    $escaped = $value -replace "'", "''"
    return "'$escaped'"
}

$inserts = @()
$inserts += '-- Data for "Customer"."Clients"'
$inserts += '-- Migrated from SQL Server with column mapping'
$inserts += ''

foreach ($row in $data) {
    $clientId = $row.ClientId
    $name = Escape-String $row.Name
    $email = Escape-String $row.Email
    $phone = Escape-String $row.Phone
    $address = Escape-String $row.Address
    $city = Escape-String $row.City
    $state = Escape-String $row.State
    $country = Escape-String $row.Country
    $postalCode = Escape-String $row.PostalCode
    $isActive = if ($row.IsActive) { 'true' } else { 'false' }
    $createdAt = if ($row.CreatedAt) { "'" + $row.CreatedAt.ToString('yyyy-MM-dd HH:mm:ss.fff') + "'" } else { 'NOW()' }
    $updatedAt = if ($row.UpdatedAt) { "'" + $row.UpdatedAt.ToString('yyyy-MM-dd HH:mm:ss.fff') + "'" } else { 'NULL' }
    
    $inserts += "INSERT INTO `"Customer`".`"Clients`" (`"ClientId`", `"Name`", `"Email`", `"Phone`", `"Address`", `"City`", `"State`", `"Country`", `"PostalCode`", `"IsActive`", `"CreatedAt`", `"UpdatedAt`") VALUES ($clientId, $name, $email, $phone, $address, $city, $state, $country, $postalCode, $isActive, $createdAt, $updatedAt);"
}

# Add sequence reset
$maxId = ($data | Measure-Object -Property ClientId -Maximum).Maximum
$inserts += ""
$inserts += "-- Reset sequence"
$inserts += "SELECT setval('`"Customer`".`"Clients_ClientId_seq`"', $($maxId + 1));"

$inserts | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Saved $($data.Count) clients to $OutputFile"

# Script to export Products without Stock column
$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputFile = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\03-catalog-products-fixed.sql"

# Columns to export (excluding Stock which doesn't exist in PostgreSQL)
$columns = @('ProductId', 'Price', 'NameSpanish', 'NameEnglish', 'DescriptionSpanish', 'DescriptionEnglish', 'SKU', 'Brand', 'Slug', 'OriginalPrice', 'DiscountPercentage', 'TaxRate', 'Images', 'MetaTitle', 'MetaDescription', 'MetaKeywords', 'IsActive', 'IsFeatured', 'CreatedAt', 'UpdatedAt', 'BrandId', 'TotalSold')

$selectCols = $columns -join ', '
$query = "SELECT $selectCols FROM [Catalog].[Products]"
$data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535

$inserts = @()
$inserts += '-- Data for "Catalog"."Products"'
$inserts += '-- Exported from SQL Server [Catalog].[Products] (without Stock column)'
$inserts += ''

foreach ($row in $data) {
    $values = @()
    foreach ($col in $columns) {
        $value = $row.$col
        if ($null -eq $value -or $value -is [System.DBNull]) {
            $values += 'NULL'
        } elseif ($col -match 'Id|Price|Percentage|Rate|TotalSold') {
            $values += $value.ToString()
        } elseif ($col -match 'IsActive|IsFeatured') {
            if ($value -eq $true -or $value -eq 1) { $values += 'true' } else { $values += 'false' }
        } elseif ($col -match 'At$') {
            $values += "'" + $value.ToString('yyyy-MM-dd HH:mm:ss.fff') + "'"
        } else {
            $escaped = $value.ToString() -replace "'", "''"
            $values += "'$escaped'"
        }
    }
    $colNames = ($columns | ForEach-Object { "`"$_`"" }) -join ', '
    $valuesStr = $values -join ', '
    $inserts += "INSERT INTO `"Catalog`".`"Products`" ($colNames) VALUES ($valuesStr);"
}

$inserts | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Saved $($data.Count) products to $OutputFile"

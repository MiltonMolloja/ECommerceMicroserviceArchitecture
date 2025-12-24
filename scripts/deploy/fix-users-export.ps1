# Script to export AspNetUsers without PasswordChangedAt column
$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputFile = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\21-identity-aspnetusers-fixed.sql"

# Columns to export (matching PostgreSQL schema, reordered to match PostgreSQL)
$columns = @('Id', 'FirstName', 'LastName', 'UserName', 'NormalizedUserName', 'Email', 'NormalizedEmail', 'EmailConfirmed', 'PasswordHash', 'SecurityStamp', 'ConcurrencyStamp', 'PhoneNumber', 'PhoneNumberConfirmed', 'TwoFactorEnabled', 'LockoutEnd', 'LockoutEnabled', 'AccessFailedCount')

$selectCols = $columns -join ', '
$query = "SELECT $selectCols FROM [Identity].[AspNetUsers]"
$data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535

$inserts = @()
$inserts += '-- Data for "Identity"."AspNetUsers"'
$inserts += '-- Exported from SQL Server [Identity].[AspNetUsers]'
$inserts += ''

foreach ($row in $data) {
    $values = @()
    foreach ($col in $columns) {
        $value = $row.$col
        if ($null -eq $value -or $value -is [System.DBNull]) {
            $values += 'NULL'
        } elseif ($col -eq 'AccessFailedCount') {
            $values += $value.ToString()
        } elseif ($col -match 'Confirmed|Enabled') {
            if ($value -eq $true -or $value -eq 1) { $values += 'true' } else { $values += 'false' }
        } elseif ($col -eq 'LockoutEnd' -and $value) {
            $values += "'" + $value.ToString('yyyy-MM-dd HH:mm:ss.fff') + "'"
        } else {
            $escaped = $value.ToString() -replace "'", "''"
            $values += "'$escaped'"
        }
    }
    $colNames = ($columns | ForEach-Object { "`"$_`"" }) -join ', '
    $valuesStr = $values -join ', '
    $inserts += "INSERT INTO `"Identity`".`"AspNetUsers`" ($colNames) VALUES ($valuesStr);"
}

$inserts | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Saved $($data.Count) users to $OutputFile"

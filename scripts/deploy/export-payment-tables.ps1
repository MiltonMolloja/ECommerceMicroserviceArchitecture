# Export Payment tables
$SqlConnStr = 'Server=localhost\SQLEXPRESS;Database=ECommerceDb;Integrated Security=True;TrustServerCertificate=True'
$conn = New-Object System.Data.SqlClient.SqlConnection($SqlConnStr)
$conn.Open()

function Escape-Pg { param([string]$v) if ($null -eq $v) { "NULL" } else { "'" + $v.Replace("'", "''") + "'" } }

# Payments
$cmd = $conn.CreateCommand()
$cmd.CommandText = 'SELECT * FROM [Payment].Payments'
$adapter = New-Object System.Data.SqlClient.SqlDataAdapter($cmd)
$ds = New-Object System.Data.DataSet
$adapter.Fill($ds) | Out-Null

$output = @()
foreach ($row in $ds.Tables[0].Rows) {
    $sql = 'INSERT INTO "Payment"."Payments" ("PaymentId", "OrderId", "UserId", "Amount", "Currency", "Status", "PaymentMethod", "TransactionId", "PaymentGateway", "PaymentDate", "CreatedAt", "UpdatedAt") VALUES ('
    $sql += $row.PaymentId.ToString() + ', '
    $sql += $row.OrderId.ToString() + ', '
    $sql += $row.UserId.ToString() + ', '
    $sql += $row.Amount.ToString() + ', '
    $sql += (Escape-Pg $row.Currency) + ', '
    $sql += $row.Status.ToString() + ', '
    $sql += $row.PaymentMethod.ToString() + ', '
    $sql += (Escape-Pg $row.TransactionId) + ', '
    $sql += (Escape-Pg $row.PaymentGateway) + ', '
    $sql += $(if ($row.PaymentDate -eq [DBNull]::Value) { 'NULL' } else { "'" + $row.PaymentDate.ToString('yyyy-MM-ddTHH:mm:ss') + "'" }) + ', '
    $sql += "'" + $row.CreatedAt.ToString('yyyy-MM-ddTHH:mm:ss') + "', "
    $sql += "'" + $row.UpdatedAt.ToString('yyyy-MM-ddTHH:mm:ss') + "'"
    $sql += ');'
    $output += $sql
}
$output | Out-File -FilePath 'C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\60-payment-payments.sql' -Encoding UTF8
Write-Host "Payments: $($ds.Tables[0].Rows.Count) rows"

# PaymentDetails
$cmd.CommandText = 'SELECT * FROM [Payment].PaymentDetails'
$ds = New-Object System.Data.DataSet
$adapter.SelectCommand = $cmd
$adapter.Fill($ds) | Out-Null

$output = @()
foreach ($row in $ds.Tables[0].Rows) {
    $sql = 'INSERT INTO "Payment"."PaymentDetails" ("PaymentDetailId", "PaymentId", "CardLast4Digits", "CardBrand", "ExpiryMonth", "ExpiryYear", "BillingAddress", "BillingCity", "BillingCountry", "BillingZipCode") VALUES ('
    $sql += $row.PaymentDetailId.ToString() + ', '
    $sql += $row.PaymentId.ToString() + ', '
    $sql += (Escape-Pg $row.CardLast4Digits) + ', '
    $sql += (Escape-Pg $row.CardBrand) + ', '
    $sql += $(if ($row.ExpiryMonth -eq [DBNull]::Value) { 'NULL' } else { $row.ExpiryMonth.ToString() }) + ', '
    $sql += $(if ($row.ExpiryYear -eq [DBNull]::Value) { 'NULL' } else { $row.ExpiryYear.ToString() }) + ', '
    $sql += (Escape-Pg $row.BillingAddress) + ', '
    $sql += (Escape-Pg $row.BillingCity) + ', '
    $sql += (Escape-Pg $row.BillingCountry) + ', '
    $sql += (Escape-Pg $row.BillingZipCode)
    $sql += ');'
    $output += $sql
}
$output | Out-File -FilePath 'C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\61-payment-paymentdetails.sql' -Encoding UTF8
Write-Host "PaymentDetails: $($ds.Tables[0].Rows.Count) rows"

# PaymentTransactions
$cmd.CommandText = 'SELECT * FROM [Payment].PaymentTransactions'
$ds = New-Object System.Data.DataSet
$adapter.SelectCommand = $cmd
$adapter.Fill($ds) | Out-Null

$output = @()
foreach ($row in $ds.Tables[0].Rows) {
    $sql = 'INSERT INTO "Payment"."PaymentTransactions" ("TransactionId", "PaymentId", "TransactionType", "Amount", "Status", "GatewayResponse", "ErrorMessage", "TransactionDate", "IPAddress") VALUES ('
    $sql += $row.TransactionId.ToString() + ', '
    $sql += $row.PaymentId.ToString() + ', '
    $sql += $row.TransactionType.ToString() + ', '
    $sql += $row.Amount.ToString() + ', '
    $sql += $row.Status.ToString() + ', '
    $sql += $(if ($row.GatewayResponse -eq [DBNull]::Value) { 'NULL' } else { (Escape-Pg $row.GatewayResponse) }) + ', '
    $sql += $(if ($row.ErrorMessage -eq [DBNull]::Value) { 'NULL' } else { (Escape-Pg $row.ErrorMessage) }) + ', '
    $sql += "'" + $row.TransactionDate.ToString('yyyy-MM-ddTHH:mm:ss') + "', "
    $sql += $(if ($row.IPAddress -eq [DBNull]::Value) { 'NULL' } else { (Escape-Pg $row.IPAddress) })
    $sql += ');'
    $output += $sql
}
$output | Out-File -FilePath 'C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\62-payment-paymenttransactions.sql' -Encoding UTF8
Write-Host "PaymentTransactions: $($ds.Tables[0].Rows.Count) rows"

$conn.Close()
Write-Host "Done!"

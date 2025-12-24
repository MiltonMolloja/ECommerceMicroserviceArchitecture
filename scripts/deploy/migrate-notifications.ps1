# Script to migrate Notification tables from SQL Server to PostgreSQL

$SqlServer = "localhost\SQLEXPRESS"
$Database = "ECommerceDb"
$OutputFile = "C:\Source\ECommerceMicroserviceArchitecture\scripts\deploy\migration-data\71-notification-notifications-mapped.sql"

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

# Get user emails for mapping UserId -> Recipient
$usersQuery = "SELECT Id, Email FROM [Identity].[AspNetUsers]"
$users = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $usersQuery
$userEmails = @{}
foreach ($u in $users) {
    $userEmails[$u.Id] = $u.Email
}

# Migrate Notifications
# SQL Server: NotificationId, UserId, Type, Title, Message, Data, IsRead, ReadAt, Priority, ExpiresAt, CreatedAt
# PostgreSQL: NotificationId, ClientId, Type, Channel, Recipient, Subject, Body, Status, ErrorMessage, SentAt, CreatedAt

$query = @"
SELECT 
    n.NotificationId,
    c.ClientId,
    n.Type,
    n.Title,
    n.Message,
    n.IsRead,
    n.ReadAt,
    n.CreatedAt,
    u.Email
FROM [Notification].[Notifications] n
LEFT JOIN [Identity].[AspNetUsers] u ON n.UserId = u.Id
LEFT JOIN [Customer].[Clients] c ON c.UserId = u.Id
"@

$data = Invoke-Sqlcmd -ServerInstance $SqlServer -Database $Database -Query $query -MaxCharLength 65535

$inserts = @()
$inserts += '-- Data for "Notification"."Notifications"'
$inserts += '-- Migrated from SQL Server [Notification].[Notifications]'
$inserts += ''

foreach ($row in $data) {
    $notificationId = $row.NotificationId
    $clientId = if ($row.ClientId) { $row.ClientId } else { 'NULL' }
    $type = Escape-String $row.Type
    $channel = "'Email'"  # Default channel
    $recipient = Escape-String $row.Email
    $subject = Escape-String $row.Title
    $body = Escape-String $row.Message
    $status = if ($row.IsRead) { "'Sent'" } else { "'Pending'" }
    $errorMessage = 'NULL'
    $sentAt = Format-DateTime $row.ReadAt
    $createdAt = Format-DateTime $row.CreatedAt
    
    $inserts += "INSERT INTO `"Notification`".`"Notifications`" (`"NotificationId`", `"ClientId`", `"Type`", `"Channel`", `"Recipient`", `"Subject`", `"Body`", `"Status`", `"ErrorMessage`", `"SentAt`", `"CreatedAt`") VALUES ($notificationId, $clientId, $type, $channel, $recipient, $subject, $body, $status, $errorMessage, $sentAt, $createdAt);"
}

if ($data.Count -gt 0) {
    $maxId = ($data | Measure-Object -Property NotificationId -Maximum).Maximum
    $inserts += ""
    $inserts += "SELECT setval('`"Notification`".`"Notifications_NotificationId_seq`"', $($maxId + 1));"
}

$inserts | Out-File -FilePath $OutputFile -Encoding UTF8
Write-Host "Saved $($data.Count) notifications to $OutputFile"

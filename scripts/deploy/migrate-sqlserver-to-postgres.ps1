# Migration Script: SQL Server to PostgreSQL
# ECommerce Database Migration

$SqlServer = "localhost\SQLEXPRESS"
$SqlDatabase = "ECommerceDb"
$PgConnectionString = "postgresql://postgres:3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ2@72.61.128.126:5433/ecommerce"

function Invoke-PgSql {
    param([string]$Query)
    docker run --rm postgres:15 psql $PgConnectionString -c $Query
}

function Export-SqlServerTable {
    param(
        [string]$Schema,
        [string]$Table,
        [string]$Columns
    )
    $query = "SET NOCOUNT ON; SELECT $Columns FROM [$Schema].[$Table]"
    sqlcmd -S $SqlServer -d $SqlDatabase -Q $query -E -C -W -h -1 -s"|"
}

Write-Host "Starting migration..." -ForegroundColor Green

# The actual data migration will be done via direct SQL inserts
Write-Host "Migration script created. Run individual migration commands." -ForegroundColor Yellow

# Migration Script: SQL Server to PostgreSQL
# ECommerce Database Migration
#
# IMPORTANT: Set environment variables before running:
#   $env:PG_CONNECTION_STRING = "postgresql://user:password@host:port/database"
#   $env:SQL_SERVER = "localhost\SQLEXPRESS"
#   $env:SQL_DATABASE = "ECommerceDb"

$SqlServer = $env:SQL_SERVER ?? "localhost\SQLEXPRESS"
$SqlDatabase = $env:SQL_DATABASE ?? "ECommerceDb"
$PgConnectionString = $env:PG_CONNECTION_STRING

if (-not $PgConnectionString) {
    Write-Host "ERROR: PG_CONNECTION_STRING environment variable is required." -ForegroundColor Red
    Write-Host "Set it with: `$env:PG_CONNECTION_STRING = 'postgresql://user:password@host:port/database'" -ForegroundColor Yellow
    exit 1
}

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

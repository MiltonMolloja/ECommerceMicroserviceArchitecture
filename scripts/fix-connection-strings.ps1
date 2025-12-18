# Fix Connection Strings in User Secrets
# This script updates the connection string for all services

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "Fixing Connection Strings..." -ForegroundColor Cyan
Write-Host ""

$services = @(
    "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj",
    "src/Services/Cart/Cart.Api/Cart.Api.csproj",
    "src/Services/Customer/Customer.Api/Customer.Api.csproj",
    "src/Services/Identity/Identity.Api/Identity.Api.csproj",
    "src/Services/Order/Order.Api/Order.Api.csproj",
    "src/Services/Payment/Payment.Api/Payment.Api.csproj",
    "src/Services/Notification/Notification.Api/Notification.Api.csproj",
    "src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj"
)

$connectionString = "Server=localhost\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Application Name=ECommerce"

foreach ($service in $services) {
    $serviceName = Split-Path -Leaf (Split-Path -Parent $service)
    Write-Host "Updating $serviceName..." -ForegroundColor Yellow
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $connectionString --project $service | Out-Null
    Write-Host "  OK" -ForegroundColor Green
}

Write-Host ""
Write-Host "Done! All connection strings updated." -ForegroundColor Green
Write-Host ""

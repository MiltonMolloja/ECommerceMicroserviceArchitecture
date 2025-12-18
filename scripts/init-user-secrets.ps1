# Simple User Secrets Initialization Script
# Run this to initialize User Secrets for all services

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Initializing User Secrets" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

$services = @{
    "Catalog.Api" = "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj"
    "Cart.Api" = "src/Services/Cart/Cart.Api/Cart.Api.csproj"
    "Customer.Api" = "src/Services/Customer/Customer.Api/Customer.Api.csproj"
    "Identity.Api" = "src/Services/Identity/Identity.Api/Identity.Api.csproj"
    "Order.Api" = "src/Services/Order/Order.Api/Order.Api.csproj"
    "Payment.Api" = "src/Services/Payment/Payment.Api/Payment.Api.csproj"
    "Notification.Api" = "src/Services/Notification/Notification.Api/Notification.Api.csproj"
    "Gateway" = "src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj"
}

foreach ($service in $services.Keys) {
    Write-Host "Initializing $service..." -ForegroundColor Yellow
    try {
        dotnet user-secrets init --project $services[$service]
        Write-Host "  OK - $service initialized" -ForegroundColor Green
    }
    catch {
        Write-Host "  ERROR - Failed to initialize $service" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Done! User Secrets initialized for all services." -ForegroundColor Green
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "1. Set secrets: dotnet user-secrets set 'Key' 'Value' --project <path>" -ForegroundColor Gray
Write-Host "2. List secrets: dotnet user-secrets list --project <path>" -ForegroundColor Gray
Write-Host "3. Or run: .\scripts\set-default-secrets.ps1" -ForegroundColor Gray
Write-Host ""

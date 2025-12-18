# Set Default User Secrets for Development
# This script sets common development secrets for all services

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host "  Setting Default User Secrets" -ForegroundColor Cyan
Write-Host "=============================================" -ForegroundColor Cyan
Write-Host ""

# Generate random values for security
$randomKey = -join ((65..90) + (97..122) + (48..57) | Get-Random -Count 32 | ForEach-Object {[char]$_})
$apiKey = "dev-api-key-" + (Get-Random -Maximum 99999)

# Common secrets for all services
$commonSecrets = @{
    "SecretKey" = $randomKey
    "ConnectionStrings:DefaultConnection" = "Server=localhost\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Application Name=ECommerce"
    "Redis:ConnectionString" = "localhost:6379"
    "ApiKey:ApiKey" = $apiKey
    "RabbitMQ:Host" = "localhost"
    "RabbitMQ:Port" = "5672"
    "RabbitMQ:Username" = "guest"
    "RabbitMQ:Password" = "guest"
}

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

# Set common secrets for all services
foreach ($service in $services.Keys) {
    Write-Host "Configuring $service..." -ForegroundColor Yellow
    
    foreach ($key in $commonSecrets.Keys) {
        dotnet user-secrets set $key $commonSecrets[$key] --project $services[$service] | Out-Null
    }
    
    Write-Host "  OK - Common secrets set" -ForegroundColor Green
}

# Service-specific secrets
Write-Host ""
Write-Host "Setting service-specific secrets..." -ForegroundColor Yellow

# Notification.Api - SMTP settings
Write-Host "  Notification.Api - SMTP settings" -ForegroundColor Gray
dotnet user-secrets set "SmtpSettings:Host" "smtp.gmail.com" --project $services["Notification.Api"] | Out-Null
dotnet user-secrets set "SmtpSettings:Port" "587" --project $services["Notification.Api"] | Out-Null
dotnet user-secrets set "SmtpSettings:Username" "your-email@gmail.com" --project $services["Notification.Api"] | Out-Null
dotnet user-secrets set "SmtpSettings:Password" "your-app-password" --project $services["Notification.Api"] | Out-Null
dotnet user-secrets set "SmtpSettings:FromEmail" "noreply@yourdomain.com" --project $services["Notification.Api"] | Out-Null
dotnet user-secrets set "SmtpSettings:FromName" "ECommerce Platform" --project $services["Notification.Api"] | Out-Null
dotnet user-secrets set "SmtpSettings:EnableSsl" "true" --project $services["Notification.Api"] | Out-Null

# Payment.Api - Payment gateway settings
Write-Host "  Payment.Api - Payment gateway settings" -ForegroundColor Gray
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_stripe_secret_key" --project $services["Payment.Api"] | Out-Null
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_your_stripe_publishable_key" --project $services["Payment.Api"] | Out-Null
dotnet user-secrets set "MercadoPago:AccessToken" "your_mercadopago_access_token" --project $services["Payment.Api"] | Out-Null
dotnet user-secrets set "MercadoPago:PublicKey" "your_mercadopago_public_key" --project $services["Payment.Api"] | Out-Null

Write-Host ""
Write-Host "=============================================" -ForegroundColor Green
Write-Host "  User Secrets Configured Successfully!" -ForegroundColor Green
Write-Host "=============================================" -ForegroundColor Green
Write-Host ""
Write-Host "Generated Values:" -ForegroundColor Cyan
Write-Host "  JWT SecretKey: $randomKey" -ForegroundColor Gray
Write-Host "  API Key: $apiKey" -ForegroundColor Gray
Write-Host ""
Write-Host "IMPORTANT: Update these placeholder values:" -ForegroundColor Yellow
Write-Host "  1. SMTP settings in Notification.Api (for email)" -ForegroundColor Gray
Write-Host "  2. Payment gateway keys in Payment.Api" -ForegroundColor Gray
Write-Host "  3. Database connection string if not using localhost\SQLEXPRESS" -ForegroundColor Gray
Write-Host ""
Write-Host "To update a secret:" -ForegroundColor Cyan
Write-Host '  dotnet user-secrets set "Key" "Value" --project <path>' -ForegroundColor Gray
Write-Host ""
Write-Host "To list secrets for a service:" -ForegroundColor Cyan
Write-Host '  dotnet user-secrets list --project src/Services/Order/Order.Api/Order.Api.csproj' -ForegroundColor Gray
Write-Host ""

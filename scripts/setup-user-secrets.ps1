# =============================================
# Script: setup-user-secrets.ps1
# Description: Configures User Secrets for all microservices
# Author: DBA Expert
# Date: 2024
# =============================================

param(
    [switch]$Init,
    [switch]$SetDefaults,
    [switch]$List,
    [switch]$Clear,
    [string]$Service = "all"
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# Service paths
$services = @{
    "Catalog" = "src/Services/Catalog/Catalog.Api/Catalog.Api.csproj"
    "Cart" = "src/Services/Cart/Cart.Api/Cart.Api.csproj"
    "Customer" = "src/Services/Customer/Customer.Api/Customer.Api.csproj"
    "Identity" = "src/Services/Identity/Identity.Api/Identity.Api.csproj"
    "Order" = "src/Services/Order/Order.Api/Order.Api.csproj"
    "Payment" = "src/Services/Payment/Payment.Api/Payment.Api.csproj"
    "Notification" = "src/Services/Notification/Notification.Api/Notification.Api.csproj"
    "Gateway" = "src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj"
}

function Initialize-UserSecrets {
    param([string]$ServiceName, [string]$ProjectPath)
    
    Write-Info "Initializing User Secrets for $ServiceName..."
    
    try {
        dotnet user-secrets init --project $ProjectPath
        Write-Success "  ✓ User Secrets initialized for $ServiceName"
    }
    catch {
        Write-Error "  ✗ Failed to initialize User Secrets for $ServiceName"
    }
}

function Set-DefaultSecrets {
    param([string]$ServiceName, [string]$ProjectPath)
    
    Write-Info "Setting default secrets for $ServiceName..."
    
    # Common secrets for all services
    $secrets = @{
        "SecretKey" = "your-super-secure-jwt-secret-key-min-32-chars-$(Get-Random)"
        "ConnectionStrings:DefaultConnection" = "Server=localhost\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Application Name=ECommerce"
        "Redis:ConnectionString" = "localhost:6379"
        "ApiKey:ApiKey" = "dev-api-key-$(Get-Random)"
        "RabbitMQ:Host" = "localhost"
        "RabbitMQ:Port" = "5672"
        "RabbitMQ:Username" = "guest"
        "RabbitMQ:Password" = "guest"
    }
    
    # Service-specific secrets
    if ($ServiceName -eq "Notification") {
        $secrets["SmtpSettings:Host"] = "smtp.gmail.com"
        $secrets["SmtpSettings:Port"] = "587"
        $secrets["SmtpSettings:Username"] = "your-email@gmail.com"
        $secrets["SmtpSettings:Password"] = "your-app-password"
        $secrets["SmtpSettings:FromEmail"] = "noreply@yourdomain.com"
        $secrets["SmtpSettings:FromName"] = "ECommerce Platform"
    }
    
    if ($ServiceName -eq "Payment") {
        $secrets["Stripe:SecretKey"] = "sk_test_your_stripe_secret_key"
        $secrets["Stripe:PublishableKey"] = "pk_test_your_stripe_publishable_key"
        $secrets["MercadoPago:AccessToken"] = "your_mercadopago_access_token"
    }
    
    foreach ($key in $secrets.Keys) {
        try {
            dotnet user-secrets set $key $secrets[$key] --project $ProjectPath | Out-Null
            Write-Host "  ✓ Set: $key" -ForegroundColor Gray
        }
        catch {
            Write-Warning "  ✗ Failed to set: $key"
        }
    }
    
    Write-Success "  ✓ Default secrets configured for $ServiceName"
}

function List-UserSecrets {
    param([string]$ServiceName, [string]$ProjectPath)
    
    Write-Info "`n$ServiceName Secrets:"
    Write-Host "=============================================" -ForegroundColor Cyan
    
    try {
        dotnet user-secrets list --project $ProjectPath
    }
    catch {
        Write-Warning "  No secrets configured for $ServiceName"
    }
}

function Clear-UserSecrets {
    param([string]$ServiceName, [string]$ProjectPath)
    
    Write-Warning "Clearing all secrets for $ServiceName..."
    
    try {
        dotnet user-secrets clear --project $ProjectPath
        Write-Success "  ✓ Secrets cleared for $ServiceName"
    }
    catch {
        Write-Error "  ✗ Failed to clear secrets for $ServiceName"
    }
}

function Show-Usage {
    Write-Host @"
User Secrets Setup Script
==========================

Usage:
  .\setup-user-secrets.ps1 [-Init] [-SetDefaults] [-List] [-Clear] [-Service <name>]

Parameters:
  -Init          Initialize User Secrets for services
  -SetDefaults   Set default development secrets
  -List          List all configured secrets
  -Clear         Clear all secrets (use with caution!)
  -Service       Target specific service (default: all)
                 Options: Catalog, Cart, Customer, Identity, Order, Payment, Notification, Gateway, all

Examples:
  # Initialize all services
  .\setup-user-secrets.ps1 -Init

  # Initialize and set defaults for all services
  .\setup-user-secrets.ps1 -Init -SetDefaults

  # Set defaults for Order service only
  .\setup-user-secrets.ps1 -SetDefaults -Service Order

  # List secrets for all services
  .\setup-user-secrets.ps1 -List

  # List secrets for specific service
  .\setup-user-secrets.ps1 -List -Service Payment

  # Clear secrets for all services
  .\setup-user-secrets.ps1 -Clear

Quick Start:
  1. Run: .\setup-user-secrets.ps1 -Init -SetDefaults
  2. Edit secrets: dotnet user-secrets set "Key" "Value" --project <path>
  3. List secrets: .\setup-user-secrets.ps1 -List

Important Notes:
  - User Secrets are stored in your user profile (NOT in the project)
  - Windows: %APPDATA%\Microsoft\UserSecrets\<id>\secrets.json
  - Linux/Mac: ~/.microsoft/usersecrets/<id>/secrets.json
  - These secrets are ONLY for development, use environment variables in production
  - Update the default values with your actual credentials

"@
}

# Main execution
Write-Host ""
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host "  User Secrets Configuration Script" -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host ""

# Determine which services to process
$servicesToProcess = @{}
if ($Service -eq "all") {
    $servicesToProcess = $services
}
elseif ($services.ContainsKey($Service)) {
    $servicesToProcess[$Service] = $services[$Service]
}
else {
    Write-Error "Invalid service name: $Service"
    Write-Info "Valid services: $($services.Keys -join ', '), all"
    exit 1
}

# Execute requested actions
if ($Init) {
    Write-Info "Initializing User Secrets..."
    Write-Host ""
    foreach ($svc in $servicesToProcess.Keys) {
        Initialize-UserSecrets -ServiceName $svc -ProjectPath $servicesToProcess[$svc]
    }
    Write-Host ""
}

if ($SetDefaults) {
    Write-Info "Setting default secrets..."
    Write-Host ""
    foreach ($svc in $servicesToProcess.Keys) {
        Set-DefaultSecrets -ServiceName $svc -ProjectPath $servicesToProcess[$svc]
        Write-Host ""
    }
    
    Write-Warning "`nIMPORTANT: Update these secrets with your actual credentials!"
    Write-Info "Edit secrets with: dotnet user-secrets set `"Key`" `"Value`" --project <path>"
}

if ($List) {
    foreach ($svc in $servicesToProcess.Keys) {
        List-UserSecrets -ServiceName $svc -ProjectPath $servicesToProcess[$svc]
    }
}

if ($Clear) {
    Write-Warning "`nThis will delete ALL secrets for the selected services!"
    $confirmation = Read-Host "Are you sure? (yes/no)"
    if ($confirmation -eq "yes") {
        foreach ($svc in $servicesToProcess.Keys) {
            Clear-UserSecrets -ServiceName $svc -ProjectPath $servicesToProcess[$svc]
        }
    }
    else {
        Write-Info "Operation cancelled."
    }
}

if (-not $Init -and -not $SetDefaults -and -not $List -and -not $Clear) {
    Show-Usage
}

Write-Host ""
Write-Success "Done!"
Write-Host ""

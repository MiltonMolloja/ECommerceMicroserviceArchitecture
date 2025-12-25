# ============================================================================
# Script para inicializar User Secrets con credenciales del VPS
# Ejecutar desde la raiz del proyecto: .\scripts\init-user-secrets-vps.ps1
# ============================================================================

$ErrorActionPreference = "Stop"

# Credenciales del VPS PostgreSQL
$PostgresPassword = "3jxEbemom6JTy9dqbrpAoAlNfUVpzmbQ"
$ConnectionString = "Host=72.61.128.126;Port=5432;Database=ecommerce;Username=postgres;Password=$PostgresPassword"

# Lista de proyectos API
$projects = @(
    "src\Services\Identity\Identity.Api\Identity.Api.csproj",
    "src\Services\Catalog\Catalog.Api\Catalog.Api.csproj",
    "src\Services\Customer\Customer.Api\Customer.Api.csproj",
    "src\Services\Order\Order.Api\Order.Api.csproj",
    "src\Services\Cart\Cart.Api\Cart.Api.csproj",
    "src\Services\Payment\Payment.Api\Payment.Api.csproj",
    "src\Services\Notification\Notification.Api\Notification.Api.csproj"
)

Write-Host "============================================" -ForegroundColor Cyan
Write-Host "Inicializando User Secrets para VPS PostgreSQL" -ForegroundColor Cyan
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""

foreach ($project in $projects) {
    $projectName = Split-Path $project -Leaf
    Write-Host "Configurando: $projectName" -ForegroundColor Yellow
    
    # Establecer el connection string
    dotnet user-secrets set "ConnectionStrings:DefaultConnection" $ConnectionString --project $project
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  [OK] Connection string configurado" -ForegroundColor Green
    } else {
        Write-Host "  [ERROR] Fallo al configurar $projectName" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "============================================" -ForegroundColor Cyan
Write-Host "User Secrets configurados exitosamente!" -ForegroundColor Green
Write-Host "============================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Los secrets se guardaron en:" -ForegroundColor Gray
Write-Host "  %APPDATA%\Microsoft\UserSecrets\<project-guid>\secrets.json" -ForegroundColor Gray
Write-Host ""
Write-Host "Ahora puedes ejecutar cualquier microservicio y se conectara al VPS PostgreSQL." -ForegroundColor White

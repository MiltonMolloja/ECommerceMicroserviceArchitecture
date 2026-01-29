# ===========================================
# SonarQube Local Analysis Script (PowerShell)
# E-Commerce Microservices Architecture
# ===========================================
# 
# USO:
#   .\scripts\sonar-scan.ps1
#   .\scripts\sonar-scan.ps1 -SonarUrl "http://localhost:9000" -SonarToken "your-token"
#
# PREREQUISITOS:
#   1. Docker Desktop corriendo con SonarQube
#   2. .NET SDK 8.0+
#   3. Token de SonarQube generado
#
# ===========================================

param(
    [string]$SonarUrl = "http://localhost:9000",
    [string]$SonarToken = "",
    [string]$ProjectKey = "ECommerceMicroserviceArchitecture",
    [switch]$SkipTests = $false,
    [switch]$Verbose = $false
)

# Colores para output
function Write-Step { param($Message) Write-Host "`n==> $Message" -ForegroundColor Cyan }
function Write-Success { param($Message) Write-Host "[OK] $Message" -ForegroundColor Green }
function Write-Warning { param($Message) Write-Host "[WARN] $Message" -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host "[ERROR] $Message" -ForegroundColor Red }

# Banner
Write-Host @"

  ____                        ___            _           _     
 / ___|  ___  _ __   __ _ _ _/ _ \ _   _  __| | ___  ___| |__  
 \___ \ / _ \| '_ \ / _` | '__| | | | | |/ _` |/ _ \/ __| '_ \ 
  ___) | (_) | | | | (_| | |  | |_| | |_| (_| |  __/ (__| | | |
 |____/ \___/|_| |_|\__,_|_|   \__\_\\__,_\__,_|\___|\___|_| |_|
                                                                
  E-Commerce Microservices - Local Analysis
  
"@ -ForegroundColor Magenta

# ===========================================
# Validaciones
# ===========================================
Write-Step "Validando prerequisitos..."

# Verificar .NET SDK
$dotnetVersion = dotnet --version 2>$null
if (-not $dotnetVersion) {
    Write-Error ".NET SDK no encontrado. Instala .NET 8.0+ desde https://dotnet.microsoft.com/download"
    exit 1
}
Write-Success ".NET SDK $dotnetVersion encontrado"

# Verificar SonarScanner
$scannerInstalled = dotnet tool list -g | Select-String "dotnet-sonarscanner"
if (-not $scannerInstalled) {
    Write-Warning "SonarScanner no instalado. Instalando..."
    dotnet tool install --global dotnet-sonarscanner
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Error instalando SonarScanner"
        exit 1
    }
    Write-Success "SonarScanner instalado correctamente"
} else {
    Write-Success "SonarScanner ya instalado"
}

# Verificar token
if ([string]::IsNullOrEmpty($SonarToken)) {
    Write-Warning "Token no proporcionado. Intentando usar variable de entorno SONAR_TOKEN..."
    $SonarToken = $env:SONAR_TOKEN
    
    if ([string]::IsNullOrEmpty($SonarToken)) {
        Write-Host "`nPara obtener un token:" -ForegroundColor Yellow
        Write-Host "  1. Abre $SonarUrl en tu navegador"
        Write-Host "  2. Login con admin/admin (primera vez)"
        Write-Host "  3. Ve a My Account > Security > Generate Tokens"
        Write-Host ""
        $SonarToken = Read-Host "Ingresa tu SonarQube token"
        
        if ([string]::IsNullOrEmpty($SonarToken)) {
            Write-Error "Token requerido para el analisis"
            exit 1
        }
    }
}

# Verificar conexion a SonarQube
Write-Step "Verificando conexion a SonarQube..."
try {
    $response = Invoke-WebRequest -Uri "$SonarUrl/api/system/status" -UseBasicParsing -TimeoutSec 10
    $status = ($response.Content | ConvertFrom-Json).status
    if ($status -eq "UP") {
        Write-Success "SonarQube esta corriendo en $SonarUrl"
    } else {
        Write-Warning "SonarQube status: $status"
    }
} catch {
    Write-Error "No se puede conectar a SonarQube en $SonarUrl"
    Write-Host "Asegurate de que Docker este corriendo: docker-compose up -d sonarqube" -ForegroundColor Yellow
    exit 1
}

# ===========================================
# Analisis
# ===========================================
$solutionPath = Join-Path $PSScriptRoot ".." "ECommerce.sln"
$solutionPath = Resolve-Path $solutionPath

Write-Step "Iniciando analisis de SonarQube..."
Write-Host "  Proyecto: $ProjectKey"
Write-Host "  Solucion: $solutionPath"
Write-Host "  URL: $SonarUrl"

# Begin analysis
Write-Step "Comenzando escaneo..."
$beginArgs = @(
    "sonarscanner", "begin",
    "/k:$ProjectKey",
    "/d:sonar.host.url=$SonarUrl",
    "/d:sonar.token=$SonarToken",
    "/d:sonar.cs.opencover.reportsPaths=**/coverage.opencover.xml",
    "/d:sonar.coverage.exclusions=**/Migrations/**,**/Program.cs,**/*.Tests/**",
    "/d:sonar.exclusions=**/bin/**,**/obj/**,**/Migrations/**"
)

if ($Verbose) {
    $beginArgs += "/d:sonar.verbose=true"
}

& dotnet @beginArgs
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error iniciando SonarScanner"
    exit 1
}

# Build
Write-Step "Compilando solucion..."
dotnet build $solutionPath --configuration Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error en la compilacion"
    # Continuar para ver errores en SonarQube
}

# Tests con cobertura
if (-not $SkipTests) {
    Write-Step "Ejecutando tests con cobertura..."
    dotnet test $solutionPath `
        --configuration Release `
        --no-build `
        --collect:"XPlat Code Coverage" `
        --results-directory ./TestResults `
        -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
    
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "Algunos tests fallaron. Continuando con el analisis..."
    }
} else {
    Write-Warning "Tests omitidos (flag -SkipTests)"
}

# End analysis
Write-Step "Finalizando analisis y enviando resultados..."
dotnet sonarscanner end /d:sonar.token=$SonarToken
if ($LASTEXITCODE -ne 0) {
    Write-Error "Error finalizando SonarScanner"
    exit 1
}

# ===========================================
# Resultado
# ===========================================
Write-Host @"

===========================================
  ANALISIS COMPLETADO
===========================================

  Ver resultados en:
  $SonarUrl/dashboard?id=$ProjectKey

  Comandos utiles:
  - Ver issues: $SonarUrl/project/issues?id=$ProjectKey
  - Ver cobertura: $SonarUrl/component_measures?id=$ProjectKey&metric=coverage
  - Ver duplicados: $SonarUrl/component_measures?id=$ProjectKey&metric=duplicated_lines_density

"@ -ForegroundColor Green

# Abrir navegador (opcional)
$openBrowser = Read-Host "Abrir resultados en el navegador? (S/n)"
if ($openBrowser -ne "n" -and $openBrowser -ne "N") {
    Start-Process "$SonarUrl/dashboard?id=$ProjectKey"
}

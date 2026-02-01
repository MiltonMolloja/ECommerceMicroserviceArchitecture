# Run SonarQube Analysis
# This script loads credentials from User Secrets and runs the full analysis

param(
    [switch]$SkipBuild = $false
)

# Load from User Secrets
$secretsPath = "$env:APPDATA\Microsoft\UserSecrets\cdff406a-e7ae-4bd0-8967-9716c2beed78\secrets.json"

if (-not (Test-Path $secretsPath)) {
    Write-Host "Error: User Secrets not found" -ForegroundColor Red
    Write-Host "Please configure SonarQube settings in User Secrets" -ForegroundColor Yellow
    exit 1
}

$secrets = Get-Content $secretsPath | ConvertFrom-Json
$token = $secrets.'SonarQube:Token'
$url = $secrets.'SonarQube:Url'
$projectKey = $secrets.'SonarQube:ProjectKey'

if (-not $token -or -not $url -or -not $projectKey) {
    Write-Host "Error: Missing SonarQube configuration in User Secrets" -ForegroundColor Red
    Write-Host "Required: SonarQube:Token, SonarQube:Url, SonarQube:ProjectKey" -ForegroundColor Yellow
    exit 1
}

Write-Host "=== SonarQube Analysis ===" -ForegroundColor Cyan
Write-Host "Project: $projectKey" -ForegroundColor Gray
Write-Host "URL: $url" -ForegroundColor Gray
Write-Host ""

# Step 1: Begin analysis
Write-Host "[1/3] Starting SonarScanner..." -ForegroundColor Yellow
dotnet sonarscanner begin "/key:$projectKey" "/d:sonar.host.url=$url" "/d:sonar.token=$token"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: SonarScanner begin failed" -ForegroundColor Red
    exit 1
}

# Step 2: Build
if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "[2/3] Building solution..." -ForegroundColor Yellow
    dotnet build ECommerce.sln --configuration Release --no-incremental
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Build failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "[2/3] Skipping build (--SkipBuild)" -ForegroundColor Gray
}

# Step 3: End analysis
Write-Host ""
Write-Host "[3/3] Finishing analysis and uploading results..." -ForegroundColor Yellow
dotnet sonarscanner end "/d:sonar.token=$token"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: SonarScanner end failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Analysis Complete ===" -ForegroundColor Green
Write-Host "View results at: $url/dashboard?id=$projectKey" -ForegroundColor Cyan

# Run SonarQube Analysis with Code Coverage
# This script loads credentials from User Secrets and runs the full analysis with coverage

param(
    [switch]$SkipBuild = $false,
    [switch]$SkipTests = $false,
    [switch]$SkipCoverage = $false
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

Write-Host "=== SonarQube Analysis with Coverage ===" -ForegroundColor Cyan
Write-Host "Project: $projectKey" -ForegroundColor Gray
Write-Host "URL: $url" -ForegroundColor Gray
Write-Host ""

# Step 1: Clean previous test results
if (-not $SkipTests -and -not $SkipCoverage) {
    Write-Host "[1/5] Cleaning previous test results..." -ForegroundColor Yellow
    Remove-Item -Recurse -Force TestResults -ErrorAction SilentlyContinue
}

# Step 2: Run tests with coverage (OpenCover format for SonarQube)
if (-not $SkipTests) {
    Write-Host ""
    Write-Host "[2/5] Running tests with coverage..." -ForegroundColor Yellow
    
    if ($SkipCoverage) {
        dotnet test ECommerce.sln --configuration Release --no-build
    } else {
        dotnet test ECommerce.sln --configuration Release --collect:"XPlat Code Coverage" --results-directory ./TestResults -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=opencover
    }
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Warning: Some tests failed, continuing with analysis..." -ForegroundColor Yellow
    }
} else {
    Write-Host ""
    Write-Host "[2/5] Skipping tests (-SkipTests)" -ForegroundColor Gray
}

# Step 3: Begin SonarQube analysis
Write-Host ""
Write-Host "[3/5] Starting SonarScanner..." -ForegroundColor Yellow

$coverageParam = ""
if (-not $SkipTests -and -not $SkipCoverage) {
    $coverageParam = "/d:sonar.cs.opencover.reportsPaths=**/TestResults/**/coverage.opencover.xml"
}

$beginArgs = @(
    "sonarscanner", "begin",
    "/key:$projectKey",
    "/d:sonar.host.url=$url",
    "/d:sonar.token=$token"
)

if ($coverageParam) {
    $beginArgs += $coverageParam
}

& dotnet $beginArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: SonarScanner begin failed" -ForegroundColor Red
    exit 1
}

# Step 4: Build
if (-not $SkipBuild) {
    Write-Host ""
    Write-Host "[4/5] Building solution..." -ForegroundColor Yellow
    dotnet build ECommerce.sln --configuration Release --no-incremental
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Error: Build failed" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host ""
    Write-Host "[4/5] Skipping build (-SkipBuild)" -ForegroundColor Gray
}

# Step 5: End analysis
Write-Host ""
Write-Host "[5/5] Finishing analysis and uploading results..." -ForegroundColor Yellow
dotnet sonarscanner end "/d:sonar.token=$token"

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: SonarScanner end failed" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=== Analysis Complete ===" -ForegroundColor Green
Write-Host "View results at: $url/dashboard?id=$projectKey" -ForegroundColor Cyan
Write-Host ""
Write-Host "Options used:" -ForegroundColor Gray
Write-Host "  -SkipBuild    : Skip the build step" -ForegroundColor Gray
Write-Host "  -SkipTests    : Skip running tests" -ForegroundColor Gray
Write-Host "  -SkipCoverage : Run tests without coverage" -ForegroundColor Gray

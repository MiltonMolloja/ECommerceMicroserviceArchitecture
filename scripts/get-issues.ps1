param(
    [string]$Rule = "csharpsquid:S2629",
    [int]$PageSize = 50,
    [string]$Token = $env:SONAR_TOKEN
)

# Try to load from User Secrets if not set
if (-not $Token) {
    $secretsPath = "$env:APPDATA\Microsoft\UserSecrets\cdff406a-e7ae-4bd0-8967-9716c2beed78\secrets.json"
    if (Test-Path $secretsPath) {
        $secrets = Get-Content $secretsPath | ConvertFrom-Json
        $Token = $secrets.'SonarQube:Token'
        Write-Host "Loaded token from User Secrets" -ForegroundColor Gray
    }
}

if (-not $Token) {
    Write-Host "Error: SONAR_TOKEN not found" -ForegroundColor Red
    Write-Host "Options:" -ForegroundColor Yellow
    Write-Host "  1. Set environment variable: `$env:SONAR_TOKEN = 'your-token'" -ForegroundColor Gray
    Write-Host "  2. Add to User Secrets (Gateway project)" -ForegroundColor Gray
    Write-Host "  3. Pass as parameter: -Token 'your-token'" -ForegroundColor Gray
    exit 1
}

$headers = @{ "Authorization" = "Bearer $Token" }
$url = "http://localhost:9000/api/issues/search?componentKeys=ECommerceMicroserviceArchitecture&resolved=false&rules=$Rule&ps=$PageSize"

$response = Invoke-RestMethod -Uri $url -Headers $headers

Write-Host "Total issues: $($response.total)" -ForegroundColor Cyan
Write-Host ""

foreach ($issue in $response.issues) {
    $file = $issue.component -replace "ECommerceMicroserviceArchitecture:", ""
    Write-Host "$file`:$($issue.line)" -ForegroundColor Yellow
    Write-Host "  $($issue.message)" -ForegroundColor Gray
}

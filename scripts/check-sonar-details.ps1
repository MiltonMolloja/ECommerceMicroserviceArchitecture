# Get token from environment variable or parameter
param(
    [string]$SonarToken = $env:SONAR_TOKEN
)

if ([string]::IsNullOrEmpty($SonarToken)) {
    Write-Host "Error: SONAR_TOKEN environment variable not set." -ForegroundColor Red
    Write-Host "Set it with: `$env:SONAR_TOKEN = 'your-token'" -ForegroundColor Yellow
    exit 1
}

$headers = @{ "Authorization" = "Bearer $SonarToken" }

$response = Invoke-WebRequest -Uri "http://localhost:9000/api/issues/search?componentKeys=ECommerceMicroserviceArchitecture&severities=CRITICAL&statuses=OPEN&ps=50" -Headers $headers -UseBasicParsing
$json = $response.Content | ConvertFrom-Json

Write-Host "=== CRITICAL Issues Details ==="
Write-Host ""

foreach ($issue in $json.issues) {
    $file = $issue.component -replace "ECommerceMicroserviceArchitecture:", ""
    Write-Host "[$($issue.rule)] Line $($issue.line)"
    Write-Host "  File: $file"
    Write-Host "  Message: $($issue.message)"
    Write-Host ""
}

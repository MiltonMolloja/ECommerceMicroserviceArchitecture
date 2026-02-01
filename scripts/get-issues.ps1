param(
    [string]$Rule = "csharpsquid:S2629",
    [int]$PageSize = 50,
    [string]$Token = $env:SONAR_TOKEN
)

if (-not $Token) {
    Write-Host "Error: SONAR_TOKEN environment variable not set" -ForegroundColor Red
    Write-Host "Set it with: `$env:SONAR_TOKEN = 'your-token'" -ForegroundColor Yellow
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

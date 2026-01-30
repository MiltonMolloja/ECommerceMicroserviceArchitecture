param(
    [string]$Rule = "csharpsquid:S2629",
    [int]$PageSize = 50,
    [string]$Token = "squ_f10da5ad19320666cef18128c2c9f31a67bed5b7"
)

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

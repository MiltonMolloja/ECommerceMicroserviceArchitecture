$token = "squ_f10da5ad19320666cef18128c2c9f31a67bed5b7"
$headers = @{ "Authorization" = "Bearer $token" }

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

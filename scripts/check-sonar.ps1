$token = "squ_f10da5ad19320666cef18128c2c9f31a67bed5b7"
$headers = @{ "Authorization" = "Bearer $token" }

$response = Invoke-WebRequest -Uri "http://localhost:9000/api/issues/search?componentKeys=ECommerceMicroserviceArchitecture&severities=CRITICAL&statuses=OPEN&ps=50" -Headers $headers -UseBasicParsing
$json = $response.Content | ConvertFrom-Json

Write-Host "Total CRITICAL: $($json.total)"
Write-Host ""
Write-Host "=== CRITICAL Issues by Rule ==="

$ruleCount = @{}
foreach ($issue in $json.issues) {
    $rule = $issue.rule
    if (-not $ruleCount.ContainsKey($rule)) {
        $ruleCount[$rule] = 0
    }
    $ruleCount[$rule]++
}

$ruleCount.GetEnumerator() | Sort-Object -Property Value -Descending | ForEach-Object {
    Write-Host "$($_.Value) - $($_.Key)"
}

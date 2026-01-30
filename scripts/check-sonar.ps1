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

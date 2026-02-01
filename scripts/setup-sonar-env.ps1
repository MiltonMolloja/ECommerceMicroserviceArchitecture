# Setup SonarQube environment variables
# Run this script once to set up the environment for SonarQube analysis

# Load from User Secrets (Gateway project)
$secretsPath = "$env:APPDATA\Microsoft\UserSecrets\cdff406a-e7ae-4bd0-8967-9716c2beed78\secrets.json"

if (Test-Path $secretsPath) {
    $secrets = Get-Content $secretsPath | ConvertFrom-Json
    
    # Set environment variables for current session
    $env:SONAR_TOKEN = $secrets.'SonarQube:Token'
    $env:SONAR_HOST_URL = $secrets.'SonarQube:Url'
    $env:SONAR_PROJECT_KEY = $secrets.'SonarQube:ProjectKey'
    
    Write-Host "SonarQube environment variables loaded from User Secrets" -ForegroundColor Green
    Write-Host "  SONAR_TOKEN: ****" + $env:SONAR_TOKEN.Substring($env:SONAR_TOKEN.Length - 4) -ForegroundColor Gray
    Write-Host "  SONAR_HOST_URL: $env:SONAR_HOST_URL" -ForegroundColor Gray
    Write-Host "  SONAR_PROJECT_KEY: $env:SONAR_PROJECT_KEY" -ForegroundColor Gray
} else {
    Write-Host "User Secrets file not found at: $secretsPath" -ForegroundColor Red
    Write-Host "Please configure User Secrets for the Gateway project" -ForegroundColor Yellow
}

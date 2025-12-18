# =============================================
# Script: test-rabbitmq-integration.ps1
# Description: Tests RabbitMQ integration with the microservices
# Author: DBA Expert
# Date: 2024
# =============================================

param(
    [switch]$StartRabbitMQ,
    [switch]$StopRabbitMQ,
    [switch]$CheckHealth,
    [switch]$PublishTestEvent,
    [string]$RabbitMQHost = "localhost",
    [int]$RabbitMQPort = 15672,
    [string]$Username = "guest",
    [string]$Password = "guest"
)

$ErrorActionPreference = "Stop"

# Colors for output
function Write-Success { param($Message) Write-Host $Message -ForegroundColor Green }
function Write-Info { param($Message) Write-Host $Message -ForegroundColor Cyan }
function Write-Warning { param($Message) Write-Host $Message -ForegroundColor Yellow }
function Write-Error { param($Message) Write-Host $Message -ForegroundColor Red }

# RabbitMQ Management API base URL
$baseUrl = "http://${RabbitMQHost}:${RabbitMQPort}/api"
$authHeader = @{
    Authorization = "Basic " + [Convert]::ToBase64String([Text.Encoding]::ASCII.GetBytes("${Username}:${Password}"))
}

function Start-RabbitMQ {
    Write-Info "Starting RabbitMQ container..."
    
    $result = docker compose up -d rabbitmq 2>&1
    if ($LASTEXITCODE -eq 0) {
        Write-Success "RabbitMQ container started successfully!"
        Write-Info "Waiting for RabbitMQ to be ready..."
        Start-Sleep -Seconds 10
        
        # Check if RabbitMQ is ready
        $maxRetries = 30
        $retryCount = 0
        while ($retryCount -lt $maxRetries) {
            try {
                $response = Invoke-RestMethod -Uri "$baseUrl/overview" -Headers $authHeader -Method Get -ErrorAction Stop
                Write-Success "RabbitMQ is ready!"
                Write-Info "  Version: $($response.rabbitmq_version)"
                Write-Info "  Erlang: $($response.erlang_version)"
                Write-Info "  Management UI: http://${RabbitMQHost}:${RabbitMQPort}"
                return $true
            }
            catch {
                $retryCount++
                Write-Warning "Waiting for RabbitMQ... ($retryCount/$maxRetries)"
                Start-Sleep -Seconds 2
            }
        }
        Write-Error "RabbitMQ did not become ready in time"
        return $false
    }
    else {
        Write-Error "Failed to start RabbitMQ: $result"
        return $false
    }
}

function Stop-RabbitMQ {
    Write-Info "Stopping RabbitMQ container..."
    docker compose stop rabbitmq
    Write-Success "RabbitMQ container stopped."
}

function Test-RabbitMQHealth {
    Write-Info "Checking RabbitMQ health..."
    Write-Info "============================================="
    
    try {
        # Get overview
        $overview = Invoke-RestMethod -Uri "$baseUrl/overview" -Headers $authHeader -Method Get
        Write-Success "RabbitMQ is running!"
        Write-Info "  Version: $($overview.rabbitmq_version)"
        Write-Info "  Node: $($overview.node)"
        Write-Info "  Uptime: $([math]::Round($overview.uptime / 1000 / 60, 2)) minutes"
        
        # Get queues
        Write-Info ""
        Write-Info "Queues:"
        $queues = Invoke-RestMethod -Uri "$baseUrl/queues" -Headers $authHeader -Method Get
        if ($queues.Count -eq 0) {
            Write-Warning "  No queues found (queues will be created when services start)"
        }
        else {
            foreach ($queue in $queues) {
                $status = if ($queue.state -eq "running") { "[OK]" } else { "[!]" }
                Write-Info "  $status $($queue.name)"
                Write-Info "      Messages: $($queue.messages) | Consumers: $($queue.consumers)"
            }
        }
        
        # Get exchanges
        Write-Info ""
        Write-Info "Exchanges (custom):"
        $exchanges = Invoke-RestMethod -Uri "$baseUrl/exchanges" -Headers $authHeader -Method Get
        $customExchanges = $exchanges | Where-Object { $_.name -ne "" -and $_.name -notlike "amq.*" }
        if ($customExchanges.Count -eq 0) {
            Write-Warning "  No custom exchanges found (exchanges will be created when services start)"
        }
        else {
            foreach ($exchange in $customExchanges) {
                Write-Info "  - $($exchange.name) (type: $($exchange.type))"
            }
        }
        
        # Get connections
        Write-Info ""
        Write-Info "Connections:"
        $connections = Invoke-RestMethod -Uri "$baseUrl/connections" -Headers $authHeader -Method Get
        if ($connections.Count -eq 0) {
            Write-Warning "  No active connections (connections will appear when services start)"
        }
        else {
            foreach ($conn in $connections) {
                Write-Info "  - $($conn.user)@$($conn.peer_host):$($conn.peer_port)"
            }
        }
        
        Write-Info ""
        Write-Success "Health check completed successfully!"
        return $true
    }
    catch {
        Write-Error "Failed to connect to RabbitMQ: $($_.Exception.Message)"
        Write-Warning "Make sure RabbitMQ is running: docker compose up -d rabbitmq"
        return $false
    }
}

function Test-ServiceHealthChecks {
    Write-Info ""
    Write-Info "Testing service health checks..."
    Write-Info "============================================="
    
    $services = @(
        @{ Name = "Catalog.Api"; Port = 50000 },
        @{ Name = "Cart.Api"; Port = 50010 },
        @{ Name = "Customer.Api"; Port = 50020 },
        @{ Name = "Identity.Api"; Port = 50030 },
        @{ Name = "Order.Api"; Port = 50040 },
        @{ Name = "Payment.Api"; Port = 50050 },
        @{ Name = "Notification.Api"; Port = 50060 },
        @{ Name = "Api.Gateway.WebClient"; Port = 45000 }
    )
    
    foreach ($service in $services) {
        try {
            $healthUrl = "http://localhost:$($service.Port)/hc"
            $response = Invoke-RestMethod -Uri $healthUrl -Method Get -TimeoutSec 5 -ErrorAction Stop
            
            $rabbitMQStatus = $response.entries.rabbitmq.status
            $dbStatus = $response.entries | Get-Member -MemberType NoteProperty | 
                Where-Object { $_.Name -like "*DbContext*" } | 
                ForEach-Object { $response.entries.$($_.Name).status } | 
                Select-Object -First 1
            
            if ($response.status -eq "Healthy") {
                Write-Success "  [OK] $($service.Name)"
                Write-Info "       RabbitMQ: $rabbitMQStatus | DB: $dbStatus"
            }
            else {
                Write-Warning "  [!] $($service.Name) - $($response.status)"
            }
        }
        catch {
            Write-Warning "  [?] $($service.Name) - Not running or not accessible"
        }
    }
}

function Show-EventFlow {
    Write-Info ""
    Write-Info "Event Flow Diagram"
    Write-Info "============================================="
    Write-Host @"

  Identity.Api -----> CustomerRegisteredEvent -----> Notification.Api
                                                     (Welcome Email)

  Order.Api --------> OrderCreatedEvent -----------> Cart.Api (Clear Cart)
                                              |----> Catalog.Api (Reserve Stock)
                                              |----> Notification.Api

  Order.Api --------> OrderCancelledEvent ---------> Catalog.Api (Release Stock)
                                              |----> Notification.Api

  Order.Api --------> OrderShippedEvent -----------> Notification.Api
                                                     (Shipping Notification)

  Order.Api --------> OrderDeliveredEvent ---------> Notification.Api
                                                     (Delivery + Review Request)

  Payment.Api ------> PaymentCompletedEvent -------> Order.Api (Update Status)
                                              |----> Notification.Api

  Payment.Api ------> PaymentFailedEvent ----------> Order.Api (Update Status)
                                              |----> Notification.Api

  Catalog.Api ------> StockUpdatedEvent -----------> Notification.Api
                                                     (Back in Stock)

  Cart.Api ---------> CartAbandonedEvent ----------> Notification.Api
  (Background Job)                                   (Recovery Email)

"@ -ForegroundColor Cyan
}

function Show-Usage {
    Write-Host @"
RabbitMQ Integration Test Script
================================

Usage:
  .\test-rabbitmq-integration.ps1 [-StartRabbitMQ] [-StopRabbitMQ] [-CheckHealth]

Parameters:
  -StartRabbitMQ    Start RabbitMQ container using docker compose
  -StopRabbitMQ     Stop RabbitMQ container
  -CheckHealth      Check RabbitMQ and service health
  -RabbitMQHost     RabbitMQ host (default: localhost)
  -RabbitMQPort     RabbitMQ management port (default: 15672)
  -Username         RabbitMQ username (default: guest)
  -Password         RabbitMQ password (default: guest)

Examples:
  # Start RabbitMQ and check health
  .\test-rabbitmq-integration.ps1 -StartRabbitMQ -CheckHealth

  # Just check health
  .\test-rabbitmq-integration.ps1 -CheckHealth

  # Stop RabbitMQ
  .\test-rabbitmq-integration.ps1 -StopRabbitMQ

Quick Start:
  1. Start RabbitMQ: docker compose up -d rabbitmq
  2. Start services: dotnet run (each service)
  3. Check health: .\test-rabbitmq-integration.ps1 -CheckHealth
  4. View RabbitMQ UI: http://localhost:15672 (guest/guest)

"@
}

# Main execution
Write-Host ""
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host "  RabbitMQ Integration Test Script" -ForegroundColor Magenta
Write-Host "=============================================" -ForegroundColor Magenta
Write-Host ""

if ($StartRabbitMQ) {
    Start-RabbitMQ
}

if ($StopRabbitMQ) {
    Stop-RabbitMQ
}

if ($CheckHealth) {
    $rabbitOk = Test-RabbitMQHealth
    if ($rabbitOk) {
        Test-ServiceHealthChecks
    }
    Show-EventFlow
}

if (-not $StartRabbitMQ -and -not $StopRabbitMQ -and -not $CheckHealth) {
    Show-Usage
}

Write-Host ""

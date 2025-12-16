# =====================================================
# Script: Test Reviews Endpoint
# Descripción: Verifica que el endpoint de reviews funcione
# =====================================================

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "   Test Reviews Endpoint" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

$gatewayUrl = "http://localhost:45000"
$productId = 232

Write-Host "Testing Gateway endpoints..." -ForegroundColor Yellow
Write-Host ""

# Test 1: Get Product Reviews
Write-Host "1. GET /api/products/$productId/reviews" -ForegroundColor Cyan
$reviewsUrl = "$gatewayUrl/api/products/$productId/reviews?page=1&pageSize=10&sortBy=newest&verifiedOnly=false"

try {
    $response = Invoke-RestMethod -Uri $reviewsUrl -Method Get -ErrorAction Stop
    Write-Host "   ✓ SUCCESS" -ForegroundColor Green
    Write-Host "   Total Reviews: $($response.totalReviews)" -ForegroundColor White
    Write-Host "   Average Rating: $($response.averageRating)" -ForegroundColor White
    Write-Host "   Items returned: $($response.items.Count)" -ForegroundColor White
    
    if ($response.items.Count -gt 0) {
        Write-Host ""
        Write-Host "   First review:" -ForegroundColor Gray
        $firstReview = $response.items[0]
        Write-Host "   - Rating: $($firstReview.rating)" -ForegroundColor Gray
        Write-Host "   - Title: $($firstReview.title)" -ForegroundColor Gray
        Write-Host "   - Comment: $($firstReview.comment.Substring(0, [Math]::Min(50, $firstReview.comment.Length)))..." -ForegroundColor Gray
    }
}
catch {
    Write-Host "   ✗ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""

# Test 2: Get Rating Summary
Write-Host "2. GET /api/products/$productId/reviews/summary" -ForegroundColor Cyan
$summaryUrl = "$gatewayUrl/api/products/$productId/reviews/summary"

try {
    $response = Invoke-RestMethod -Uri $summaryUrl -Method Get -ErrorAction Stop
    Write-Host "   ✓ SUCCESS" -ForegroundColor Green
    Write-Host "   Average Rating: $($response.averageRating)" -ForegroundColor White
    Write-Host "   Total Reviews: $($response.totalReviews)" -ForegroundColor White
    Write-Host "   5 Stars: $($response.rating5Star)" -ForegroundColor White
    Write-Host "   4 Stars: $($response.rating4Star)" -ForegroundColor White
    Write-Host "   3 Stars: $($response.rating3Star)" -ForegroundColor White
    Write-Host "   2 Stars: $($response.rating2Star)" -ForegroundColor White
    Write-Host "   1 Star: $($response.rating1Star)" -ForegroundColor White
    Write-Host "   Recommendation: $($response.recommendationPercentage)%" -ForegroundColor White
}
catch {
    Write-Host "   ✗ FAILED" -ForegroundColor Red
    Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    
    if ($_.Exception.Response) {
        $statusCode = $_.Exception.Response.StatusCode.value__
        Write-Host "   Status Code: $statusCode" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Instructions
Write-Host "Si los endpoints fallan:" -ForegroundColor Yellow
Write-Host "1. Asegúrate de que Gateway.WebClient esté corriendo" -ForegroundColor White
Write-Host "2. Reinicia el servicio para aplicar los cambios" -ForegroundColor White
Write-Host "3. Verifica que Catalog.Api esté corriendo en localhost:20000" -ForegroundColor White
Write-Host ""

pause

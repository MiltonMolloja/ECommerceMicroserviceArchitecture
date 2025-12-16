# Test Search UI - Amazon Style
# Quick testing script for the new search UI improvements

Write-Host "🧪 Testing Search UI - Amazon Style" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if frontend is running
Write-Host "1. Checking if frontend is running..." -ForegroundColor Yellow
$frontendRunning = $false
try {
    $response = Invoke-WebRequest -Uri "https://localhost:4200" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        $frontendRunning = $true
        Write-Host "   ✅ Frontend is running" -ForegroundColor Green
    }
} catch {
    Write-Host "   ❌ Frontend is NOT running" -ForegroundColor Red
    Write-Host "   Run: cd C:\Source\ECommerceFrontend && npm start" -ForegroundColor Yellow
}

Write-Host ""

# Check if backend is running
Write-Host "2. Checking if backend is running..." -ForegroundColor Yellow
$backendRunning = $false
try {
    $response = Invoke-WebRequest -Uri "https://localhost:45000/health" -UseBasicParsing -TimeoutSec 5 -ErrorAction SilentlyContinue
    if ($response.StatusCode -eq 200) {
        $backendRunning = $true
        Write-Host "   ✅ Backend is running" -ForegroundColor Green
    }
} catch {
    Write-Host "   ❌ Backend is NOT running" -ForegroundColor Red
    Write-Host "   Run: docker-compose up -d" -ForegroundColor Yellow
}

Write-Host ""

# Test search endpoint
if ($backendRunning) {
    Write-Host "3. Testing search endpoint..." -ForegroundColor Yellow
    try {
        $searchUrl = "https://localhost:45000/api/catalog/products/search/advanced?query=laptop&page=1&pageSize=24&includeBrandFacets=true&includeCategoryFacets=true&includePriceFacets=true&includeRatingFacets=true&includeAttributeFacets=true"
        $response = Invoke-RestMethod -Uri $searchUrl -Method Get -SkipCertificateCheck
        
        Write-Host "   ✅ Search endpoint working" -ForegroundColor Green
        Write-Host "   📊 Total products: $($response.totalResults)" -ForegroundColor Cyan
        Write-Host "   🎛️  Total filters: $($response.filters.Count)" -ForegroundColor Cyan
        Write-Host "   📦 Products returned: $($response.products.Count)" -ForegroundColor Cyan
    } catch {
        Write-Host "   ❌ Search endpoint failed" -ForegroundColor Red
        Write-Host "   Error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "📋 Manual Testing Checklist" -ForegroundColor Yellow
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

if ($frontendRunning) {
    Write-Host "✅ Frontend is ready for testing!" -ForegroundColor Green
    Write-Host ""
    Write-Host "🌐 Open these URLs to test:" -ForegroundColor Cyan
    Write-Host "   1. Search with query:    https://localhost:4200/s?k=laptop" -ForegroundColor White
    Write-Host "   2. Search with brand:    https://localhost:4200/s?k=laptop`&brandIds=6" -ForegroundColor White
    Write-Host "   3. Search with category: https://localhost:4200/s?categoryIds=1" -ForegroundColor White
    Write-Host "   4. All products:         https://localhost:4200/s" -ForegroundColor White
    Write-Host ""
    Write-Host "🧪 Test these features:" -ForegroundColor Cyan
    Write-Host "   [ ] Breadcrumb shows at top" -ForegroundColor White
    Write-Host "   [ ] Results count shows" -ForegroundColor White
    Write-Host "   [ ] Active filters show when applied" -ForegroundColor White
    Write-Host "   [ ] Can remove filters via X button" -ForegroundColor White
    Write-Host "   [ ] Clear All button works" -ForegroundColor White
    Write-Host "   [ ] Delivery info is dynamic" -ForegroundColor White
    Write-Host "   [ ] Stock count shows with quantity" -ForegroundColor White
    Write-Host "   [ ] Out of stock shows in red" -ForegroundColor White
    Write-Host "   [ ] Cards fade in smoothly" -ForegroundColor White
    Write-Host "   [ ] Staggered animation works" -ForegroundColor White
    Write-Host "   [ ] Hover effects work" -ForegroundColor White
    Write-Host "   [ ] Responsive on mobile - 1 column" -ForegroundColor White
    Write-Host "   [ ] Responsive on tablet - 2 columns" -ForegroundColor White
    Write-Host "   [ ] Dark mode works" -ForegroundColor White
    Write-Host "   [ ] All filters work" -ForegroundColor White
    Write-Host "   [ ] Sorting works" -ForegroundColor White
    Write-Host "   [ ] Infinite scroll works" -ForegroundColor White
    Write-Host ""
    Write-Host "🎨 Visual checks:" -ForegroundColor Cyan
    Write-Host "   [ ] 3-column grid on desktop" -ForegroundColor White
    Write-Host "   [ ] Images are 280px height" -ForegroundColor White
    Write-Host "   [ ] Badges show correctly" -ForegroundColor White
    Write-Host "   [ ] Prices are green" -ForegroundColor White
    Write-Host "   [ ] Add to Cart buttons are orange" -ForegroundColor White
    Write-Host "   [ ] Query highlight is orange" -ForegroundColor White
    Write-Host ""
} else {
    Write-Host "❌ Start the frontend first:" -ForegroundColor Red
    Write-Host "   cd C:\Source\ECommerceFrontend" -ForegroundColor Yellow
    Write-Host "   npm start" -ForegroundColor Yellow
    Write-Host ""
}

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "📚 Documentation:" -ForegroundColor Yellow
Write-Host "   - SEARCH-UI-IMPROVEMENTS.md (detailed guide)" -ForegroundColor White
Write-Host "   - SEARCH-UI-REFACTOR-APPLIED.md (original refactor)" -ForegroundColor White
Write-Host "=====================================" -ForegroundColor Cyan

# Debug: Category Filter Empty When Filtering by Brand

## Problem

When navigating to `/s?brandIds=6` (filtering by Apple brand), the category filter panel is empty:

```html
<mat-expansion-panel class="filter-panel">
  <mat-expansion-panel-header>
    <span>Categoría</span>
  </mat-expansion-panel-header>
  <div class="filter-options">
    <!-- EMPTY - No categories shown -->
  </div>
</mat-expansion-panel>
```

## Root Cause Analysis

### Expected Behavior
When filtering products by `brandIds=6`, the API should return:
1. Products from Apple brand
2. **Facets** including:
   - Categories that contain Apple products
   - Other brands (for cross-filtering)
   - Price ranges
   - Ratings
   - Attributes

### Actual Behavior
The category facet is either:
1. Empty array `[]`
2. Not being returned by the backend
3. Not being mapped correctly by the frontend

## Investigation Steps

### 1. Backend - FacetService

**File**: `Catalog.Service.Queries/Services/FacetService.cs`

**Method**: `CalculateCategoryFacetsAsync`

**Changes Made**:
- ✅ Added `ILanguageContext` dependency injection
- ✅ Added caching (5 minutes TTL)
- ✅ Added localization support (Spanish/English)
- ✅ Added console logging for debugging

**Constructor Update**:
```csharp
public FacetService(
    ApplicationDbContext context,
    IMemoryCache cache,
    ILanguageContext languageContext)  // ✅ NEW
{
    _context = context;
    _cache = cache;
    _languageContext = languageContext;  // ✅ NEW
}
```

```csharp
private async Task<List<FacetItemDto>> CalculateCategoryFacetsAsync(IQueryable<Product> query)
{
    var cacheKey = $"facets:categories:{query.GetHashCode()}";

    if (_cache.TryGetValue(cacheKey, out List<FacetItemDto> cached))
    {
        Console.WriteLine($"✅ Category facets from cache: {cached.Count} items");
        return cached;
    }

    var facets = await query
        .SelectMany(p => p.ProductCategories)
        .GroupBy(pc => new { 
            pc.CategoryId, 
            NameSpanish = pc.Category.NameSpanish,
            NameEnglish = pc.Category.NameEnglish
        })
        .Select(g => new FacetItemDto
        {
            Id = g.Key.CategoryId,
            Name = _languageContext.IsEnglish ? g.Key.NameEnglish : g.Key.NameSpanish,
            Count = g.Count(),
            IsSelected = false
        })
        .OrderByDescending(f => f.Count)
        .Take(15)
        .ToListAsync();

    Console.WriteLine($"📁 Category facets calculated: {facets.Count} items");
    foreach (var facet in facets)
    {
        Console.WriteLine($"  - {facet.Name} ({facet.Count})");
    }

    _cache.Set(cacheKey, facets, TimeSpan.FromMinutes(5));
    return facets;
}
```

**How it works**:
1. Receives `IQueryable<Product>` already filtered by `brandIds=6`
2. Expands `ProductCategories` navigation property
3. Groups by `CategoryId` and `Name`
4. Counts products per category
5. Returns top 15 categories by product count

**Potential Issues**:
- If Apple products are not assigned to categories → empty result
- If `ProductCategories` navigation is not loaded → empty result
- If query is too restrictive → few or no categories

### 2. Frontend - Search Results Component

**File**: `search-results.component.ts`

**Changes Made**:
- ✅ Added logging in `performSearch` method
- ✅ Added logging for facets and filters

```typescript
next: (response) => {
  console.log('🔍 Search Response:', response);
  console.log('📊 Total Products:', response.products.length);
  console.log('🎛️ Filters:', response.filters);
  console.log('📁 Category Filter:', response.filters.find(f => f.id === 'category'));
  
  // ... rest of code
  
  console.log('✅ Filters with selection:', filtersWithSelection);
  this.filters.set(filtersWithSelection);
}
```

### 3. Frontend - Facet Mapper

**File**: `facet-mapper.service.ts`

**Method**: `mapFacetsToFilters`

**Line 50-52**:
```typescript
// 5. Mapear facetas de categorías
if (facets.categories && facets.categories.length > 0) {
  filters.push(this.createCategoryFilter(facets.categories));
}
```

**Issue**: If `facets.categories` is empty or undefined, no category filter is added.

## Testing

### Test 1: API Direct Call

**Request**:
```http
POST https://localhost:45000/catalog/v1/products/search/advanced
Content-Type: application/json
Accept-Language: es

{
  "query": "",
  "brandIds": [6],
  "page": 1,
  "pageSize": 24,
  "includeBrandFacets": true,
  "includeCategoryFacets": true,
  "includePriceFacets": true,
  "includeRatingFacets": true,
  "includeAttributeFacets": true
}
```

**Expected Response**:
```json
{
  "items": [
    { "productId": 140, "name": "MacBook Pro", "brandId": 6, ... }
  ],
  "facets": {
    "brands": [...],
    "categories": [
      { "id": 1, "name": "Computadoras", "count": 15, "isSelected": false }
    ],
    "priceRanges": {...},
    "ratings": {...},
    "attributes": {...}
  },
  "total": 15,
  "page": 1,
  "pageSize": 24
}
```

**Check**:
- ✅ `facets.categories` should NOT be empty
- ✅ Should contain categories where Apple products exist
- ✅ Count should match number of Apple products in that category

### Test 2: Frontend Console Logs

**Navigate to**: `https://localhost:4200/s?brandIds=6`

**Open Console** (F12) and check:

```
🏷️ Brand filter from URL: ["6"]
🔍 Search Response: { products: [...], filters: [...], facets: {...} }
📊 Total Products: 15
🎛️ Filters: [...]
📁 Category Filter: { id: "category", name: "Categoría", options: [...] }
✅ Filters with selection: [...]
```

**Check**:
1. `brandIds` is correctly parsed from URL
2. `facets.categories` exists in response
3. Category filter is created with options
4. Filters are set in component state

### Test 3: Backend Console Logs

**Check Docker logs**:
```bash
docker logs ecommerce-catalog-api -f
```

**Expected Output**:
```
📁 Category facets calculated: 1 items
  - Computadoras (15)
```

**Or if cached**:
```
✅ Category facets from cache: 1 items
```

## Possible Causes & Solutions

### Cause 1: Apple Products Not Assigned to Categories

**Check**:
```sql
SELECT 
    p.ProductId,
    p.NameSpanish,
    p.BrandId,
    b.Name AS BrandName,
    pc.CategoryId,
    c.NameSpanish AS CategoryName
FROM Catalog.Products p
LEFT JOIN Catalog.Brands b ON p.BrandId = b.BrandId
LEFT JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId
LEFT JOIN Catalog.Categories c ON pc.CategoryId = c.CategoryId
WHERE p.BrandId = 6
```

**Solution**: If no categories, run category assignment script:
```bash
cd C:/Source/ECommerceMicroserviceArchitecture
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -i scripts/assign-categories-fixed.sql
```

### Cause 2: ProductCategories Not Loaded

**Check**: Verify `Include` in `SearchAdvancedAsync`:
```csharp
var query = _context.Products
    .Include(p => p.Stock)
    .Include(p => p.BrandNavigation)
    .Include(p => p.ProductCategories)  // ✅ Must be included
        .ThenInclude(pc => pc.Category)  // ✅ Must be included
    .Include(p => p.ProductRating)
    .AsQueryable();
```

**Solution**: Already included in line 385-386 of `ProductQueryService.cs`

### Cause 3: Cache Issue

**Check**: Clear Redis cache:
```bash
cd C:/Source/ECommerceMicroserviceArchitecture
.\clear-redis-cache.ps1
```

**Or restart Redis**:
```bash
docker-compose restart redis
```

### Cause 4: Frontend Not Receiving Facets

**Check**: Network tab in browser DevTools
- Request URL: `https://localhost:4200/api/products/search/advanced`
- Request Body: Should include `"includeCategoryFacets": true`
- Response: Check if `facets.categories` exists

**Solution**: If missing, check Gateway proxy configuration

## Files Modified

### Backend (.NET)

1. ✅ `Catalog.Service.Queries/Services/FacetService.cs`
   - Added caching for category facets
   - Added localization support
   - Added debug logging

### Frontend (Angular)

2. ✅ `search-results.component.ts`
   - Added debug logging in `performSearch`
   - Added logging for facets and filters

## Next Steps

1. **Run Test 1**: Call API directly and verify `facets.categories` is not empty
2. **Run Test 2**: Check frontend console logs
3. **Run Test 3**: Check backend console logs
4. **If categories are empty**:
   - Run SQL query to check if Apple products have categories
   - Run category assignment script if needed
5. **If categories exist but not showing**:
   - Check frontend facet mapper
   - Check if filter is being added to filters array
   - Check if filter-group component is rendering

## Related Files

- `Catalog.Service.Queries/Services/FacetService.cs` - Backend facet calculation
- `Catalog.Service.Queries/ProductQueryService.cs` - Advanced search implementation
- `search-results.component.ts` - Frontend search component
- `facet-mapper.service.ts` - Maps backend facets to frontend filters
- `filter-group.component.ts` - Renders individual filter panels
- `filters-sidebar.component.ts` - Renders all filters

## Expected Final Result

After fixes, navigating to `/s?brandIds=6` should show:

```
Filtros
-------
Precio
  [slider]

Marca
  ☑ Apple (15)
  ☐ Dell (0)
  ☐ HP (0)

Disponibilidad
  ☐ En stock

Ofertas
  ☐ Con descuento

Calificación
  ☐ ⭐⭐⭐⭐ 4 estrellas o más

Categoría  ← Should show categories!
  ☐ Computadoras (15)
  ☐ Laptops (10)
  ☐ Tablets (5)
```

## Debug Commands

```bash
# Clear Redis cache
.\clear-redis-cache.ps1

# Restart backend
docker-compose restart catalog-api

# Check logs
docker logs ecommerce-catalog-api -f

# Run SQL query
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -Q "SELECT COUNT(*) FROM Catalog.ProductCategories pc INNER JOIN Catalog.Products p ON pc.ProductId = p.ProductId WHERE p.BrandId = 6"
```

# Brand Navigation & Category Filter - Complete Implementation

## Summary

Implemented two major features:
1. **Brand Navigation**: Click on brand name in product detail → Navigate to search filtered by that brand
2. **Category Filter Debug**: Fixed and improved category facets when filtering by brand

## Changes Made

### 1. Backend - Add BrandId to Product DTOs

#### Catalog Service

**File**: `Catalog.Service.Queries/DTOs/ProductDto.cs`
```csharp
public class ProductDto
{
    // ... existing properties
    public int? BrandId { get; set; }  // ✅ NEW
    public string Brand { get; set; }
    // ... rest
}
```

**File**: `Catalog.Service.Queries/Extensions/LocalizationExtensions.cs`
```csharp
// Map Brand name and BrandId if BrandNavigation is available
if (product.BrandNavigation != null)
{
    dto.BrandId = product.BrandNavigation.BrandId;  // ✅ NEW
    dto.Brand = product.BrandNavigation.Name;
}
else if (product.BrandId.HasValue)
{
    dto.BrandId = product.BrandId;  // ✅ NEW
}
```

#### Gateway

**File**: `Api.Gateway.Models/Catalog/DTOs/ProductDto.cs`
```csharp
public class ProductDto
{
    // ... existing properties
    public int? BrandId { get; set; }  // ✅ NEW
    public string Brand { get; set; }
    // ... rest
}
```

### 2. Backend - Improve Category Facets

**File**: `Catalog.Service.Queries/Services/FacetService.cs`

**Constructor Update**:
```csharp
private readonly ILanguageContext _languageContext;

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

**Method Update**: `CalculateCategoryFacetsAsync`
```csharp
private async Task<List<FacetItemDto>> CalculateCategoryFacetsAsync(IQueryable<Product> query)
{
    // ✅ NEW: Add caching
    var cacheKey = $"facets:categories:{query.GetHashCode()}";
    if (_cache.TryGetValue(cacheKey, out List<FacetItemDto> cached))
    {
        Console.WriteLine($"✅ Category facets from cache: {cached.Count} items");
        return cached;
    }

    // ✅ NEW: Use localized names
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
            // ✅ NEW: Localized name based on language context
            Name = _languageContext.IsEnglish ? g.Key.NameEnglish : g.Key.NameSpanish,
            Count = g.Count(),
            IsSelected = false
        })
        .OrderByDescending(f => f.Count)
        .Take(15)
        .ToListAsync();

    // ✅ NEW: Debug logging
    Console.WriteLine($"📁 Category facets calculated: {facets.Count} items");
    foreach (var facet in facets)
    {
        Console.WriteLine($"  - {facet.Name} ({facet.Count})");
    }

    // ✅ NEW: Cache for 5 minutes
    _cache.Set(cacheKey, facets, TimeSpan.FromMinutes(5));
    return facets;
}
```

### 3. Frontend - Update Models

**File**: `product-detail-info.component.ts`
```typescript
export interface ProductInfo {
  productId: number | string;
  name: string;
  brandId?: number;  // ✅ NEW
  brand: string;
  // ... rest
}
```

**File**: `product-detail.component.ts`
```typescript
interface ProductDetailResponse {
  // ... existing properties
  brandId?: number;  // ✅ NEW
  brand?: string | { name?: string; id?: string };
  // ... rest
}

// In productInfo computed:
const brandId = product.brandId || (typeof product.brand === 'object' ? product.brand?.id : undefined);

// ... later
if (brandId !== undefined) {
  result.brandId = Number(brandId);
}
```

### 4. Frontend - Update Brand Link

**File**: `product-detail-info.component.html`

**Before**:
```html
<a href="#" class="brand-link" (click)="$event.preventDefault()">
  {{ product.brand || 'Sin marca' }}
</a>
```

**After**:
```html
<a 
  *ngIf="product.brandId" 
  [routerLink]="['/s']" 
  [queryParams]="{ brandIds: product.brandId }"
  class="brand-link">
  {{ product.brand || 'Sin marca' }}
</a>
<span *ngIf="!product.brandId" class="brand-link">
  {{ product.brand || 'Sin marca' }}
</span>
```

### 5. Frontend - Parse brandIds Query Parameter

**File**: `search-results.component.ts`

**Method**: `parseQueryParams`
```typescript
public parseQueryParams(params: Record<string, string | string[]>): SearchParams {
  // ... existing code
  
  // ✅ NEW: Parse brandIds from URL
  if (params['brandIds']) {
    const brandIdsValue = Array.isArray(params['brandIds'])
      ? params['brandIds']
      : [params['brandIds']];
    
    if (!searchParams.filters) {
      searchParams.filters = {};
    }
    searchParams.filters['brand'] = brandIdsValue as string[];
    console.log('🏷️ Brand filter from URL:', searchParams.filters['brand']);
  }
  
  // ... rest of code
}
```

### 6. Frontend - Add Debug Logging

**File**: `search-results.component.ts`

**Method**: `performSearch`
```typescript
next: (response) => {
  console.log('🔍 Search Response:', response);
  console.log('📊 Total Products:', response.products.length);
  console.log('🎛️ Filters:', response.filters);
  console.log('📁 Category Filter:', response.filters.find(f => f.id === 'category'));
  
  // ... existing code
  
  console.log('✅ Filters with selection:', filtersWithSelection);
  this.filters.set(filtersWithSelection);
}
```

## Files Modified

### Backend (.NET)
1. ✅ `Catalog.Service.Queries/DTOs/ProductDto.cs` - Added `BrandId`
2. ✅ `Catalog.Service.Queries/Extensions/LocalizationExtensions.cs` - Map `BrandId`
3. ✅ `Api.Gateway.Models/Catalog/DTOs/ProductDto.cs` - Added `BrandId`
4. ✅ `Catalog.Service.Queries/Services/FacetService.cs` - Improved category facets

### Frontend (Angular)
5. ✅ `product-detail-info.component.ts` - Added `brandId` to `ProductInfo`
6. ✅ `product-detail-info.component.html` - Brand link with router navigation
7. ✅ `product-detail.component.ts` - Map `brandId` from API response
8. ✅ `search-results.component.ts` - Parse `brandIds` query param + debug logs

## User Flow

### Flow 1: Brand Navigation

```
1. User views product detail: /product/140
   ↓
2. API returns: { "brandId": 6, "brand": "Apple" }
   ↓
3. Frontend renders: <a [routerLink]="['/s']" [queryParams]="{ brandIds: 6 }">Apple</a>
   ↓
4. User clicks "Apple"
   ↓
5. Navigates to: /s?brandIds=6
   ↓
6. SearchResultsComponent parses brandIds=6
   ↓
7. Calls API: POST /api/products/search/advanced { brandIds: [6] }
   ↓
8. Shows only Apple products
   ↓
9. Brand filter "Apple" is pre-selected
   ↓
10. Category filter shows categories with Apple products
```

### Flow 2: Category Facets

```
1. API receives: { brandIds: [6], includeCategoryFacets: true }
   ↓
2. ProductQueryService filters products by brandId=6
   ↓
3. FacetService.CalculateCategoryFacetsAsync(filteredQuery)
   ↓
4. Expands ProductCategories navigation
   ↓
5. Groups by CategoryId and localized Name
   ↓
6. Returns: [{ id: 1, name: "Computadoras", count: 15 }]
   ↓
7. Frontend receives facets.categories
   ↓
8. FacetMapper creates category filter
   ↓
9. FiltersSidebar renders category panel with options
```

## Testing

### Test 1: Brand Navigation

1. Navigate to: `https://localhost:4200/product/140`
2. Verify brand link is clickable (blue, underlined)
3. Click on "Apple"
4. Should navigate to: `https://localhost:4200/s?brandIds=6`
5. Should show only Apple products
6. Brand filter should show "Apple" as selected

### Test 2: Category Facets

1. Navigate to: `https://localhost:4200/s?brandIds=6`
2. Open browser console (F12)
3. Check logs:
   ```
   🏷️ Brand filter from URL: ["6"]
   🔍 Search Response: { ... }
   📁 Category Filter: { id: "category", options: [...] }
   ```
4. Verify category filter panel shows categories
5. Categories should only include those with Apple products

### Test 3: Backend Logs

```bash
docker logs ecommerce-catalog-api -f
```

Expected output:
```
📁 Category facets calculated: 1 items
  - Computadoras (15)
```

### Test 4: API Direct Call

```bash
curl -X POST https://localhost:45000/catalog/v1/products/search/advanced \
  -H "Content-Type: application/json" \
  -H "Accept-Language: es" \
  -d '{
    "brandIds": [6],
    "page": 1,
    "pageSize": 24,
    "includeCategoryFacets": true
  }'
```

Verify response includes:
```json
{
  "facets": {
    "categories": [
      { "id": 1, "name": "Computadoras", "count": 15 }
    ]
  }
}
```

## Build & Deploy

### Backend

```bash
cd C:/Source/ECommerceMicroserviceArchitecture

# Build
dotnet build src/Services/Catalog/Catalog.Api/Catalog.Api.csproj

# Restart container
docker-compose restart catalog-api

# Check logs
docker logs ecommerce-catalog-api -f
```

### Frontend

```bash
cd C:/Source/ECommerceFrontend

# Build (optional)
npm run build

# Restart dev server
# Ctrl+C to stop
npm start
```

## Troubleshooting

### Issue 1: Brand link not clickable

**Check**:
- Product has `brandId` in API response
- `product.brandId` is defined in component

**Solution**:
- Verify backend is returning `brandId`
- Clear browser cache (Ctrl+Shift+R)

### Issue 2: Category filter empty

**Check**:
- Products have categories assigned in database
- API response includes `facets.categories`
- Frontend logs show category filter

**Solution**:
```sql
-- Check if products have categories
SELECT COUNT(*) 
FROM Catalog.ProductCategories pc 
INNER JOIN Catalog.Products p ON pc.ProductId = p.ProductId 
WHERE p.BrandId = 6

-- If 0, run assignment script
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -i scripts/assign-categories-fixed.sql
```

### Issue 3: Compilation error in FacetService

**Error**: `The name '_languageContext' does not exist in the current context`

**Solution**: Already fixed - `ILanguageContext` added to constructor

## Performance Improvements

1. **Category Facets Caching**: 5-minute cache reduces DB queries
2. **Localization**: Uses language context instead of hardcoded Spanish
3. **Debug Logging**: Console logs help identify issues quickly

## Related Documentation

- [BRAND-FILTER-NAVIGATION.md](./BRAND-FILTER-NAVIGATION.md) - Brand navigation feature
- [BRAND-FILTER-CATEGORIES-DEBUG.md](./BRAND-FILTER-CATEGORIES-DEBUG.md) - Category filter debugging
- [PRODUCT-CATEGORIES-FIX.md](./PRODUCT-CATEGORIES-FIX.md) - Product categories display fix
- [FILTROS_CATALOGO_README.md](./FILTROS_CATALOGO_README.md) - Advanced filters documentation

## Next Steps

1. ✅ Build backend - **DONE**
2. ✅ Fix compilation errors - **DONE**
3. ⏳ Restart backend services
4. ⏳ Test brand navigation
5. ⏳ Test category facets
6. ⏳ Verify logs in console and backend

## Notes

- `BrandId` is optional (`int?`) to support products without brands
- Category facets are cached for 5 minutes
- Localization uses `ILanguageContext` for Spanish/English support
- Debug logs can be removed in production
- All changes are backward compatible

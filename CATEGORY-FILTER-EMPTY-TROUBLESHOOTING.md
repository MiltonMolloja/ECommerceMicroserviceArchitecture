# Troubleshooting: Category Filter Empty in Search Results

## Problem

When searching for "tv" at `https://localhost:4200/s?k=tv`, the category filter panel is empty:

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

However, the simple search API endpoint returns categories in each product:
```
GET /api/products/search?Query=tv
Response:
{
  "items": [
    {
      "productId": 123,
      "name": "Samsung TV 55\"",
      "categories": [
        {
          "categoryId": 1,
          "name": "Computers",
          "slug": "computadoras"
        }
      ]
    }
  ]
}
```

## Root Cause

The frontend uses **Advanced Search** (`/api/products/search/advanced`) which returns categories as **facets**, not in each product.

### Difference Between Endpoints

#### Simple Search (`/api/products/search`)
- Returns: Products with embedded categories
- Does NOT return: Facets for filtering
- Used for: Basic product listing

#### Advanced Search (`/api/products/search/advanced`)
- Returns: Products + Facets (brands, categories, price ranges, ratings, attributes)
- Facets are aggregated counts across all matching products
- Used for: Search with dynamic filters

## Investigation Steps

### Step 1: Verify Frontend is Using Advanced Search

**Check Console Logs** (F12):

```
🚀 Advanced Search Params: { query: "tv", includeCategoryFacets: true, ... }
📤 Request Body: { query: "tv", includeCategoryFacets: true, ... }
🔗 Request URL: https://localhost:4200/api/products/search/advanced
```

**Expected**: Should see `includeCategoryFacets: true`

### Step 2: Check API Response

**Check Console Logs**:

```
🔍 Search Response: { products: [...], facets: {...}, filters: [...] }
🎯 Raw Facets: { brands: [...], categories: [...], priceRanges: {...} }
📁 Category Filter: { id: "category", options: [...] }
```

**Check**:
- Does `facets.categories` exist?
- Is it an empty array `[]` or undefined?
- Are there options in the category filter?

### Step 3: Verify Backend Returns Category Facets

**Test API Directly**:

```bash
# Use test-search-tv-categories.http
# Or use curl:
curl -X POST https://localhost:45000/catalog/v1/products/search/advanced \
  -H "Content-Type: application/json" \
  -H "Accept-Language: es" \
  -d '{
    "query": "tv",
    "page": 1,
    "pageSize": 24,
    "includeCategoryFacets": true
  }'
```

**Expected Response**:
```json
{
  "items": [...],
  "facets": {
    "brands": [...],
    "categories": [
      { "id": 1, "name": "Computadoras", "count": 15, "isSelected": false }
    ],
    "priceRanges": {...},
    "ratings": {...}
  },
  "total": 15
}
```

**Check**:
- ✅ `facets.categories` should NOT be empty
- ✅ Should contain categories where TV products exist
- ✅ Count should match number of TV products in that category

### Step 4: Check Database - Do TV Products Have Categories?

**Run SQL Query**:

```bash
cd C:/Source/ECommerceMicroserviceArchitecture
.\check-tv-categories.bat
```

**Or manually**:

```sql
SELECT 
    p.ProductId,
    p.NameSpanish,
    COUNT(pc.CategoryId) AS CategoryCount
FROM Catalog.Products p
LEFT JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId
WHERE 
    p.NameSpanish LIKE '%tv%' OR 
    p.NameEnglish LIKE '%tv%'
GROUP BY p.ProductId, p.NameSpanish
ORDER BY CategoryCount DESC;
```

**Expected**:
- Products should have `CategoryCount > 0`
- If `CategoryCount = 0`, products don't have categories assigned

### Step 5: Check Backend Logs

**View Docker Logs**:

```bash
docker logs ecommerce-catalog-api -f
```

**Search for**:
```
📁 Category facets calculated: X items
  - Computadoras (15)
  - Monitores (8)
```

**If you see**:
```
📁 Category facets calculated: 0 items
```

Then TV products don't have categories assigned.

## Possible Causes & Solutions

### Cause 1: TV Products Don't Have Categories Assigned ⚠️ MOST LIKELY

**Symptom**:
- API returns `facets.categories: []`
- Backend logs show `Category facets calculated: 0 items`
- SQL query shows `CategoryCount = 0`

**Solution**:

Run category assignment script:

```bash
cd C:/Source/ECommerceMicroserviceArchitecture
sqlcmd -S localhost\SQLEXPRESS -d ECommerceDb -i scripts/assign-categories-fixed.sql
```

**Or create specific script for TV products**:

```sql
-- Assign TV products to "Monitores" category
INSERT INTO Catalog.ProductCategories (ProductId, CategoryId)
SELECT DISTINCT p.ProductId, 5 -- Assuming 5 is Monitores category
FROM Catalog.Products p
WHERE 
    (p.NameSpanish LIKE '%tv%' OR p.NameEnglish LIKE '%tv%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductCategories pc 
        WHERE pc.ProductId = p.ProductId AND pc.CategoryId = 5
    );
```

### Cause 2: FacetService Not Including ProductCategories

**Symptom**:
- Products have categories in DB
- But API returns empty facets

**Check**: `ProductQueryService.cs` line 385-386

```csharp
var query = _context.Products
    .Include(p => p.ProductCategories)  // ✅ Must be here
        .ThenInclude(pc => pc.Category)  // ✅ Must be here
    .AsQueryable();
```

**Solution**: Already included in current code.

### Cause 3: Cache Issue

**Symptom**:
- Products have categories
- Backend logs show old data

**Solution**:

```bash
# Clear Redis cache
.\clear-redis-cache.ps1

# Or restart Redis
docker-compose restart redis

# Or restart catalog service
docker-compose restart catalog-api
```

### Cause 4: Frontend Not Mapping Facets Correctly

**Symptom**:
- API returns `facets.categories: [...]`
- But frontend shows empty filter

**Check Console Logs**:

```
🔍 DEBUG: Facetas del backend: { categories: [...] }
🔍 DEBUG: Filtros generados: [...]
```

**Check**: `facet-mapper.service.ts` line 50-52

```typescript
if (facets.categories && facets.categories.length > 0) {
  filters.push(this.createCategoryFilter(facets.categories));
}
```

**Solution**: If `facets.categories` is empty, filter won't be added.

### Cause 5: Gateway Not Forwarding Facets

**Symptom**:
- Catalog API returns facets
- But Gateway doesn't forward them

**Check**: Gateway proxy configuration

**Solution**: Verify Gateway is correctly proxying the response.

## Debug Checklist

- [ ] Frontend uses advanced search endpoint
- [ ] Request includes `includeCategoryFacets: true`
- [ ] API response includes `facets.categories`
- [ ] `facets.categories` is not empty
- [ ] TV products have categories in database
- [ ] Backend logs show category facets calculated
- [ ] Frontend logs show facets received
- [ ] Frontend logs show filters generated
- [ ] Category filter appears in filters array
- [ ] FiltersSidebar receives category filter
- [ ] FilterGroup renders category options

## Testing Commands

### 1. Check Database
```bash
.\check-tv-categories.bat
```

### 2. Test API Directly
```bash
# Use test-search-tv-categories.http in VS Code
# Or use curl
curl -X POST https://localhost:45000/catalog/v1/products/search/advanced \
  -H "Content-Type: application/json" \
  -d '{"query":"tv","includeCategoryFacets":true,"page":1,"pageSize":24}'
```

### 3. Check Backend Logs
```bash
docker logs ecommerce-catalog-api -f | findstr "Category facets"
```

### 4. Check Frontend Logs
- Open `https://localhost:4200/s?k=tv`
- Open Console (F12)
- Look for logs starting with 🔍, 📁, 🎯

### 5. Clear Cache
```bash
.\clear-redis-cache.ps1
docker-compose restart catalog-api
```

## Expected Result After Fix

After assigning categories to TV products, searching for "tv" should show:

```
Filtros
-------
Precio
  [slider: $200 - $2000]

Marca
  ☐ Samsung (25)
  ☐ LG (18)
  ☐ Sony (12)

Categoría  ← Should show categories!
  ☐ Monitores (35)
  ☐ Computadoras (20)

Calificación
  ☐ ⭐⭐⭐⭐ 4 estrellas o más

Atributos
  Resolución
    ☐ 4K (30)
    ☐ Full HD (25)
```

## Related Files

### Backend
- `Catalog.Service.Queries/Services/FacetService.cs` - Calculates category facets
- `Catalog.Service.Queries/ProductQueryService.cs` - Advanced search implementation
- `scripts/check-tv-product-categories.sql` - SQL diagnostic script
- `scripts/assign-categories-fixed.sql` - Category assignment script

### Frontend
- `search-results.component.ts` - Search component with logs
- `product-search.service.ts` - API calls with logs
- `facet-mapper.service.ts` - Maps facets to filters
- `filter-group.component.ts` - Renders filter panels

## Quick Fix

If TV products don't have categories:

```sql
-- Quick fix: Assign all TV products to "Monitores" category
USE ECommerceDb;
GO

-- Find or create "Monitores" category
DECLARE @MonitoresCategoryId INT;
SELECT @MonitoresCategoryId = CategoryId 
FROM Catalog.Categories 
WHERE NameSpanish = 'Monitores';

-- If doesn't exist, create it
IF @MonitoresCategoryId IS NULL
BEGIN
    INSERT INTO Catalog.Categories (NameSpanish, NameEnglish, DescriptionSpanish, DescriptionEnglish, Slug, IsActive, DisplayOrder)
    VALUES ('Monitores', 'Monitors', 'Monitores y pantallas', 'Monitors and displays', 'monitores', 1, 5);
    
    SET @MonitoresCategoryId = SCOPE_IDENTITY();
END

-- Assign TV products
INSERT INTO Catalog.ProductCategories (ProductId, CategoryId)
SELECT DISTINCT p.ProductId, @MonitoresCategoryId
FROM Catalog.Products p
WHERE 
    (p.NameSpanish LIKE '%tv%' OR 
     p.NameEnglish LIKE '%tv%' OR
     p.DescriptionSpanish LIKE '%tv%' OR
     p.DescriptionEnglish LIKE '%tv%')
    AND NOT EXISTS (
        SELECT 1 FROM Catalog.ProductCategories pc 
        WHERE pc.ProductId = p.ProductId AND pc.CategoryId = @MonitoresCategoryId
    );

-- Verify
SELECT COUNT(*) AS TVProductsWithCategories
FROM Catalog.Products p
INNER JOIN Catalog.ProductCategories pc ON p.ProductId = pc.ProductId
WHERE p.NameSpanish LIKE '%tv%' OR p.NameEnglish LIKE '%tv%';
```

## Summary

The most likely cause is that **TV products don't have categories assigned in the database**. The advanced search API correctly returns category facets, but if products don't have categories, the facets array will be empty.

**Solution**: Run the category assignment script or manually assign TV products to appropriate categories (Monitores, Computadoras, etc.).

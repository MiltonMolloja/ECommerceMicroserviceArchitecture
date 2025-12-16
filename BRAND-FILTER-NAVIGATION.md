# Brand Filter Navigation - Click Brand to Search

## Overview

Implemented functionality to allow users to click on a brand name in the product detail page and navigate to the advanced search page with that brand pre-filtered.

## User Flow

1. User views product detail page (e.g., `/product/140`)
2. User sees brand name "Apple" as a clickable link
3. User clicks on "Apple"
4. User is redirected to `/s?brandIds=6` (search page with Apple brand filter)
5. Search results show only products from Apple brand

## Implementation

### 1. Backend - Add BrandId to ProductDto

**File**: `Catalog.Service.Queries/DTOs/ProductDto.cs`

Added `BrandId` property:

```csharp
public class ProductDto
{
    // ... existing properties
    
    // Identificación
    public string SKU { get; set; }
    public int? BrandId { get; set; }  // ✅ NEW
    public string Brand { get; set; }
    public string Slug { get; set; }
    
    // ... rest of properties
}
```

**File**: `Catalog.Service.Queries/Extensions/LocalizationExtensions.cs`

Updated mapping to include `BrandId`:

```csharp
// Map Brand name and BrandId if BrandNavigation is available
if (product.BrandNavigation != null)
{
    dto.BrandId = product.BrandNavigation.BrandId;  // ✅ NEW
    dto.Brand = product.BrandNavigation.Name;
}
else if (product.BrandId.HasValue)
{
    // If BrandNavigation is null but BrandId exists, keep the BrandId
    dto.BrandId = product.BrandId;  // ✅ NEW
}
```

### 2. Gateway - Add BrandId to ProductDto

**File**: `Api.Gateway.Models/Catalog/DTOs/ProductDto.cs`

Added `BrandId` property:

```csharp
public class ProductDto
{
    // ... existing properties
    
    // Identificación
    public string SKU { get; set; }
    public int? BrandId { get; set; }  // ✅ NEW
    public string Brand { get; set; }
    public string Slug { get; set; }
    
    // ... rest of properties
}
```

### 3. Frontend - Update Models

**File**: `product-detail-info.component.ts`

Updated `ProductInfo` interface:

```typescript
export interface ProductInfo {
  productId: number | string;
  name: string;
  brandId?: number;  // ✅ NEW
  brand: string;
  // ... rest of properties
}
```

**File**: `product-detail.component.ts`

Updated `ProductDetailResponse` interface:

```typescript
interface ProductDetailResponse {
  // ... existing properties
  brandId?: number;  // ✅ NEW
  brand?: string | { name?: string; id?: string };
  // ... rest of properties
}
```

Updated `productInfo` computed signal to map `brandId`:

```typescript
// Extract brandId
const brandId = product.brandId || (typeof product.brand === 'object' ? product.brand?.id : undefined);

// ... later in the code

// Add brandId if available
if (brandId !== undefined) {
  result.brandId = Number(brandId);
}
```

### 4. Frontend - Update Brand Link

**File**: `product-detail-info.component.html`

Changed brand link from static to dynamic with router navigation:

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
  class="category-link">
  {{ product.brand || 'Sin marca' }}
</a>
<span *ngIf="!product.brandId" class="brand-link">
  {{ product.brand || 'Sin marca' }}
</span>
```

### 5. Frontend - Update Search Component

**File**: `search-results.component.ts`

Updated `parseQueryParams` method to handle `brandIds` query parameter:

```typescript
public parseQueryParams(params: Record<string, string | string[]>): SearchParams {
  // ... existing code
  
  // Parsear brandIds si existe (puede venir como brandIds o filter_brand)
  if (params['brandIds']) {
    const brandIdsValue = Array.isArray(params['brandIds'])
      ? params['brandIds']
      : [params['brandIds']];
    
    // Agregar a filters como 'brand'
    if (!searchParams.filters) {
      searchParams.filters = {};
    }
    searchParams.filters['brand'] = brandIdsValue as string[];
    console.log('🏷️ Brand filter from URL:', searchParams.filters['brand']);
  }
  
  // ... rest of code
}
```

The `convertToAdvancedParams` method already handles brand filters:

```typescript
// BrandIds
if (params.filters['brand']) {
  advancedParams.brandIds = params.filters['brand']
    .map(id => parseInt(id, 10))
    .filter(id => !isNaN(id));
}
```

## API Flow

### Request Flow

1. **Frontend**: User clicks on "Apple" brand link
2. **Router**: Navigates to `/s?brandIds=6`
3. **SearchResultsComponent**: Parses `brandIds=6` from URL
4. **ProductSearchService**: Calls advanced search API with `brandIds: [6]`
5. **Gateway**: Forwards request to Catalog service
6. **Catalog Service**: Filters products by `BrandId = 6`
7. **Response**: Returns products + facets (including brand facets)

### Example API Request

**URL**: `https://localhost:4200/api/products/search/advanced`

**Body**:
```json
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

**Response**:
```json
{
  "products": [
    {
      "productId": 140,
      "name": "MacBook Pro 16\"",
      "brandId": 6,
      "brand": "Apple",
      "price": 2499.99,
      // ... rest of product data
    }
    // ... more Apple products
  ],
  "filters": [
    {
      "id": "brand",
      "name": "Marca",
      "type": "checkbox",
      "options": [
        {
          "id": "6",
          "label": "Apple",
          "count": 15,
          "selected": true  // ✅ Pre-selected
        },
        // ... other brands
      ]
    }
    // ... other filters
  ],
  "totalResults": 15,
  "pagination": {
    "currentPage": 1,
    "totalPages": 1,
    "pageSize": 24
  }
}
```

## Database Schema

The `BrandId` comes from the `Products` table:

```sql
SELECT 
    p.ProductId,
    p.BrandId,
    b.Name AS BrandName
FROM Catalog.Products p
LEFT JOIN Catalog.Brands b ON p.BrandId = b.BrandId
WHERE p.ProductId = 140
```

**Result**:
```
ProductId | BrandId | BrandName
----------|---------|----------
140       | 6       | Apple
```

## Files Modified

### Backend (.NET)

1. ✅ `Catalog.Service.Queries/DTOs/ProductDto.cs` - Added `BrandId` property
2. ✅ `Catalog.Service.Queries/Extensions/LocalizationExtensions.cs` - Map `BrandId` from domain entity
3. ✅ `Api.Gateway.Models/Catalog/DTOs/ProductDto.cs` - Added `BrandId` property

### Frontend (Angular)

1. ✅ `product-detail-info.component.ts` - Added `brandId` to `ProductInfo` interface
2. ✅ `product-detail-info.component.html` - Changed brand link to router navigation
3. ✅ `product-detail.component.ts` - Added `brandId` to `ProductDetailResponse`, mapped in `productInfo` computed
4. ✅ `search-results.component.ts` - Parse `brandIds` query parameter

## Testing

### Manual Testing Steps

1. **Start backend**:
   ```bash
   cd C:/Source/ECommerceMicroserviceArchitecture
   docker-compose up -d
   ```

2. **Start frontend**:
   ```bash
   cd C:/Source/ECommerceFrontend
   npm start
   ```

3. **Test product detail**:
   - Navigate to: `https://localhost:4200/product/140`
   - Verify API response includes `brandId`: 
     ```bash
     curl https://localhost:4200/api/products/140
     ```
   - Should see: `"brandId": 6, "brand": "Apple"`

4. **Test brand link**:
   - Click on "Apple" brand link
   - Should navigate to: `https://localhost:4200/s?brandIds=6`
   - Should show only Apple products
   - Brand filter should be pre-selected in sidebar

5. **Test with different products**:
   - Product 232 (should have different brand)
   - Product 1 (should have different brand)

### Expected Behavior

✅ **Product Detail Page**:
- Brand name is clickable (blue link with hover effect)
- Clicking brand navigates to search page
- URL includes `brandIds` parameter

✅ **Search Results Page**:
- Shows only products from selected brand
- Brand filter in sidebar is pre-selected
- Can combine with other filters (category, price, etc.)
- Can remove brand filter to see all products

✅ **API Response**:
- `/api/products/{id}` includes `brandId` field
- `/api/products/search/advanced` accepts `brandIds` array
- Facets show brand filter as selected

## Edge Cases

### 1. Product without BrandId
If `brandId` is null or undefined:
- Brand name shows as plain text (not clickable)
- No navigation occurs

### 2. Multiple Brands
Future enhancement: Support multiple brand filters
- URL: `/s?brandIds=6&brandIds=7`
- Shows products from Apple OR Dell

### 3. Brand + Category Filter
Combine brand with category:
- URL: `/s?brandIds=6&category=1`
- Shows Apple products in "Computers" category

## Related Documentation

- [PRODUCT-CATEGORIES-FIX.md](./PRODUCT-CATEGORIES-FIX.md) - Category navigation
- [FILTROS_CATALOGO_README.md](./FILTROS_CATALOGO_README.md) - Advanced filters
- [API-ROUTES-ANALYSIS.md](./API-ROUTES-ANALYSIS.md) - API endpoints
- [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md) - Database schema

## Future Enhancements

1. **Brand Detail Page**: Create dedicated brand page (`/brand/apple`)
2. **Brand Logo**: Show brand logo instead of text
3. **Brand Description**: Add brand description and featured products
4. **Brand SEO**: Add meta tags for brand pages
5. **Brand Analytics**: Track brand clicks and conversions

## Notes

- The `brandId` is optional (`int?`) to support products without brands
- The search component already had support for `brandIds` in `convertToAdvancedParams`
- The brand filter is automatically selected when navigating from product detail
- Users can remove the brand filter to see all products
- The implementation follows the same pattern as category navigation

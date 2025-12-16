# Category Navigation Fix - Click Category to Search

## Problem

When viewing a product detail page (e.g., `/product/278`), clicking on the category link:

```html
<a href="/catalog/computadoras" class="category-link">
  Computers
</a>
```

Was navigating to `/catalog/computadoras` which doesn't exist or shows incorrect results.

**Expected Behavior**: Should navigate to search page filtered by that category (similar to brand navigation).

## Solution

Changed category link to navigate to search with category filter, matching the pattern used for brand navigation.

### 1. Update Category Link in Product Detail

**File**: `product-detail-info.component.html`

**Before**:
```html
<a 
  *ngIf="product.primaryCategory" 
  [routerLink]="['/catalog', product.primaryCategory.slug]" 
  class="category-link">
  {{ product.primaryCategory.name }}
</a>
```

**After**:
```html
<a 
  *ngIf="product.primaryCategory" 
  [routerLink]="['/s']" 
  [queryParams]="{ categoryIds: product.primaryCategory.categoryId }"
  class="category-link">
  {{ product.primaryCategory.name }}
</a>
```

**Changes**:
- ✅ Changed route from `['/catalog', slug]` to `['/s']`
- ✅ Added `[queryParams]="{ categoryIds: product.primaryCategory.categoryId }"`
- ✅ Uses `categoryId` instead of `slug`

### 2. Parse categoryIds Query Parameter

**File**: `search-results.component.ts`

**Method**: `parseQueryParams`

**Added**:
```typescript
// Parsear categoryIds si existe (puede venir como categoryIds o filter_category)
if (params['categoryIds']) {
  const categoryIdsValue = Array.isArray(params['categoryIds'])
    ? params['categoryIds']
    : [params['categoryIds']];
  
  // Agregar a filters como 'category'
  if (!searchParams.filters) {
    searchParams.filters = {};
  }
  searchParams.filters['category'] = categoryIdsValue as string[];
  console.log('📁 Category filter from URL:', searchParams.filters['category']);
}
```

### 3. Convert Category Filter to Advanced Search Params

**File**: `search-results.component.ts`

**Method**: `convertToAdvancedParams`

**Added**:
```typescript
// CategoryIds
if (params.filters['category']) {
  advancedParams.categoryIds = params.filters['category']
    .map(id => parseInt(id, 10))
    .filter(id => !isNaN(id));
}
```

## User Flow

### Before Fix

```
1. User views product: /product/278
   ↓
2. Clicks "Computers" category
   ↓
3. Navigates to: /catalog/computadoras
   ↓
4. Page not found or shows wrong results ❌
```

### After Fix

```
1. User views product: /product/278
   ↓
2. Product has: { primaryCategory: { categoryId: 1, name: "Computers" } }
   ↓
3. Renders: <a [routerLink]="['/s']" [queryParams]="{ categoryIds: 1 }">Computers</a>
   ↓
4. User clicks "Computers"
   ↓
5. Navigates to: /s?categoryIds=1
   ↓
6. SearchResultsComponent parses categoryIds=1
   ↓
7. Calls API: POST /api/products/search/advanced { categoryIds: [1] }
   ↓
8. Shows only products in "Computers" category ✅
   ↓
9. Category filter "Computers" is pre-selected ✅
```

## API Flow

### Request

**URL**: `/s?categoryIds=1`

**Parsed to**:
```typescript
{
  filters: {
    category: ['1']
  }
}
```

**Converted to**:
```typescript
{
  categoryIds: [1],
  includeBrandFacets: true,
  includeCategoryFacets: true,
  includePriceFacets: true,
  includeRatingFacets: true,
  includeAttributeFacets: true
}
```

### Response

```json
{
  "items": [
    {
      "productId": 278,
      "name": "Laptop Dell XPS 15",
      "categories": [
        { "categoryId": 1, "name": "Computers", "slug": "computadoras" }
      ]
    }
    // ... more products in Computers category
  ],
  "facets": {
    "brands": [...],
    "categories": [
      { "id": 1, "name": "Computers", "count": 158, "isSelected": true }
    ],
    "priceRanges": {...},
    "ratings": {...}
  },
  "total": 158
}
```

## Comparison: Brand vs Category Navigation

| Feature | Brand Navigation | Category Navigation |
|---------|------------------|---------------------|
| Link From | Product Detail | Product Detail |
| URL Param | `brandIds` | `categoryIds` |
| Example URL | `/s?brandIds=6` | `/s?categoryIds=1` |
| Filter Key | `filters['brand']` | `filters['category']` |
| API Param | `brandIds: [6]` | `categoryIds: [1]` |
| Pre-selected | ✅ Yes | ✅ Yes |

## Files Modified

### Frontend (Angular)

1. ✅ `product-detail-info.component.html`
   - Changed category link from `/catalog/{slug}` to `/s?categoryIds={id}`

2. ✅ `search-results.component.ts`
   - Added `categoryIds` parsing in `parseQueryParams`
   - Added category filter conversion in `convertToAdvancedParams`

## Testing

### Test 1: Product Detail Page

1. Navigate to: `https://localhost:4200/product/278`
2. Verify category link exists
3. Hover over "Computers" link
4. Should show URL: `/s?categoryIds=1` (not `/catalog/computadoras`)

### Test 2: Click Category Link

1. Click on "Computers" category
2. Should navigate to: `https://localhost:4200/s?categoryIds=1`
3. Should show only products in "Computers" category
4. Category filter should show "Computers" as selected

### Test 3: Verify Console Logs

Open console (F12) after clicking category:

```
📁 Category filter from URL: ["1"]
🚀 Advanced Search Params: { categoryIds: [1], ... }
📤 Request Body: { categoryIds: [1], ... }
🔍 Search Response: { products: [...], facets: {...} }
📁 Category Filter: { id: "category", options: [{ id: "1", label: "Computers", isSelected: true }] }
```

### Test 4: Verify API Request

Check Network tab (F12) → XHR:

**Request**:
```
POST /api/products/search/advanced
Body: {
  "categoryIds": [1],
  "page": 1,
  "pageSize": 24,
  "includeCategoryFacets": true
}
```

**Response**:
```json
{
  "facets": {
    "categories": [
      { "id": 1, "name": "Computers", "count": 158, "isSelected": true }
    ]
  }
}
```

## Edge Cases

### Case 1: Product Without Category

**Scenario**: Product has `primaryCategory = null`

**Behavior**:
```html
<span *ngIf="!product.primaryCategory" class="category-link">Sin categoría</span>
```

**Result**: Shows plain text, not clickable ✅

### Case 2: Multiple Categories

**Current**: Only uses `primaryCategory` (first category)

**Future Enhancement**: Show all categories as chips:
```html
<div class="categories">
  <a *ngFor="let cat of product.categories" 
     [routerLink]="['/s']" 
     [queryParams]="{ categoryIds: cat.categoryId }">
    {{ cat.name }}
  </a>
</div>
```

### Case 3: Combining Filters

**Scenario**: User clicks brand, then clicks category

**URL**: `/s?brandIds=6&categoryIds=1`

**Behavior**: Shows products that match BOTH filters (Apple AND Computers) ✅

## Related Changes

This change complements the brand navigation feature implemented earlier:

1. **Brand Navigation** ([BRAND-FILTER-NAVIGATION.md](./BRAND-FILTER-NAVIGATION.md))
   - Click brand → `/s?brandIds={id}`
   
2. **Category Navigation** (This document)
   - Click category → `/s?categoryIds={id}`

3. **Category Filter Fix** ([CATEGORY-FILTER-FIX-COMPLETE.md](./CATEGORY-FILTER-FIX-COMPLETE.md))
   - Category filter now displays correctly

## Consistency

Both brand and category navigation now follow the same pattern:

```typescript
// Brand Link
<a [routerLink]="['/s']" [queryParams]="{ brandIds: product.brandId }">
  {{ product.brand }}
</a>

// Category Link
<a [routerLink]="['/s']" [queryParams]="{ categoryIds: product.primaryCategory.categoryId }">
  {{ product.primaryCategory.name }}
</a>
```

Both are:
- ✅ Clickable links
- ✅ Navigate to search page
- ✅ Pre-select filter
- ✅ Show filtered results
- ✅ Use query parameters
- ✅ Support multiple values

## Future Enhancements

1. **Breadcrumb Navigation**:
   ```
   Home > Computers > Laptops > Dell XPS 15
   ```

2. **Category Tree**:
   ```
   Computers (158)
     ├─ Laptops (95)
     ├─ Desktops (48)
     └─ Tablets (15)
   ```

3. **SEO-Friendly URLs**:
   ```
   /categoria/computadoras
   /categoria/computadoras/laptops
   ```

4. **Category Slug Support**:
   ```typescript
   // Option to use slug instead of ID
   [queryParams]="{ category: product.primaryCategory.slug }"
   // Backend handles: /s?category=computadoras
   ```

## Summary

Category links in product detail pages now navigate to the search page with the category filter pre-selected, matching the behavior of brand links. This provides a consistent and intuitive navigation experience.

**Key Changes**:
- Category link changed from `/catalog/{slug}` to `/s?categoryIds={id}`
- Added `categoryIds` parsing in search component
- Category filter is pre-selected in results

**Result**: Clicking a category name shows all products in that category with the filter pre-selected.

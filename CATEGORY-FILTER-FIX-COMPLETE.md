# Category Filter Fix - Complete Solution

## Problem

When searching at `https://localhost:4200/s?k=tv`, the category filter panel appears empty even though the API returns category facets correctly:

**API Response** (`/api/products/search/advanced`):
```json
{
  "facets": {    
    "categories": [
      {
        "id": 1,
        "name": "Computers",
        "count": 27,
        "isSelected": false
      }
    ]    
  }
}
```

**Frontend HTML** (empty):
```html
<mat-expansion-panel class="filter-panel">
  <mat-expansion-panel-header>
    <span>Categoría</span>
  </mat-expansion-panel-header>
  <div class="filter-options">
    <!-- EMPTY - No options rendered -->
  </div>
</mat-expansion-panel>
```

## Root Cause

The `filter-group.component.html` template **does not handle `FilterType.CATEGORY`**.

The component only handles:
- ✅ `FilterType.CHECKBOX`
- ✅ `FilterType.RADIO`
- ✅ `FilterType.RANGE`
- ❌ `FilterType.CATEGORY` - **MISSING**

When `facet-mapper.service.ts` creates a category filter with `type: FilterType.CATEGORY`, the template doesn't render it because there's no `@if` block for that type.

## Solution

### 1. Add `FilterType.CATEGORY` Handler in Template

**File**: `filter-group.component.html`

**Location**: After the `RADIO` block, before the `RANGE` block

**Code Added**:
```html
@if (filter.type === FilterType.CATEGORY) {
  @for (option of getVisibleOptions(); track option.id) {
    <div class="filter-option">
      <mat-checkbox
        [checked]="option.isSelected"
        [disabled]="option.disabled"
        (change)="onCheckboxChange(option.id, $event.checked)"
      >
        <span class="option-label">{{ option.label }}</span>
        @if (option.count !== undefined) {
          <span class="option-count">({{ option.count }})</span>
        }
      </mat-checkbox>
    </div>
  }

  <!-- Botón "Ver más/menos" para categorías -->
  @if (hasMoreOptions() && !searchQuery()) {
    <button
      mat-button
      color="primary"
      class="show-more-btn"
      (click)="toggleShowAll()"
    >
      @if (showAllOptions()) {
        <ng-container>
          <mat-icon>expand_less</mat-icon>
          <span>Ver menos</span>
        </ng-container>
      } @else {
        <ng-container>
          <mat-icon>expand_more</mat-icon>
          <span>Ver más</span>
        </ng-container>
      }
    </button>
  }
}
```

### 2. Add Debug Logging

**File**: `filter-group.component.ts`

**Changes**:
1. Import `OnInit`
2. Implement `OnInit` interface
3. Add `ngOnInit` method with logging

```typescript
import { Component, Input, Output, EventEmitter, ChangeDetectionStrategy, signal, OnInit } from '@angular/core';

export class FilterGroupComponent implements OnInit {
  @Input({ required: true }) filter!: FilterOption;
  @Output() filterChange = new EventEmitter<FilterChangeEvent>();

  FilterType = FilterType;
  // ... rest of properties

  ngOnInit(): void {
    console.log('🎛️ Filter Group:', {
      id: this.filter.id,
      name: this.filter.name,
      type: this.filter.type,
      optionsCount: this.filter.options?.length || 0,
      options: this.filter.options
    });
  }

  // ... rest of methods
}
```

## How It Works

### Data Flow

1. **Backend** returns facets:
   ```json
   {
     "facets": {
       "categories": [
         { "id": 1, "name": "Computers", "count": 27 }
       ]
     }
   }
   ```

2. **FacetMapper** creates filter:
   ```typescript
   {
     id: 'category',
     name: 'Categoría',
     type: FilterType.CATEGORY,  // ← Key: Uses CATEGORY type
     options: [
       { id: '1', label: 'Computers', count: 27 }
     ]
   }
   ```

3. **FiltersSidebar** passes filter to FilterGroup:
   ```html
   <app-filter-group [filter]="filter" />
   ```

4. **FilterGroup** renders based on type:
   ```html
   @if (filter.type === FilterType.CATEGORY) {
     <!-- Render checkboxes for categories -->
   }
   ```

### Why CATEGORY Type?

The `FilterType.CATEGORY` was created to distinguish category filters from regular checkbox filters, allowing for:
- Different styling
- Hierarchical display (future enhancement)
- Different behavior (e.g., single selection vs multi-selection)

However, the template was never updated to handle this type, so category filters were invisible.

## Files Modified

### Frontend (Angular)

1. ✅ `filter-group.component.html`
   - Added `@if (filter.type === FilterType.CATEGORY)` block
   - Renders checkboxes with counts
   - Includes "Show more/less" button

2. ✅ `filter-group.component.ts`
   - Added `OnInit` import and implementation
   - Added `ngOnInit` with debug logging

## Testing

### Step 1: Verify Console Logs

Navigate to `https://localhost:4200/s?k=tv` and open console (F12):

**Expected Logs**:
```
🚀 Advanced Search Params: { query: "tv", includeCategoryFacets: true }
📤 Request Body: { query: "tv", includeCategoryFacets: true, ... }
🔍 Search Response: { products: [...], facets: {...} }
🎯 Raw Facets: { categories: [{ id: 1, name: "Computers", count: 27 }] }
🔍 DEBUG: Facetas del backend: { categories: [...] }
🔍 DEBUG: Filtros generados: [{ id: "category", type: "category", options: [...] }]
📁 Category Filter: { id: "category", name: "Categoría", type: "category", options: [...] }
🎛️ Filter Group: { id: "category", name: "Categoría", type: "category", optionsCount: 1 }
```

### Step 2: Verify UI

**Expected Result**:
```
Filtros
-------
Precio
  [slider: $0 - $3000]

Marca
  ☐ Samsung (15)
  ☐ LG (10)

Categoría  ← Should now show options!
  ☐ Computers (27)

Calificación
  ☐ ⭐⭐⭐⭐ 4 estrellas o más
```

### Step 3: Test Interaction

1. Click on "Computers" checkbox
2. Should filter products to only show those in "Computers" category
3. URL should update to include category filter
4. Checkbox should show as checked

## Before vs After

### Before Fix

**Template**:
```html
@if (filter.type === FilterType.CHECKBOX) { ... }
@if (filter.type === FilterType.RADIO) { ... }
@if (filter.type === FilterType.RANGE) { ... }
<!-- FilterType.CATEGORY not handled - renders nothing -->
```

**Result**: Empty filter panel

### After Fix

**Template**:
```html
@if (filter.type === FilterType.CHECKBOX) { ... }
@if (filter.type === FilterType.RADIO) { ... }
@if (filter.type === FilterType.CATEGORY) { ... }  ← NEW
@if (filter.type === FilterType.RANGE) { ... }
```

**Result**: Category options displayed with checkboxes and counts

## Alternative Solutions Considered

### Option 1: Change FilterType to CHECKBOX (Not Recommended)

**In `facet-mapper.service.ts`**:
```typescript
private createCategoryFilter(categories: FacetItem[]): FilterOption {
  return {
    id: 'category',
    name: 'Categoría',
    type: FilterType.CHECKBOX,  // ← Change from CATEGORY to CHECKBOX
    // ...
  };
}
```

**Pros**: Would work immediately without template changes
**Cons**: 
- Loses semantic meaning
- Can't differentiate category filters from other checkbox filters
- Harder to add category-specific features later

### Option 2: Add Template Handler (Recommended) ✅

**Pros**:
- Maintains semantic type
- Allows category-specific styling/behavior
- Future-proof for hierarchical categories

**Cons**:
- Requires template update

## Future Enhancements

With `FilterType.CATEGORY` properly supported, we can now add:

1. **Hierarchical Categories**:
   ```html
   <div class="category-tree">
     <div class="category-level-1">
       <mat-checkbox>Computers (50)</mat-checkbox>
       <div class="category-level-2">
         <mat-checkbox>Laptops (30)</mat-checkbox>
         <mat-checkbox>Desktops (20)</mat-checkbox>
       </div>
     </div>
   </div>
   ```

2. **Category Icons**:
   ```html
   <mat-checkbox>
     <mat-icon>computer</mat-icon>
     <span>Computers (50)</span>
   </mat-checkbox>
   ```

3. **Single Selection Mode**:
   ```typescript
   if (filter.id === 'category' && filter.singleSelection) {
     // Use radio buttons instead of checkboxes
   }
   ```

## Related Documentation

- [BRAND-NAVIGATION-COMPLETE.md](./BRAND-NAVIGATION-COMPLETE.md) - Brand navigation feature
- [CATEGORY-FILTER-EMPTY-TROUBLESHOOTING.md](./CATEGORY-FILTER-EMPTY-TROUBLESHOOTING.md) - Troubleshooting guide
- [FILTROS_CATALOGO_README.md](./FILTROS_CATALOGO_README.md) - Advanced filters documentation

## Summary

The category filter was not displaying because the `filter-group.component.html` template did not have a handler for `FilterType.CATEGORY`. The fix adds a new `@if` block that renders category options as checkboxes with counts, identical to the checkbox filter but semantically distinct for future enhancements.

**Key Takeaway**: When adding new filter types to the enum, always update the template to handle them.

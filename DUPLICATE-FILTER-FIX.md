# Fix: Duplicate "Condición" Filter

## Problem

When searching at `https://localhost:4200/s?k=tv`, the filter sidebar shows a duplicate "Condición" (Condition) filter:

```
Filtros
-------
Condición  ← Duplicate
  ☐ Nuevo (27)

Condición  ← Duplicate
  ☐ Nuevo (27)
```

## Root Cause

The "Condición" filter is being returned by the backend as a **dynamic attribute facet** in addition to potentially being a static filter or appearing elsewhere.

The backend's advanced search API returns attribute facets for all product attributes, including "Condition/Condición", which the frontend then renders as a filter.

## Solution

Added an exclusion list (blacklist) in `facet-mapper.service.ts` to skip certain attributes that shouldn't be displayed as filters.

### Implementation

**File**: `facet-mapper.service.ts`

**Method**: `createAttributeFilter`

**Changes**:

```typescript
private createAttributeFilter(attributeName: string, attr: AttributeFacet): FilterOption | null {
  // Lista de atributos a excluir (blacklist)
  const excludedAttributes = ['Condition', 'Condición', 'condition', 'condición'];
  
  // Verificar si el atributo debe ser excluido
  if (excludedAttributes.includes(attr.attributeName) || excludedAttributes.includes(attributeName)) {
    console.log(`⏭️ Skipping excluded attribute: ${attr.attributeName}`);
    return null;
  }

  // ... rest of method
}
```

### How It Works

1. **Backend** returns attribute facets including "Condición":
   ```json
   {
     "facets": {
       "attributes": {
         "Condición": {
           "attributeName": "Condición",
           "attributeType": "Select",
           "values": [
             { "id": "1", "name": "Nuevo", "count": 27 }
           ]
         }
       }
     }
   }
   ```

2. **FacetMapper** iterates through attributes:
   ```typescript
   Object.entries(facets.attributes).forEach(([key, attr]) => {
     const attributeFilter = this.createAttributeFilter(key, attr);
     if (attributeFilter) {  // ← Returns null for excluded attributes
       filters.push(attributeFilter);
     }
   });
   ```

3. **createAttributeFilter** checks blacklist:
   ```typescript
   if (excludedAttributes.includes(attr.attributeName)) {
     return null;  // ← Skipped, not added to filters
   }
   ```

4. **Result**: "Condición" filter is not displayed

## Why Exclude Condition?

The "Condition" attribute is typically:
- Not useful for filtering in most e-commerce contexts
- Redundant (most products are new)
- Takes up space in the filter sidebar
- Can be confusing for users

If needed in the future, it can be:
- Re-enabled by removing from exclusion list
- Moved to a different location (e.g., product detail page)
- Made toggleable via configuration

## Alternative Solutions Considered

### Option 1: Backend Exclusion (Not Recommended)
Exclude "Condición" from attribute facets in backend.

**Pros**: Cleaner API response
**Cons**: 
- Less flexible
- Requires backend changes
- Other clients might need it

### Option 2: Frontend Exclusion (Recommended) ✅
Exclude at frontend level with blacklist.

**Pros**:
- Flexible (easy to add/remove)
- No backend changes needed
- Can be configured per environment
- Other attributes can be excluded easily

**Cons**: 
- Attribute data still transferred over network

### Option 3: Configuration-Based
Load exclusion list from config file.

**Pros**: 
- Can change without code changes
- Different per environment

**Cons**: 
- More complex
- Overkill for now

## Extending the Exclusion List

To exclude additional attributes, simply add them to the array:

```typescript
const excludedAttributes = [
  'Condition', 'Condición', 'condition', 'condición',
  'Internal ID',  // Add more attributes to exclude
  'SKU',
  'Barcode'
];
```

**Note**: Use both Spanish and English names, and consider case variations.

## Console Logging

When an attribute is excluded, you'll see in the console:

```
⏭️ Skipping excluded attribute: Condición
```

This helps verify the exclusion is working correctly. Can be removed in production.

## Files Modified

### Frontend (Angular)

1. ✅ `facet-mapper.service.ts`
   - Added `excludedAttributes` array
   - Added exclusion check in `createAttributeFilter`
   - Added console log for debugging

## Testing

### Before Fix

Navigate to `/s?k=tv` and see:

```
Filtros
-------
Condición
  ☐ Nuevo (27)
  
Condición  ← Duplicate!
  ☐ Nuevo (27)
```

### After Fix

Navigate to `/s?k=tv` and see:

```
Filtros
-------
Precio
  [slider]

Marca
  ☐ Samsung (15)

Categoría
  ☐ Computers (27)

Calificación
  ☐ ⭐⭐⭐⭐ 4+ estrellas

<!-- "Condición" no longer appears -->
```

### Verify Console

Open console (F12) and look for:

```
⏭️ Skipping excluded attribute: Condición
```

## Related Issues

This fix addresses:
- Duplicate filters in sidebar
- Unnecessary filters cluttering UI
- Improved user experience

## Future Enhancements

1. **Configuration File**: Move exclusion list to config
   ```typescript
   // config/filter-exclusions.ts
   export const EXCLUDED_ATTRIBUTES = [
     'Condition', 'Condición',
     // ... more
   ];
   ```

2. **Backend Flag**: Add `excludeFromFilters` flag to attributes
   ```json
   {
     "attributeName": "Condición",
     "excludeFromFilters": true
   }
   ```

3. **Admin Panel**: Allow admins to configure which attributes show as filters

4. **Smart Filtering**: Only show attributes with variation
   - Don't show if all products have same value
   - Don't show if only 1 option

## Summary

The duplicate "Condición" filter was caused by the backend returning it as a dynamic attribute facet. The fix adds a blacklist in the frontend to exclude specific attributes from being displayed as filters. The solution is flexible, maintainable, and doesn't require backend changes.

**Key Change**: Added exclusion check in `createAttributeFilter` method to skip unwanted attributes.

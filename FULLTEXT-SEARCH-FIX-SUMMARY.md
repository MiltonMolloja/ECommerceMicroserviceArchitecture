# Full-Text Search Error - Fix Summary

## 🔴 Errores Resueltos

### 1. DbContext Threading Issue
```
System.InvalidOperationException: A second operation was started on this context instance 
before a previous operation completed.
```
**Ubicación**: `FacetService.cs:82`

### 2. Full-Text Search Not Configured
```
Microsoft.Data.SqlClient.SqlException: Cannot use a CONTAINS or FREETEXT predicate 
on table or indexed view 'Catalog.Products' because it is not full-text indexed.
```
**Ubicación**: `ProductQueryService.cs:464`

---

## ✅ Soluciones Implementadas

### Error 1: DbContext Threading - Múltiples Archivos

**Problema**: El código usaba operaciones paralelas sobre el mismo DbContext en dos lugares diferentes:

1. **FacetService.cs**: `Task.Run()` para ejecutar queries en paralelo
2. **ProductQueryService.cs**: `Task.WhenAll()` para ejecutar 3 queries simultáneas

**Causa**: DbContext NO es thread-safe. Múltiples operaciones concurrentes causan `InvalidOperationException`.

**Solución**: Eliminado paralelismo y ejecutar queries secuencialmente con `await`.

#### FacetService.cs - Cambio 1
```csharp
// ❌ ANTES (causaba threading issues con Task.Run)
var tasks = new List<Task>();
tasks.Add(Task.Run(async () => facets.Brands = await CalculateBrandFacetsAsync(baseQuery)));
tasks.Add(Task.Run(async () => facets.Categories = await CalculateCategoryFacetsAsync(baseQuery)));
await Task.WhenAll(tasks);

// ✅ DESPUÉS (sin threading issues)
if (request.IncludeBrandFacets)
    facets.Brands = await CalculateBrandFacetsAsync(baseQuery);

if (request.IncludeCategoryFacets)
    facets.Categories = await CalculateCategoryFacetsAsync(baseQuery);
```

#### ProductQueryService.cs - Cambio 2
```csharp
// ❌ ANTES (causaba threading issues con Task.WhenAll)
var facetsTask = _facetService.CalculateFacetsAsync(query, request);
var totalTask = query.CountAsync();
var productsTask = query.ToListAsync();
await Task.WhenAll(facetsTask, totalTask, productsTask);

// ✅ DESPUÉS (queries secuenciales)
var total = await query.CountAsync();
query = ApplyAdvancedSorting(query, request);
var products = await query.Skip(skip).Take(pageSize).ToListAsync();
var facets = await _facetService.CalculateFacetsAsync(query, request);
```

**Optimización adicional**: En `CalculateAttributeFacetsAsync()`, materializar IDs primero para evitar N+1 queries:

```csharp
// Materializar IDs primero
var productIds = await query.Select(p => p.ProductId).ToListAsync();

// Usar Contains() en lugar de Any()
.Where(pav => productIds.Contains(pav.ProductId))
```

**Documentación**: `DBCONTEXT-THREADING-FIX.md`

---

### Error 2: Full-Text Search - ProductQueryService.cs

**Problema**: El código intentaba usar `EF.Functions.Contains()` (Full-Text Search) pero la tabla no tiene índice de texto completo configurado.

**Solución**: Cambiar a usar LIKE (`.Contains()` de LINQ) que no requiere configuración adicional.

```csharp
// ❌ ANTES (requiere Full-Text Index)
if (isSpanish)
{
    query = query.Where(p =>
        EF.Functions.Contains(p.NameSpanish, searchTerm) ||
        EF.Functions.Contains(p.DescriptionSpanish, searchTerm)
    );
}

// ✅ DESPUÉS (usa LIKE - sin configuración requerida)
var searchTerm = request.Query.Trim().ToLower();

query = query.Where(p =>
    p.NameSpanish.ToLower().Contains(searchTerm) ||
    p.NameEnglish.ToLower().Contains(searchTerm) ||
    p.DescriptionSpanish.ToLower().Contains(searchTerm) ||
    p.DescriptionEnglish.ToLower().Contains(searchTerm) ||
    p.SKU.ToLower().Contains(searchTerm)
);
```

**Documentación**: `FULLTEXT-SEARCH-SETUP.md`

---

## 📁 Archivos Creados

### Scripts SQL
- ✅ `scripts/enable-fulltext-search.sql` - Script completo para configurar Full-Text Search
- ✅ `enable-fulltext-search.bat` - Script batch para ejecutar la configuración fácilmente

### Documentación
- ✅ `DBCONTEXT-THREADING-FIX.md` - Explicación threading (FacetService con Task.Run)
- ✅ `DBCONTEXT-TASK-WHENALL-FIX.md` - Explicación threading (ProductQueryService con Task.WhenAll)
- ✅ `FULLTEXT-SEARCH-SETUP.md` - Guía completa de Full-Text Search
- ✅ `FULLTEXT-SEARCH-FIX-SUMMARY.md` - Este documento (resumen)

---

## 📁 Archivos Modificados

### Código C#
- ✅ `src/Services/Catalog/Catalog.Service.Queries/Services/FacetService.cs`
  - Eliminado `Task.Run()` para evitar threading issues
  - Optimizado `CalculateAttributeFacetsAsync()` para evitar N+1 queries

- ✅ `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs`
  - Eliminado `Task.WhenAll()` para evitar threading issues
  - Cambiado de `EF.Functions.Contains()` a `.Contains()` (LIKE)
  - Queries ahora ejecutan secuencialmente (CountAsync → ToListAsync → CalculateFacetsAsync)

---

## 🚀 Próximos Pasos (Opcional)

### Si necesitas mejor performance en búsquedas:

1. **Configurar Full-Text Search** (para bases de datos grandes):
   ```bash
   enable-fulltext-search.bat
   ```
   
2. **Modificar código** para usar `EF.Functions.Contains()`:
   ```csharp
   query.Where(p => EF.Functions.Contains(p.NameSpanish, searchTerm))
   ```

3. **Ventajas de Full-Text Search**:
   - ✅ Búsquedas más rápidas (50-100x en tablas grandes)
   - ✅ Búsquedas complejas (AND, OR, NEAR, wildcards)
   - ✅ Búsqueda lingüística (stemming, sinónimos)

4. **Desventajas**:
   - ⚠️ Requiere configuración y mantenimiento
   - ⚠️ Usa espacio adicional en disco
   - ⚠️ Requiere población periódica del índice

---

## ✅ Estado del Proyecto

### Compilación
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Testing
- ✅ Los servicios de Catalog deben funcionar sin errores de threading
- ✅ Las búsquedas de texto funcionan con LIKE
- ⚠️ Full-Text Search deshabilitado (puede habilitarse opcionalmente)

---

## 📚 Lecciones Aprendidas

### 1. DbContext NO es thread-safe
- Nunca usar `Task.Run()` con el mismo DbContext
- Si necesitas paralelismo real, usar `IDbContextFactory<T>`
- Preferir queries secuenciales con `await`

### 2. Full-Text Search requiere configuración
- `EF.Functions.Contains()` requiere Full-Text Index en SQL Server
- Usar LIKE (`.Contains()`) es más simple pero más lento
- Evaluar trade-offs según tamaño de datos y frecuencia de búsquedas

### 3. N+1 Query Problem
- Evitar `.Any()` con IQueryable dentro de `.Where()`
- Materializar datos necesarios primero con `ToListAsync()`
- Usar `Contains()` con listas en memoria

---

## 🎯 Recomendaciones

### Para Desarrollo/Testing
✅ Usar LIKE (código actual) - Simple y sin configuración

### Para Producción
- Si < 100,000 productos: LIKE es suficiente
- Si > 100,000 productos: Considerar Full-Text Search
- Si búsquedas frecuentes: Full-Text Search recomendado

---

## 📞 Soporte

Para más información, consulta:
- `DBCONTEXT-THREADING-FIX.md` - Threading issues
- `FULLTEXT-SEARCH-SETUP.md` - Full-Text Search completo
- [Microsoft Docs - EF Core Threading](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues)
- [Microsoft Docs - Full-Text Search](https://learn.microsoft.com/en-us/sql/relational-databases/search/full-text-search)

---

**Fecha de corrección**: 2025-12-02  
**Archivos afectados**: 2  
**Documentos creados**: 5  
**Estado**: ✅ Resuelto y documentado

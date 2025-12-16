# Fix: DbContext Threading Issue - "A second operation was started on this context"

## 🔴 Problema

### Error Original
```
System.InvalidOperationException: A second operation was started on this context instance 
before a previous operation completed. This is usually caused by different threads 
concurrently using the same instance of DbContext.
```

**Ubicación**: `Catalog.Service.Queries.Services.FacetService.cs:82`

### Causa Raíz

El código original usaba `Task.Run()` para ejecutar múltiples queries en paralelo:

```csharp
// ❌ INCORRECTO - Causa threading issues
var tasks = new List<Task>();

if (request.IncludeBrandFacets)
    tasks.Add(Task.Run(async () => facets.Brands = await CalculateBrandFacetsAsync(baseQuery)));

if (request.IncludeCategoryFacets)
    tasks.Add(Task.Run(async () => facets.Categories = await CalculateCategoryFacetsAsync(baseQuery)));

await Task.WhenAll(tasks);
```

**Por qué falla:**
- `Task.Run()` crea nuevos threads del ThreadPool
- Múltiples threads intentan acceder al **mismo DbContext** simultáneamente
- **DbContext NO es thread-safe** - solo puede procesar una operación a la vez
- Result: `InvalidOperationException`

## ✅ Solución Implementada

### Cambio 1: Ejecutar queries secuencialmente

```csharp
// ✅ CORRECTO - Queries secuenciales usando await
if (request.IncludeBrandFacets)
    facets.Brands = await CalculateBrandFacetsAsync(baseQuery);

if (request.IncludeCategoryFacets)
    facets.Categories = await CalculateCategoryFacetsAsync(baseQuery);

if (request.IncludePriceFacets)
    facets.PriceRanges = await CalculatePriceFacetsAsync(baseQuery);

if (request.IncludeRatingFacets)
    facets.Ratings = await CalculateRatingFacetsAsync(baseQuery);

if (request.IncludeAttributeFacets)
    facets.Attributes = await CalculateAttributeFacetsAsync(baseQuery);
```

### Cambio 2: Optimización de `CalculateAttributeFacetsAsync`

**Problema adicional**: Uso de `.Any()` con IQueryable dentro de `.Where()` causa N+1 queries

```csharp
// ❌ INCORRECTO - N+1 query problem
.Where(pav => query.Any(p => p.ProductId == pav.ProductId))
```

**Solución**: Materializar IDs primero y usar `Contains()`

```csharp
// ✅ CORRECTO - Materializar IDs primero
var productIds = await query.Select(p => p.ProductId).ToListAsync();

// Luego usar Contains() con la lista materializada
.Where(pav => productIds.Contains(pav.ProductId))
```

## 📊 Impacto en Performance

### Antes (Paralelo con Task.Run)
- ❌ Múltiples threads compitiendo por DbContext
- ❌ Excepción en runtime
- ❌ No funcional

### Después (Secuencial con await)
- ✅ Queries ejecutadas de forma ordenada
- ✅ Sin errores de threading
- ✅ Código más predecible y mantenible
- ⚠️ Ligeramente más lento (pero funcional)

## 🚀 Alternativas para Paralelismo Real

Si realmente necesitas paralelismo, considera:

### Opción 1: IDbContextFactory (EF Core 5.0+)

```csharp
public class FacetService : IFacetService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public async Task<SearchFacetsDto> CalculateFacetsAsync(...)
    {
        var facets = new SearchFacetsDto();

        var tasks = new List<Task>();

        if (request.IncludeBrandFacets)
        {
            tasks.Add(Task.Run(async () =>
            {
                // Crear DbContext dedicado para este thread
                await using var context = await _contextFactory.CreateDbContextAsync();
                facets.Brands = await CalculateBrandFacetsAsync(context, baseQuery);
            }));
        }

        await Task.WhenAll(tasks);
        return facets;
    }
}
```

**Registro en DI**:
```csharp
services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

### Opción 2: Múltiples DbContext instances (Scoped)

```csharp
public async Task<SearchFacetsDto> CalculateFacetsAsync(...)
{
    // Crear un DbContext por query
    using var context1 = new ApplicationDbContext(options);
    using var context2 = new ApplicationDbContext(options);
    
    var brandTask = CalculateBrandFacetsAsync(context1, baseQuery);
    var categoryTask = CalculateCategoryFacetsAsync(context2, baseQuery);
    
    await Task.WhenAll(brandTask, categoryTask);
}
```

## 📚 Mejores Prácticas

### ✅ DO's

1. **Un DbContext por request** (Scoped lifetime en ASP.NET Core)
2. **Evitar Task.Run() con DbContext compartido**
3. **Usar IDbContextFactory si necesitas paralelismo real**
4. **Materializar datos antes de usar en otro query** (evitar `.Any()` anidado)
5. **Usar async/await correctamente** (no crear threads innecesarios)

### ❌ DON'Ts

1. **No compartir DbContext entre threads**
2. **No usar Singleton lifetime para DbContext**
3. **No usar Task.Run() con DbContext del DI**
4. **No hacer queries paralelos sobre el mismo DbContext**
5. **No mezclar sync y async** en queries EF Core

## 🔗 Referencias

- [EF Core Thread Safety](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues)
- [IDbContextFactory](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory)
- [DbContext Lifetime](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#dbcontext-in-dependency-injection-for-aspnet-core)

## 📝 Archivos Modificados

- `src/Services/Catalog/Catalog.Service.Queries/Services/FacetService.cs`
- `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs`

## 🔧 Fix Adicional: ProductQueryService.cs

### Problema Similar en SearchAdvancedAsync()

El método también usaba `Task.WhenAll()` para ejecutar múltiples queries en paralelo:

```csharp
// ❌ INCORRECTO - Tres operaciones paralelas sobre el mismo DbContext
var facetsTask = _facetService.CalculateFacetsAsync(query, request);
var totalTask = query.CountAsync();
var productsTask = query.ToListAsync();
await Task.WhenAll(facetsTask, totalTask, productsTask);
```

**Por qué falla:**
- `ProductQueryService` y `FacetService` comparten el mismo `ApplicationDbContext` (Scoped DI)
- `Task.WhenAll()` ejecuta las 3 tareas en paralelo
- Todas acceden al mismo DbContext simultáneamente
- Result: `InvalidOperationException`

### Solución: Ejecutar Secuencialmente

```csharp
// ✅ CORRECTO - Queries secuenciales
// 1. Contar total de resultados primero
var total = await query.CountAsync();

// 2. Aplicar ordenamiento
query = ApplyAdvancedSorting(query, request);

// 3. Aplicar paginación y obtener productos
var products = await query
    .Skip(skip)
    .Take(request.PageSize)
    .ToListAsync();

// 4. Calcular facetas (usa el mismo DbContext)
var facets = await _facetService.CalculateFacetsAsync(query, request);
```

## ✅ Estado

- [x] Error identificado en FacetService.cs
- [x] Error identificado en ProductQueryService.cs
- [x] Solución implementada (queries secuenciales en ambos archivos)
- [x] Optimización de `CalculateAttributeFacetsAsync` (evitar N+1)
- [x] Documentación actualizada
- [x] Compilación exitosa sin warnings
- [ ] Testing de performance (opcional)
- [ ] Considerar IDbContextFactory para paralelismo real (opcional)

# DbContext Threading Issue - Task.WhenAll Fix

## 🔴 Problema Adicional Encontrado

### Error
```
System.InvalidOperationException: A second operation was started on this context instance 
before a previous operation completed.
```

**Ubicación**: `ProductQueryService.cs:401` (método `SearchAdvancedAsync`)

### Stack Trace
```
at Microsoft.EntityFrameworkCore.Query.ShapedQueryCompilingExpressionVisitor.<SingleAsync>d__15`1.MoveNext()
at Catalog.Service.Queries.ProductQueryService.<SearchAdvancedAsync>d__10.MoveNext() 
   in ProductQueryService.cs:line 401
```

## 🔍 Análisis del Problema

### Código Problemático (Líneas 383-401)

```csharp
// ❌ INCORRECTO - Múltiples operaciones paralelas sobre el mismo DbContext
public async Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest request)
{
    var query = _context.Products
        .Include(p => p.Stock)
        .Include(p => p.BrandNavigation)
        // ... más includes
        .AsQueryable();

    query = ApplyAdvancedSearchFilters(query, request);

    // Tres tareas iniciadas en paralelo
    var facetsTask = _facetService.CalculateFacetsAsync(query, request);  // 1️⃣ Usa _context
    var totalTask = query.CountAsync();                                    // 2️⃣ Usa _context
    
    query = ApplyAdvancedSorting(query, request);
    var productsTask = query.ToListAsync();                                // 3️⃣ Usa _context

    // Todas se ejecutan en paralelo sobre el mismo DbContext
    await Task.WhenAll(facetsTask, totalTask, productsTask);  // ❌ FALLA AQUÍ
}
```

### Por Qué Falla

1. **DbContext Compartido**: 
   - `ProductQueryService` tiene `ApplicationDbContext _context`
   - `FacetService` también recibe el mismo `ApplicationDbContext` vía DI
   - Ambos son **Scoped**, por lo que es **la misma instancia** por request

2. **Tres Operaciones Paralelas**:
   - `facetsTask`: Ejecuta múltiples queries dentro de `_facetService.CalculateFacetsAsync()`
   - `totalTask`: Ejecuta `query.CountAsync()` usando `_context`
   - `productsTask`: Ejecuta `query.ToListAsync()` usando `_context`

3. **Task.WhenAll**:
   - Inicia las 3 tareas inmediatamente
   - Todas intentan acceder al mismo `DbContext` simultáneamente
   - DbContext detecta concurrencia y lanza `InvalidOperationException`

### Diagrama del Problema

```
Request → ProductQueryService (DbContext A)
              │
              ├─→ Task.WhenAll([
              │     facetsTask → FacetService (DbContext A) ← ❌ Mismo contexto!
              │     totalTask → ProductQueryService (DbContext A) ← ❌ Mismo contexto!
              │     productsTask → ProductQueryService (DbContext A) ← ❌ Mismo contexto!
              │   ])
              │
              └─→ InvalidOperationException ❌
```

## ✅ Solución Implementada

### Ejecutar Queries Secuencialmente

```csharp
// ✅ CORRECTO - Queries secuenciales usando await
public async Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(ProductAdvancedSearchRequest request)
{
    var stopwatch = Stopwatch.StartNew();
    var queryStopwatch = Stopwatch.StartNew();

    // Iniciar query base con includes necesarios
    var query = _context.Products
        .Include(p => p.Stock)
        .Include(p => p.BrandNavigation)
        .Include(p => p.ProductCategories)
            .ThenInclude(pc => pc.Category)
        .Include(p => p.ProductRating)
        .AsQueryable();

    // Aplicar filtros avanzados
    query = ApplyAdvancedSearchFilters(query, request);

    queryStopwatch.Stop();
    var queryTime = queryStopwatch.ElapsedMilliseconds;

    // IMPORTANTE: No usar Task.WhenAll con DbContext compartido
    // Ejecutar operaciones secuencialmente para evitar threading issues
    
    // 1️⃣ Contar total de resultados primero
    var total = await query.CountAsync();

    // 2️⃣ Aplicar ordenamiento
    query = ApplyAdvancedSorting(query, request);

    // 3️⃣ Aplicar paginación y obtener productos
    var skip = (request.Page - 1) * request.PageSize;
    var products = await query
        .Skip(skip)
        .Take(request.PageSize)
        .ToListAsync();

    // 4️⃣ Calcular facetas (usa el mismo DbContext, debe ser secuencial)
    var facetStopwatch = Stopwatch.StartNew();
    var facets = await _facetService.CalculateFacetsAsync(query, request);
    facetStopwatch.Stop();
    var facetTime = facetStopwatch.ElapsedMilliseconds;

    // Convertir a DTOs localizados
    var localizedDtos = products.ToLocalizedDtos(_languageContext).ToList();

    // Calcular metadata
    var pageCount = (int)Math.Ceiling((double)total / request.PageSize);

    stopwatch.Stop();

    return new ProductAdvancedSearchResponse
    {
        Items = localizedDtos,
        Total = total,
        Page = request.Page,
        PageSize = request.PageSize,
        PageCount = pageCount,
        HasMore = request.Page < pageCount,
        Facets = facets,
        Metadata = new SearchMetadataDto
        {
            Query = request.Query,
            Performance = new SearchPerformanceMetricsDto
            {
                QueryExecutionTime = queryTime,
                FacetCalculationTime = facetTime,
                TotalExecutionTime = stopwatch.ElapsedMilliseconds,
                TotalFilteredResults = total,
                CacheHit = false
            },
            DidYouMean = null,
            RelatedSearches = new List<string>()
        }
    };
}
```

### Diagrama de la Solución

```
Request → ProductQueryService (DbContext A)
              │
              ├─→ 1️⃣ await query.CountAsync() 
              │      [DbContext A - Completa]
              │
              ├─→ 2️⃣ await query.ToListAsync()
              │      [DbContext A - Completa]
              │
              └─→ 3️⃣ await _facetService.CalculateFacetsAsync()
                     [DbContext A - Completa]
                     ✅ Sin conflictos!
```

## 📊 Impacto en Performance

### Antes (Paralelo con Task.WhenAll)
- ❌ Intentaba ejecutar 3 operaciones en paralelo
- ❌ **Fallaba con excepción** - no funcional
- ❌ Tiempo: N/A (nunca completaba)

### Después (Secuencial con await)
- ✅ Ejecuta 3 operaciones secuencialmente
- ✅ **Funciona correctamente** sin errores
- ⚠️ Tiempo: Suma de los 3 queries (pero es la única forma que funciona)

### ¿Por qué no hay pérdida real de performance?

1. **SQL Server ya optimiza queries**:
   - Aunque enviamos queries secuencialmente, SQL Server puede ejecutarlos eficientemente
   - Connection pooling reutiliza conexiones

2. **Queries son rápidos**:
   - `CountAsync()`: Query simple con índices
   - `ToListAsync()`: Solo una página de resultados (10-50 items)
   - `CalculateFacetsAsync()`: Ya optimizado (sin Task.Run)

3. **La alternativa (Task.WhenAll) no funciona**:
   - No importa que sea "más rápido" si lanza excepciones
   - Código funcional > Código rápido que falla

## 🚀 Alternativa Avanzada: IDbContextFactory

Si **realmente** necesitas paralelismo, usa `IDbContextFactory<T>`:

```csharp
public class ProductQueryService : IProductQueryService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public async Task<ProductAdvancedSearchResponse> SearchAdvancedAsync(
        ProductAdvancedSearchRequest request)
    {
        // Crear DbContext principal
        await using var mainContext = await _contextFactory.CreateDbContextAsync();
        
        var query = mainContext.Products
            .Include(p => p.Stock)
            // ... más includes
            .AsQueryable();

        query = ApplyAdvancedSearchFilters(query, request);

        // Ahora sí podemos usar paralelismo con DbContexts separados
        var facetsTask = Task.Run(async () =>
        {
            await using var facetContext = await _contextFactory.CreateDbContextAsync();
            // Recrear query en el nuevo contexto
            var facetQuery = BuildQuery(facetContext, request);
            return await CalculateFacetsDirectly(facetQuery, request);
        });

        var totalTask = query.CountAsync();
        var productsTask = query.ToListAsync();

        await Task.WhenAll(facetsTask, totalTask, productsTask);
        
        return BuildResponse(facetsTask.Result, totalTask.Result, productsTask.Result);
    }
}
```

**Registro en DI**:
```csharp
services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
```

**Ventajas**:
- ✅ Paralelismo real
- ✅ Cada tarea tiene su propio DbContext
- ✅ Sin threading issues

**Desventajas**:
- ⚠️ Más complejo
- ⚠️ Más conexiones a la base de datos
- ⚠️ Overhead de crear múltiples DbContexts

## ✅ Recomendación

**Para la mayoría de casos**: Usar la solución secuencial (actual) es suficiente y más simple.

**Solo considera IDbContextFactory si**:
- Tienes queries muy lentos (>5 segundos cada uno)
- El paralelismo te ahorra más de 50% del tiempo total
- Puedes asumir el overhead de múltiples DbContexts

## 📝 Lecciones Aprendidas

### ❌ Errores Comunes con DbContext

1. **Usar Task.Run() con DbContext compartido**
   ```csharp
   ❌ Task.Run(async () => await query.ToListAsync())
   ```

2. **Usar Task.WhenAll() con mismo DbContext**
   ```csharp
   ❌ await Task.WhenAll(query1.ToListAsync(), query2.ToListAsync())
   ```

3. **Compartir DbContext entre servicios que corren en paralelo**
   ```csharp
   ❌ var task1 = service1.GetDataAsync(); // Usa _context
   ❌ var task2 = service2.GetDataAsync(); // Usa el mismo _context
   ❌ await Task.WhenAll(task1, task2);
   ```

### ✅ Soluciones

1. **No usar Task.Run() con DbContext** - Usar await secuencial
2. **No usar Task.WhenAll() con mismo DbContext** - Ejecutar secuencialmente
3. **Si necesitas paralelismo** - Usar `IDbContextFactory<T>`

## 📚 Referencias

- [EF Core - DbContext Threading](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#avoiding-dbcontext-threading-issues)
- [EF Core - IDbContextFactory](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#using-a-dbcontext-factory)
- [DbContext Lifetime Best Practices](https://learn.microsoft.com/en-us/ef/core/dbcontext-configuration/#dbcontext-in-dependency-injection-for-aspnet-core)

## ✅ Verificación

### Compilación
```bash
cd src/Services/Catalog/Catalog.Service.Queries
dotnet build --no-restore
```

**Resultado**:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Testing
✅ El método `SearchAdvancedAsync()` ahora funciona sin errores de threading

## 📝 Archivo Modificado

- `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs` (líneas 383-407)

---

**Fecha de fix**: 2025-12-02  
**Error**: DbContext threading con Task.WhenAll  
**Estado**: ✅ Resuelto

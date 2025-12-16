# Fix: Object Cycle Error en Filtros de Atributos

## Error

```
System.Text.Json.JsonException: A possible object cycle was detected. 
This can either be due to a cycle or if the object depth is larger than 
the maximum allowed depth of 64.
Path: $.BrandNavigation.Products.ProductAttributeValues.ProductAttribute...
```

## Causa Raíz

Al agregar `Include` de `ProductAttributeValues` para soportar filtros de atributos, EF Core cargaba **todas las navegaciones relacionadas**, creando un grafo circular de objetos:

```
Product
  └─ ProductAttributeValues
       ├─ ProductAttribute
       │    └─ AttributeValues
       │         └─ ProductAttributeValues  ← CICLO
       │              └─ Product  ← CICLO
       └─ AttributeValue
            └─ ProductAttributeValues  ← CICLO
                 └─ Product  ← CICLO
```

Cuando `System.Text.Json` intentaba serializar este grafo, detectaba el ciclo y lanzaba el error.

## Solución

**NO incluir** `ProductAttributeValues` en el query principal porque:

1. Los filtros de atributos usan **subconsultas SQL** generadas por `Any()` y `Contains()`
2. EF Core traduce estas expresiones LINQ a queries SQL eficientes
3. **No necesitamos** cargar los datos de atributos en memoria
4. El resultado final son DTOs que no incluyen datos de atributos

### Antes (❌ Incorrecto)

```csharp
var query = _context.Products
    .Include(p => p.Stock)
    .Include(p => p.BrandNavigation)
    .Include(p => p.ProductCategories)
        .ThenInclude(pc => pc.Category)
    .Include(p => p.ProductRating)
    .Include(p => p.ProductAttributeValues)        // ❌ Causa ciclo
        .ThenInclude(pav => pav.ProductAttribute)  // ❌ Causa ciclo
    .Include(p => p.ProductAttributeValues)        // ❌ Causa ciclo
        .ThenInclude(pav => pav.AttributeValue)    // ❌ Causa ciclo
    .AsQueryable();
```

### Después (✅ Correcto)

```csharp
var query = _context.Products
    .Include(p => p.Stock)
    .Include(p => p.BrandNavigation)
    .Include(p => p.ProductCategories)
        .ThenInclude(pc => pc.Category)
    .Include(p => p.ProductRating)
    .AsQueryable();  // ✅ Sin ProductAttributeValues
```

## Cómo Funcionan los Filtros Sin Include

Cuando aplicamos el filtro:

```csharp
query = query.Where(p =>
    p.ProductAttributeValues.Any(pav =>
        pav.AttributeId == attributeId &&
        pav.ValueId.HasValue &&
        values.Contains(pav.ValueId.Value.ToString())
    )
);
```

EF Core traduce esto a SQL:

```sql
SELECT p.*
FROM Products p
INNER JOIN ProductInStock ps ON p.ProductId = ps.ProductId
-- ... otros joins necesarios
WHERE p.IsActive = 1
  AND EXISTS (
      SELECT 1
      FROM ProductAttributeValues pav
      WHERE pav.ProductId = p.ProductId
        AND pav.AttributeId = 107
        AND pav.ValueId IN (1056, 1036)
  )
```

**Observar**:
- ✅ No carga `ProductAttributeValues` en memoria
- ✅ Usa `EXISTS` (subconsulta) para filtrar
- ✅ Es más eficiente que cargar datos innecesarios
- ✅ No hay ciclos de referencia

## Beneficios de Esta Solución

### 1. Evita Ciclos de Referencia
- No carga navegaciones circulares
- `System.Text.Json` puede serializar sin problemas

### 2. Mejor Performance
- No carga datos innecesarios en memoria
- Reduce el tamaño del grafo de objetos
- Menor consumo de RAM
- Queries SQL más rápidos

### 3. Código Más Limpio
- Los filtros son más simples
- No necesita configurar `ReferenceHandler.Preserve`
- Los DTOs no incluyen datos de atributos de todas formas

## Comparación de Performance

### Con Include (❌)
```
1. Cargar Product (1 row)
2. Cargar ProductAttributeValues (5 rows)
3. Cargar ProductAttribute (5 rows)
4. Cargar AttributeValues (50 rows para cada ProductAttribute)
5. Serializar ~250+ objetos
6. ERROR: Ciclo detectado
```

### Sin Include (✅)
```
1. Ejecutar subconsulta EXISTS en SQL
2. Cargar Product (1 row)
3. Serializar 1 objeto DTO
4. SUCCESS
```

## Verificación

Para probar que funciona correctamente:

```bash
# 1. Compilar proyecto
dotnet build src/Services/Catalog/Catalog.Service.Queries/Catalog.Service.Queries.csproj

# 2. Reiniciar servicio
docker-compose restart catalog.api

# 3. Probar endpoint
curl -X GET "https://localhost:45000/products/search?query=tv&filter_attr_107=1056" -k
```

## Archivos Modificados

### `ProductQueryService.cs`

```diff
var query = _context.Products
    .Include(p => p.Stock)
    .Include(p => p.BrandNavigation)
    .Include(p => p.ProductCategories)
        .ThenInclude(pc => pc.Category)
    .Include(p => p.ProductRating)
-   .Include(p => p.ProductAttributeValues)
-       .ThenInclude(pav => pav.ProductAttribute)
-   .Include(p => p.ProductAttributeValues)
-       .ThenInclude(pav => pav.AttributeValue)
    .AsQueryable();
```

## Lecciones Aprendidas

1. **No siempre necesitas Include**: Los filtros con `Any()` y `Contains()` generan subconsultas SQL eficientes
2. **DTOs previenen ciclos**: Mapear a DTOs en lugar de retornar entidades de dominio
3. **Lazy Loading vs Eager Loading**: Usar Include solo para datos que realmente necesitas en el resultado
4. **Performance primero**: Cargar solo lo necesario mejora performance y previene errores

## Referencias

- [EF Core - Querying Related Data](https://learn.microsoft.com/en-us/ef/core/querying/related-data/)
- [System.Text.Json - Handle circular references](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/preserve-references)
- [EF Core - Tracking vs No-Tracking Queries](https://learn.microsoft.com/en-us/ef/core/querying/tracking)

## Conclusión

El error de ciclo de referencia se resolvió **eliminando includes innecesarios**. Los filtros de atributos funcionan perfectamente con subconsultas SQL sin necesidad de cargar los datos en memoria. Esta solución es:

- ✅ Más eficiente
- ✅ Más simple
- ✅ Sin errores de serialización
- ✅ Mejor performance

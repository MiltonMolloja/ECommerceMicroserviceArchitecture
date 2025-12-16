# Fix: Filtros de Atributos en URL Query Parameters

## Problema Identificado

La URL `https://localhost:4200/s?k=tv&filter_attr_107=1056&filter_attr_107=1036` no mostraba ningún elemento porque:

1. **Endpoint GET `/products/search`** - NO soportaba filtros de atributos dinámicos
2. **Filtros de atributos** solo estaban disponibles en `/products/search/advanced` (POST con JSON body)
3. Los filtros dinámicos de la URL no se estaban procesando

## Solución Implementada

### 1. Gateway WebClient - ProductController.cs

**Ubicación**: `src/Gateways/Api.Gateway.WebClient/Controllers/ProductController.cs`

#### Cambios:

1. **Agregado using `System.Collections.Generic`** para soportar Dictionary y List

2. **Nuevo método `ParseAttributeFiltersFromQuery()`**:
   - Parsea parámetros de query string con formato `filter_attr_X=valueId`
   - Extrae el `AttributeId` del nombre del parámetro (ej: `filter_attr_107` → `107`)
   - Agrupa múltiples valores para el mismo atributo
   - Retorna un `Dictionary<string, List<string>>` con los filtros

3. **Modificado método `Search()`**:
   - Detecta si hay filtros de atributos en la query string
   - Si los hay, convierte el request a `ProductAdvancedSearchRequest`
   - Llama al endpoint avanzado a través del proxy
   - Convierte el resultado avanzado de vuelta a `ProductSearchResponse`
   - Mantiene compatibilidad con búsquedas simples sin filtros

### 2. Catalog.Service.Queries - ProductQueryService.cs

**Ubicación**: `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs`

#### Cambios:

1. **Agregado Include de `ProductAttributeValues`**:
```csharp
.Include(p => p.ProductAttributeValues)
    .ThenInclude(pav => pav.ProductAttribute)
.Include(p => p.ProductAttributeValues)
    .ThenInclude(pav => pav.AttributeValue)
```

2. **Modificado `ApplyAdvancedSearchFilters()`**:
   - Ahora soporta filtros tanto por `AttributeId` (numérico) como por `AttributeName` (string)
   - Si la clave es un número, filtra por `pav.AttributeId`
   - Si la clave es un string, filtra por `pav.ProductAttribute.AttributeName`
   - Permite flexibilidad en el formato de los filtros

## Cómo Funciona

### Flujo de Datos

1. **Cliente envía request**:
   ```
   GET /products/search?k=tv&filter_attr_107=1056&filter_attr_107=1036
   ```

2. **Gateway detecta filtros**:
   - Parsea `filter_attr_107` → AttributeId: `107`
   - Extrae valores: `["1056", "1036"]`
   - Crea `Dictionary<string, List<string>>`: `{ "107": ["1056", "1036"] }`

3. **Gateway convierte a búsqueda avanzada**:
   ```csharp
   ProductAdvancedSearchRequest {
       Query = "tv",
       Attributes = { "107": ["1056", "1036"] }
   }
   ```

4. **Catalog Service filtra productos**:
   - Parsea "107" como `AttributeId`
   - Filtra productos donde `ProductAttributeValues.AttributeId == 107`
   - Y donde `ProductAttributeValues.ValueId` está en `[1056, 1036]`

5. **Retorna resultados filtrados**

## Formato de URL Soportado

### Filtros de Atributos

```
/products/search?k=tv&filter_attr_{attributeId}={valueId}
```

Ejemplos:

```
# Filtro por tamaño de pantalla (attribute 107)
/products/search?k=tv&filter_attr_107=1056

# Múltiples valores para el mismo atributo
/products/search?k=tv&filter_attr_107=1056&filter_attr_107=1036

# Múltiples atributos
/products/search?k=tv&filter_attr_107=1056&filter_attr_108=1020
```

### Combinado con otros filtros

```
/products/search?k=tv&filter_attr_107=1056&minPrice=500&maxPrice=2000&inStock=true
```

## Testing

### 1. Verificar Compilación

```bash
# Gateway WebClient
dotnet build src/Gateways/Api.Gateway.WebClient/Api.Gateway.WebClient.csproj

# Catalog Service
dotnet build src/Services/Catalog/Catalog.Service.Queries/Catalog.Service.Queries.csproj
```

### 2. Reiniciar Servicios

Si los servicios están corriendo en Docker:

```bash
docker-compose restart api.gateway catalog.api
```

O desde PowerShell si están corriendo localmente, detener (Ctrl+C) y reiniciar.

### 3. Probar Endpoint

```bash
# Sin filtros de atributos (búsqueda simple)
curl -X GET "https://localhost:45000/products/search?query=tv" -k

# Con filtro de atributos (búsqueda avanzada automática)
curl -X GET "https://localhost:45000/products/search?query=tv&filter_attr_107=1056" -k

# Múltiples valores del mismo atributo
curl -X GET "https://localhost:45000/products/search?query=tv&filter_attr_107=1056&filter_attr_107=1036" -k
```

### 4. Verificar Logs

Los logs mostrarán:

```
Gateway: Detected X attribute filters, using advanced search
Parsed attribute filter: attr_107 = 1056
Gateway: Advanced search with attributes executed in Xms and cached: ...
```

## Ventajas de la Solución

1. **Transparente**: El frontend puede seguir usando GET con query parameters
2. **Compatible**: Búsquedas simples sin filtros siguen usando el endpoint simple
3. **Flexible**: Soporta tanto `AttributeId` numérico como `AttributeName` string
4. **Performante**: Usa caché y búsqueda avanzada optimizada
5. **Escalable**: Soporta múltiples atributos y múltiples valores

## Consideraciones

### Caché

Los resultados con filtros de atributos se cachean por 2 minutos (configuración por defecto para búsquedas con facetas).

### Performance

- Los `Include` adicionales cargan `ProductAttributeValues` en memoria
- Para grandes volúmenes de productos, considerar paginación y proyecciones
- El filtro por `AttributeId` es más eficiente que por `AttributeName`

### Limitaciones

- Los filtros se aplican con lógica OR dentro del mismo atributo
- Los filtros entre diferentes atributos se aplican con lógica AND
- No soporta rangos de atributos desde query parameters (solo POST avanzado)

## Archivos Modificados

1. `src/Gateways/Api.Gateway.WebClient/Controllers/ProductController.cs`
   - Agregado `ParseAttributeFiltersFromQuery()`
   - Modificado `Search()` para detectar y procesar filtros de atributos

2. `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs`
   - Agregado `Include` de `ProductAttributeValues`
   - Modificado `ApplyAdvancedSearchFilters()` para soportar `AttributeId` numérico

## Próximos Pasos

Para el frontend:

1. Construir query parameters dinámicamente desde los filtros seleccionados
2. Agregar manejo de múltiples valores para el mismo atributo
3. Implementar UI para mostrar/ocultar filtros aplicados
4. Agregar capacidad de limpiar filtros individuales

Ejemplo de construcción de URL en TypeScript:

```typescript
const buildSearchUrl = (keyword: string, filters: Map<number, number[]>): string => {
  const params = new URLSearchParams();
  params.set('query', keyword);
  
  filters.forEach((values, attrId) => {
    values.forEach(valueId => {
      params.append(`filter_attr_${attrId}`, valueId.toString());
    });
  });
  
  return `/products/search?${params.toString()}`;
};
```

## Problemas Conocidos y Soluciones

### Error: JsonException - Object Cycle Detected

**Síntoma**: `System.Text.Json.JsonException: A possible object cycle was detected`

**Causa**: Al agregar `Include` de `ProductAttributeValues`, EF Core carga las navegaciones circulares:
```
Product → ProductAttributeValues → ProductAttribute → AttributeValues → ProductAttributeValues → Product → ...
```

**Solución**: **NO incluir** `ProductAttributeValues` en el query. Los filtros de atributos usan subconsultas SQL (`Any()` / `Contains()`) que no requieren cargar los datos en memoria.

```csharp
// ❌ INCORRECTO - Causa ciclo de referencia
var query = _context.Products
    .Include(p => p.ProductAttributeValues)
        .ThenInclude(pav => pav.ProductAttribute)
    .AsQueryable();

// ✅ CORRECTO - Los filtros usan subconsultas SQL
var query = _context.Products
    .Include(p => p.Stock)
    .Include(p => p.BrandNavigation)
    .AsQueryable();

// El filtro funciona sin Include:
query = query.Where(p =>
    p.ProductAttributeValues.Any(pav =>
        pav.AttributeId == attributeId &&
        values.Contains(pav.ValueId.Value.ToString())
    )
);
// Se traduce a: WHERE EXISTS (SELECT 1 FROM ProductAttributeValues ...)
```

**Beneficio adicional**: Mejor performance al no cargar datos innecesarios.

## Troubleshooting

### No se filtran productos

1. Verificar que los atributos existen en la base de datos:
```sql
SELECT * FROM ProductAttributes WHERE AttributeId = 107
SELECT * FROM AttributeValues WHERE ValueId IN (1056, 1036)
```

2. Verificar que los productos tienen valores asignados:
```sql
SELECT p.NameSpanish, pav.*
FROM Products p
JOIN ProductAttributeValues pav ON p.ProductId = pav.ProductId
WHERE pav.AttributeId = 107
```

3. Verificar logs del Gateway y Catalog Service

### Error de compilación

Asegurarse que:
- `using System.Collections.Generic;` está agregado
- Los tipos `Dictionary<string, List<string>>` son correctos
- Los navigation properties (`AttributeValue`, no `Value`) son correctos

### Productos no se actualizan

- Limpiar caché de Redis: `docker exec -it redis redis-cli FLUSHALL`
- Reiniciar servicios después de cambios en código

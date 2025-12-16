# Comparación: /products/search vs /products/search/advanced

## 📊 Resumen Ejecutivo

| Aspecto | `/products/search` (Simple) | `/products/search/advanced` (Avanzado) |
|---------|----------------------------|---------------------------------------|
| **Método HTTP** | `GET` | `POST` |
| **Uso recomendado** | Búsquedas simples, UI básica | Filtros complejos, facetas |
| **Filtros** | Básicos (10 filtros) | Avanzados (20+ filtros) |
| **Categorías** | 1 categoría (ID) | Múltiples categorías (array) |
| **Marcas** | String separado por comas | Array de IDs |
| **Atributos** | ❌ No soporta | ✅ Diccionario dinámico |
| **Facetas** | ❌ No retorna | ✅ Retorna facetas |
| **Complejidad** | Baja | Alta |
| **Performance** | Más rápido | Más lento (pero con facetas) |

---

## 🔍 `/products/search` - Búsqueda Simple

### Propósito
Búsquedas rápidas y simples con filtros básicos. Ideal para:
- Barra de búsqueda principal
- Búsquedas por texto
- Filtros básicos (precio, stock, descuento)
- UI simple sin facetas

### Método
```http
GET /products/search
```

### Parámetros (ProductSearchRequest)

#### Básicos
```
Query          : string     - Texto de búsqueda (nombre, descripción, SKU, marca)
Page           : int        - Número de página (default: 1)
PageSize       : int        - Items por página (default: 24, max: 100)
```

#### Ordenamiento
```
SortBy         : enum       - Relevance, Name, Price, Newest, Bestseller, Rating, Discount
SortOrder      : enum       - Ascending, Descending
```

#### Filtros
```
CategoryId     : int?       - UNA categoría (ej: 5)
BrandIds       : string     - Marcas separadas por coma (ej: "Apple,Samsung,LG")
MinPrice       : decimal?   - Precio mínimo
MaxPrice       : decimal?   - Precio máximo
InStock        : bool?      - Solo productos en stock
IsFeatured     : bool?      - Solo productos destacados
HasDiscount    : bool?      - Solo productos con descuento
MinRating      : decimal?   - Rating mínimo (0-5)
```

### Ejemplo de Request
```http
GET /products/search?Query=tv&Page=1&PageSize=24&HasDiscount=true&MinPrice=100&MaxPrice=1000&SortBy=Price&SortOrder=Ascending
```

### Respuesta (ProductSearchResponse)
```json
{
  "items": [
    {
      "productId": 123,
      "name": "Samsung TV 55\"",
      "price": 599.99,
      "discountPercentage": 15.00,
      "hasDiscount": true,
      ...
    }
  ],
  "total": 150,
  "page": 1,
  "pages": 7,
  "metadata": {
    "filters": {
      "query": "tv",
      "hasDiscount": true,
      "minPrice": 100,
      "maxPrice": 1000
    },
    "executionTime": 150
  }
}
```

---

## 🎯 `/products/search/advanced` - Búsqueda Avanzada

### Propósito
Búsquedas complejas con filtros múltiples y facetas dinámicas. Ideal para:
- Páginas de catálogo con filtros avanzados
- Filtros por atributos personalizados
- UI con facetas (ej: Amazon, eBay)
- Filtros múltiples de categorías y marcas

### Método
```http
POST /products/search/advanced
```

### Parámetros (ProductAdvancedSearchRequest)

#### Hereda de ProductSearchRequest
Todo lo del endpoint simple, MÁS:

#### Filtros Múltiples
```
CategoryIds              : List<int>                      - Múltiples categorías (ej: [1, 2, 5])
BrandIds                 : List<int>                      - Múltiples marcas (ej: [10, 15, 20])
```

#### Atributos Dinámicos
```
Attributes               : Dictionary<string, List<string>>  - Atributos personalizados
                           Ej: {
                             "ScreenSize": ["50-59", "60-69"],
                             "Resolution": ["4K", "8K"],
                             "SmartTV": ["true"],
                             "HDR": ["HDR10", "Dolby Vision"]
                           }
```

#### Rangos Numéricos
```
AttributeRanges          : Dictionary<string, NumericRangeDto>
                           Ej: {
                             "ScreenSize": { "Min": 50, "Max": 70 },
                             "Weight": { "Min": 10, "Max": 25 }
                           }
```

#### Filtros de Rating
```
MinAverageRating         : decimal?   - Rating mínimo promedio
MinReviewCount           : int?       - Cantidad mínima de reviews
```

#### Filtros de Disponibilidad
```
IsPreOrder               : bool?      - Solo pre-órdenes
ShipsInternational       : bool?      - Envío internacional
```

#### Filtros de Descuento
```
MinDiscountPercentage    : decimal?   - Descuento mínimo requerido
```

#### Facetas (Agregaciones)
```
IncludeBrandFacets       : bool       - Incluir facetas de marcas (default: true)
IncludePriceFacets       : bool       - Incluir facetas de precio (default: true)
IncludeAttributeFacets   : bool       - Incluir facetas de atributos (default: true)
IncludeCategoryFacets    : bool       - Incluir facetas de categorías (default: true)
IncludeRatingFacets      : bool       - Incluir facetas de rating (default: true)
```

### Ejemplo de Request
```http
POST /products/search/advanced
Content-Type: application/json

{
  "query": "tv",
  "page": 1,
  "pageSize": 24,
  "sortBy": "Price",
  "sortOrder": "Ascending",
  "categoryIds": [1, 2, 5],
  "brandIds": [10, 15, 20],
  "minPrice": 100,
  "maxPrice": 1000,
  "hasDiscount": true,
  "attributes": {
    "ScreenSize": ["50-59", "60-69"],
    "Resolution": ["4K", "8K"],
    "SmartTV": ["true"]
  },
  "minAverageRating": 4.0,
  "minReviewCount": 10,
  "minDiscountPercentage": 10,
  "includeBrandFacets": true,
  "includePriceFacets": true,
  "includeAttributeFacets": true,
  "includeCategoryFacets": true,
  "includeRatingFacets": true
}
```

### Respuesta (ProductAdvancedSearchResponse)
```json
{
  "items": [...],
  "total": 150,
  "page": 1,
  "pages": 7,
  "facets": {
    "brands": [
      { "id": 10, "name": "Samsung", "count": 45 },
      { "id": 15, "name": "LG", "count": 32 },
      { "id": 20, "name": "Sony", "count": 28 }
    ],
    "categories": [
      { "id": 1, "name": "TVs", "count": 80 },
      { "id": 2, "name": "Smart TVs", "count": 60 },
      { "id": 5, "name": "4K TVs", "count": 50 }
    ],
    "priceRanges": [
      { "range": "0-500", "count": 20 },
      { "range": "500-1000", "count": 45 },
      { "range": "1000-2000", "count": 60 },
      { "range": "2000+", "count": 25 }
    ],
    "attributes": {
      "ScreenSize": [
        { "value": "50-59", "count": 35 },
        { "value": "60-69", "count": 28 },
        { "value": "70-79", "count": 15 }
      ],
      "Resolution": [
        { "value": "4K", "count": 80 },
        { "value": "8K", "count": 25 },
        { "value": "1080p", "count": 45 }
      ],
      "SmartTV": [
        { "value": "true", "count": 120 },
        { "value": "false", "count": 30 }
      ]
    },
    "ratings": [
      { "rating": 5, "count": 45 },
      { "rating": 4, "count": 60 },
      { "rating": 3, "count": 30 },
      { "rating": 2, "count": 10 },
      { "rating": 1, "count": 5 }
    ]
  },
  "metadata": {
    "filters": {...},
    "performance": {
      "queryExecutionTime": 150,
      "facetCalculationTime": 80,
      "totalExecutionTime": 230,
      "cacheHit": false
    }
  }
}
```

---

## 🎭 Casos de Uso

### Use `/products/search` (Simple) cuando:

✅ **Barra de búsqueda simple**
```javascript
// Usuario busca "laptop"
fetch('/products/search?Query=laptop&Page=1&PageSize=20')
```

✅ **Filtros básicos en UI simple**
```javascript
// Filtrar por precio y descuento
fetch('/products/search?Query=tv&HasDiscount=true&MinPrice=300&MaxPrice=800')
```

✅ **Búsqueda rápida sin facetas**
```javascript
// Solo necesitas productos, sin agregaciones
fetch('/products/search?Query=mouse&InStock=true')
```

✅ **Performance crítico**
```javascript
// Quieres respuesta rápida, sin cálculos extras
fetch('/products/search?Query=keyboard&Page=1')
```

---

### Use `/products/search/advanced` (Avanzado) cuando:

✅ **UI de catálogo con filtros laterales** (como Amazon)
```javascript
// Necesitas facetas para mostrar opciones de filtros
fetch('/products/search/advanced', {
  method: 'POST',
  body: JSON.stringify({
    query: 'tv',
    categoryIds: [1, 2],
    includeBrandFacets: true,
    includeAttributeFacets: true
  })
})
```

✅ **Filtros por atributos personalizados**
```javascript
// Filtrar por tamaño de pantalla, resolución, etc.
fetch('/products/search/advanced', {
  method: 'POST',
  body: JSON.stringify({
    query: 'tv',
    attributes: {
      "ScreenSize": ["50-59", "60-69"],
      "Resolution": ["4K"]
    }
  })
})
```

✅ **Múltiples categorías o marcas**
```javascript
// Buscar en varias categorías a la vez
fetch('/products/search/advanced', {
  method: 'POST',
  body: JSON.stringify({
    query: 'gaming',
    categoryIds: [10, 15, 20],  // PCs, Consolas, Accesorios
    brandIds: [5, 8, 12]         // Razer, Logitech, Corsair
  })
})
```

✅ **Necesitas facetas para UI dinámica**
```javascript
// Mostrar contadores en cada filtro
// Ej: "Samsung (45)", "LG (32)", "Sony (28)"
fetch('/products/search/advanced', {
  method: 'POST',
  body: JSON.stringify({
    query: 'laptop',
    includeBrandFacets: true,
    includePriceFacets: true
  })
})
```

---

## ⚡ Diferencias de Performance

### `/products/search` (Simple)
```
Tiempo típico: 50-150ms

Operaciones:
1. Query builder      : 5ms
2. Database query     : 80ms
3. Mapping DTOs       : 20ms
4. Cache write        : 10ms
Total                 : ~115ms
```

### `/products/search/advanced` (Avanzado)
```
Tiempo típico: 150-400ms

Operaciones:
1. Query builder      : 10ms
2. Database query     : 100ms
3. Facet calculation  : 150ms  ← EXTRA
4. Mapping DTOs       : 30ms
5. Cache write        : 15ms
Total                 : ~305ms
```

**⚠️ El cálculo de facetas agrega 100-200ms** dependiendo de la cantidad de facetas solicitadas.

---

## 🔧 Cuándo Migrar de Simple a Avanzado

### Escenario 1: UI Simple → UI con Filtros

**Antes** (Simple):
```html
<input type="text" name="query" placeholder="Buscar...">
<select name="category">
  <option>Todas las categorías</option>
</select>
<button>Buscar</button>
```

**Después** (Avanzado):
```html
<input type="text" name="query" placeholder="Buscar...">

<!-- Facetas dinámicas -->
<div class="filters">
  <h5>Marcas</h5>
  <label><input type="checkbox" value="10"> Samsung (45)</label>
  <label><input type="checkbox" value="15"> LG (32)</label>
  
  <h5>Precio</h5>
  <label><input type="checkbox" value="0-500"> $0 - $500 (20)</label>
  <label><input type="checkbox" value="500-1000"> $500 - $1000 (45)</label>
  
  <h5>Atributos</h5>
  <label><input type="checkbox" value="4K"> 4K (80)</label>
  <label><input type="checkbox" value="8K"> 8K (25)</label>
</div>
```

---

## 📊 Matriz de Decisión

| Característica | Simple | Avanzado |
|----------------|--------|----------|
| Texto de búsqueda | ✅ | ✅ |
| Paginación | ✅ | ✅ |
| Ordenamiento | ✅ | ✅ |
| 1 Categoría | ✅ | ✅ |
| Múltiples Categorías | ❌ | ✅ |
| Marcas (string) | ✅ | ❌ |
| Marcas (array IDs) | ❌ | ✅ |
| Precio min/max | ✅ | ✅ |
| Stock | ✅ | ✅ |
| Destacados | ✅ | ✅ |
| Descuento (bool) | ✅ | ✅ |
| Descuento (min %) | ❌ | ✅ |
| Rating mínimo | ✅ | ✅ |
| Reviews mínimas | ❌ | ✅ |
| Atributos personalizados | ❌ | ✅ |
| Rangos de atributos | ❌ | ✅ |
| Pre-orden | ❌ | ✅ |
| Envío internacional | ❌ | ✅ |
| **Facetas de marcas** | ❌ | ✅ |
| **Facetas de categorías** | ❌ | ✅ |
| **Facetas de precio** | ❌ | ✅ |
| **Facetas de atributos** | ❌ | ✅ |
| **Facetas de rating** | ❌ | ✅ |

---

## 💡 Recomendaciones

### Para tu caso actual (Clients.WebClient)

Tu UI actual en `Pages/Products/Index.cshtml` tiene filtros por:
- ❌ Resolución (atributo dinámico)
- ❌ Año del modelo (atributo dinámico)
- ❌ Condición (atributo dinámico)
- ❌ Tipo de montaje (atributo dinámico)
- ❌ Conectividad (atributo dinámico)
- ✅ Descuento (soportado por ambos)

**Conclusión**: Deberías usar **`/products/search/advanced`** porque:
1. ✅ Soporta atributos dinámicos
2. ✅ Retorna facetas con contadores
3. ✅ Mejor para tu UI con múltiples filtros

### Cambio Sugerido

**En Index.cshtml.cs**, cambiar de:
```csharp
var response = await _productProxy.SearchAsync(searchRequest);
```

**A**:
```csharp
var advancedRequest = new ProductAdvancedSearchRequest
{
    Query = SearchQuery,
    Page = page,
    PageSize = 24,
    HasDiscount = HasDiscount,
    Attributes = new Dictionary<string, List<string>>
    {
        { "Resolution", SelectedResolutions },
        { "Year", SelectedYears },
        { "Condition", SelectedConditions },
        { "MountType", SelectedMountTypes },
        { "Connectivity", SelectedConnectivity }
    },
    IncludeBrandFacets = true,
    IncludeAttributeFacets = true,
    IncludePriceFacets = true
};

var response = await _productProxy.SearchAdvancedAsync(advancedRequest);
```

---

## ✅ Resumen Final

### `/products/search` - Simple
- ✅ Búsquedas rápidas básicas
- ✅ UI simple sin facetas
- ✅ Performance óptimo
- ❌ No soporta atributos dinámicos
- ❌ No retorna facetas

### `/products/search/advanced` - Avanzado
- ✅ Filtros complejos
- ✅ Atributos dinámicos
- ✅ Facetas con contadores
- ✅ Ideal para catálogos estilo Amazon/eBay
- ⚠️ Más lento (~2-3x) por cálculo de facetas

### Recomendación para tu proyecto
**Usar `/products/search/advanced`** porque tu UI ya tiene filtros por atributos (Resolución, Año, Condición, etc.) que solo soporta el endpoint avanzado.
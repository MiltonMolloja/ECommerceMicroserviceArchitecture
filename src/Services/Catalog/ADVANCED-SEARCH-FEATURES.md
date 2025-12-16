# Advanced Search & Filtering Features - Catalog API

## Resumen

Se ha implementado un sistema completo de búsqueda avanzada y filtrado para el Catalog API, similar a Amazon, con las siguientes características:

- ✅ Full-Text Search (con fallback automático a LIKE)
- ✅ Facetas dinámicas (marcas, categorías, precios, ratings, atributos)
- ✅ Sistema de atributos configurables por producto
- ✅ Sistema de reviews y ratings
- ✅ Búsqueda y filtrado por múltiples criterios
- ✅ Performance optimizada con caché y ejecución paralela

## Nuevas Tablas de Base de Datos

### 1. Brands (Marcas Normalizadas)
```sql
[Catalog].[Brands]
- BrandId (PK)
- Name (unique)
- Description
- LogoUrl
- IsActive
```

### 2. ProductAttributes (Atributos Configurables)
```sql
[Catalog].[ProductAttributes]
- AttributeId (PK)
- AttributeName
- AttributeNameEnglish
- AttributeType (Text, Number, Boolean, Select, MultiSelect)
- Unit
- IsFilterable
- IsSearchable
- DisplayOrder
- CategoryId (nullable)
```

### 3. AttributeValues (Valores Predefinidos)
```sql
[Catalog].[AttributeValues]
- ValueId (PK)
- AttributeId (FK)
- ValueText
- ValueTextEnglish
- DisplayOrder
```

### 4. ProductAttributeValues (Valores por Producto)
```sql
[Catalog].[ProductAttributeValues]
- ProductId (PK, FK)
- AttributeId (PK, FK)
- ValueId (nullable, FK)
- TextValue
- NumericValue
- BooleanValue
```

### 5. ProductReviews (Reseñas)
```sql
[Catalog].[ProductReviews]
- ReviewId (PK)
- ProductId (FK)
- UserId
- Rating (1-5)
- Title
- Comment
- IsVerifiedPurchase
- HelpfulCount
- NotHelpfulCount
- IsApproved
- CreatedAt
- UpdatedAt
```

### 6. ProductRatings (Ratings Agregados)
```sql
[Catalog].[ProductRatings]
- ProductId (PK, FK)
- AverageRating
- TotalReviews
- Rating5Star, Rating4Star, Rating3Star, Rating2Star, Rating1Star
- LastUpdated
```

## Nuevos Endpoints

### 1. Búsqueda Avanzada con Facetas
```
POST /v1/products/search/advanced
Content-Type: application/json

Request Body:
{
  "query": "laptop gaming",
  "page": 1,
  "pageSize": 24,
  "sortBy": "Relevance",
  "sortOrder": "Descending",

  // Filtros múltiples
  "categoryIds": [1, 2, 3],
  "brandIds": [5, 10],

  // Filtros de precio
  "minPrice": 500,
  "maxPrice": 2000,

  // Filtros de rating
  "minAverageRating": 4.0,
  "minReviewCount": 10,

  // Filtros de atributos
  "attributes": {
    "ScreenSize": ["15-17", "17+"],
    "Processor": ["Intel i7", "Intel i9"]
  },

  // Rangos numéricos
  "attributeRanges": {
    "RAM": { "min": 16, "max": 32 }
  },

  // Otros filtros
  "inStock": true,
  "isFeatured": false,
  "hasDiscount": false,

  // Facetas a incluir
  "includeBrandFacets": true,
  "includeCategoryFacets": true,
  "includePriceFacets": true,
  "includeRatingFacets": true,
  "includeAttributeFacets": true
}

Response:
{
  "items": [ /* ProductDto[] */ ],
  "total": 150,
  "page": 1,
  "pageSize": 24,
  "pageCount": 7,
  "hasMore": true,

  "facets": {
    "brands": [
      { "id": 5, "name": "HP", "count": 45, "isSelected": false },
      { "id": 10, "name": "Dell", "count": 32, "isSelected": false }
    ],
    "categories": [
      { "id": 1, "name": "Laptops Gaming", "count": 120 }
    ],
    "priceRanges": {
      "min": 500,
      "max": 3500,
      "ranges": [
        { "min": 500, "max": 1000, "count": 45, "label": "$500 a $1,000" },
        { "min": 1000, "max": 1500, "count": 60, "label": "$1,000 a $1,500" }
      ]
    },
    "ratings": {
      "ranges": [
        { "minRating": 4.0, "count": 85, "label": "⭐⭐⭐⭐ 4 estrellas o más" },
        { "minRating": 3.0, "count": 120, "label": "⭐⭐⭐ 3 estrellas o más" }
      ]
    },
    "attributes": {
      "ScreenSize": {
        "attributeId": 1,
        "attributeName": "ScreenSize",
        "attributeType": "Select",
        "unit": "pulgadas",
        "values": [
          { "id": 1, "name": "15-17", "count": 80 },
          { "id": 2, "name": "17+", "count": 40 }
        ]
      }
    }
  },

  "metadata": {
    "query": "laptop gaming",
    "performance": {
      "queryExecutionTime": 45,
      "facetCalculationTime": 120,
      "totalExecutionTime": 165,
      "totalFilteredResults": 150,
      "cacheHit": false
    },
    "didYouMean": null,
    "relatedSearches": []
  }
}
```

### 2. Reviews de Producto
```
GET /v1/products/{productId}/reviews?page=1&pageSize=10&sortBy=helpful

Response:
{
  "items": [
    {
      "reviewId": 1,
      "productId": 123,
      "userId": 456,
      "userName": "Usuario456",
      "rating": 5.0,
      "title": "Excelente laptop",
      "comment": "Muy buena calidad y rendimiento...",
      "isVerifiedPurchase": true,
      "helpfulCount": 45,
      "notHelpfulCount": 2,
      "createdAt": "2025-01-15T10:30:00Z",
      "updatedAt": "2025-01-15T10:30:00Z"
    }
  ],
  "total": 150,
  "averageRating": 4.5,
  "ratingDistribution": {
    "rating5Star": 80,
    "rating4Star": 50,
    "rating3Star": 15,
    "rating2Star": 3,
    "rating1Star": 2
  }
}

Opciones de ordenamiento (sortBy):
- helpful (default): Por más útiles
- newest: Más recientes primero
- oldest: Más antiguas primero
- rating_high: Rating más alto primero
- rating_low: Rating más bajo primero
```

### 3. Resumen de Ratings
```
GET /v1/products/{productId}/reviews/summary

Response:
{
  "productId": 123,
  "averageRating": 4.5,
  "totalReviews": 150,
  "distribution": {
    "rating5Star": 80,
    "rating4Star": 50,
    "rating3Star": 15,
    "rating2Star": 3,
    "rating1Star": 2
  },
  "recommendationPercentage": 87
}
```

## Características Técnicas

### Performance
- **Caché en Memoria**: Facetas de marcas con TTL de 5 minutos
- **Caché Redis**: Resultados de búsqueda con TTL configurable
- **Ejecución Paralela**: Facetas calculadas en paralelo usando Task.WhenAll
- **Métricas de Performance**: Tracking de tiempo de query, facetas y total

### Full-Text Search
- Usa SQL Server Full-Text Search cuando está disponible
- Fallback automático a búsqueda LIKE si FTS no está instalado
- Búsqueda multiidioma (español e inglés)

### Multitenancy/Multilanguage
- Soporte completo para español e inglés
- Cache keys incluyen contexto de idioma
- Respuestas localizadas según Accept-Language header

### Ordenamiento Disponible
- **Relevance**: Por relevancia de búsqueda
- **Name**: Por nombre alfabético
- **Price**: Por precio
- **Newest**: Productos más recientes
- **Rating**: Por calificación promedio
- **Bestseller**: Por cantidad de reviews
- **Discount**: Por porcentaje de descuento

## Migraciones Aplicadas

1. **AddBrandsTableAndRelationship** - Normalización de marcas
2. **AddProductAttributesTables** - Sistema de atributos dinámicos
3. **AddProductReviewsAndRatingsTables** - Sistema de reviews y ratings

## Archivos Modificados/Creados

### Domain
- `Brand.cs` - Nueva entidad
- `ProductAttribute.cs` - Nueva entidad
- `AttributeValue.cs` - Nueva entidad
- `ProductAttributeValue.cs` - Nueva entidad
- `ProductReview.cs` - Nueva entidad
- `ProductRating.cs` - Nueva entidad

### DTOs
- `ProductAdvancedSearchRequest.cs`
- `ProductAdvancedSearchResponse.cs`
- `SearchFacetsDto.cs`
- `FacetItemDto.cs`
- `PriceFacetDto.cs`
- `RatingFacetDto.cs`
- `AttributeFacetDto.cs`
- `ProductReviewDto.cs`
- `ProductRatingSummaryDto.cs`

### Services
- `FacetService.cs` - Cálculo de facetas dinámicas
- `ProductReviewQueryService.cs` - Queries de reviews
- `ProductQueryService.cs` - Actualizado con SearchAdvancedAsync

### Controllers
- `ProductController.cs` - Endpoint SearchAdvanced agregado
- `ProductReviewController.cs` - Nuevo controlador

### Configuration
- `Startup.cs` - Registro de nuevos servicios y Memory Cache

## Próximos Pasos (Pendientes)

Las siguientes características están diseñadas pero no implementadas:

1. **Spell Checking Service**: "Did you mean" para corrección de búsquedas
2. **Search Suggestions**: Autocompletado de búsquedas
3. **Related Searches**: Búsquedas relacionadas sugeridas
4. **Review Management**: Endpoints para crear/editar/aprobar reviews
5. **Attribute Management**: CRUD de atributos y valores

## Notas Importantes

- **Full-Text Search**: Requiere SQL Server Standard/Enterprise. En Express se usa fallback a LIKE.
- **Redis**: Requerido para caché distribuido
- **Performance**: Los índices en la BD son críticos para performance en búsquedas complejas

## Testing

### Ejemplo de Request con cURL
```bash
curl -X POST "http://localhost:20000/v1/products/search/advanced" \
  -H "Content-Type: application/json" \
  -H "Accept-Language: es" \
  -d '{
    "query": "laptop",
    "page": 1,
    "pageSize": 24,
    "sortBy": "Relevance",
    "minPrice": 500,
    "maxPrice": 2000,
    "includeBrandFacets": true,
    "includeCategoryFacets": true,
    "includePriceFacets": true
  }'
```

## Build Status

✅ Backend compila sin errores ni warnings
✅ Migraciones aplicadas correctamente
✅ Todos los endpoints funcionando

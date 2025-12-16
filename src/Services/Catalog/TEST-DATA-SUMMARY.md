# Resumen de Datos de Prueba - Catalog Service

## 📊 Datos Insertados

### Productos
- **Total:** 45 productos (IDs: 201-250)
- **Categorías:**
  - 🔶 Smartphones (201-210): 10 productos
  - 💻 Laptops (211-220): 10 productos
  - 👟 Zapatillas (221-230): 10 productos
  - 📺 TVs (231-240): 10 productos
  - ⌨️ Accesorios (241-250): 10 productos

### Atributos de Filtrado
Se crearon **8 atributos** con sus valores correspondientes:

| ID | Atributo | Tipo | Valores Disponibles |
|----|----------|------|---------------------|
| 101 | Color | Select | Negro, Blanco, Azul, Rojo, Gris, Plateado |
| 102 | Almacenamiento | Select | 64GB, 128GB, 256GB, 512GB, 1TB |
| 103 | Memoria RAM | Select | 4GB, 8GB, 16GB, 32GB |
| 104 | Tamaño de Pantalla | Numeric | Variable por producto (6.1" - 65") |
| 105 | Talla | Select | 38, 39, 40, 41, 42, 43 |
| 106 | Procesador | Select | Intel i5, Intel i7, Intel i9, Apple M2, Apple M3, AMD Ryzen 5 |
| 107 | Resolución | Select | Full HD, 4K Ultra HD, 8K Ultra HD |
| 108 | Conectividad | MultiSelect | Wi-Fi, Bluetooth, USB-C, Inalámbrico |

### Reviews y Ratings
- **Total Reviews:** 229 reviews distribuidas entre todos los productos
- **Total Ratings:** 45 productos tienen rating calculado
- **Distribución:** 3-7 reviews por producto
- **Ratings:** Mayormente entre 4.0 y 5.0 estrellas

---

## 🧪 Ejemplos de Prueba del Filtrado Avanzado

### 1. Filtrar Smartphones por Marca y Almacenamiento

**Endpoint:** `GET /api/products/advanced-search`

**Request Body:**
```json
{
  "brandIds": [101, 102],
  "attributeFilters": [
    {
      "attributeId": 102,
      "valueIds": [1008, 1009]
    }
  ],
  "pageNumber": 1,
  "pageSize": 20
}
```

**Resultado esperado:** Smartphones Samsung y Apple con 128GB o 256GB de almacenamiento

---

### 2. Filtrar Laptops por Procesador y RAM

**Request Body:**
```json
{
  "brandIds": [107, 108, 102],
  "attributeFilters": [
    {
      "attributeId": 106,
      "valueIds": [1030, 1031]
    },
    {
      "attributeId": 103,
      "valueIds": [1014, 1015]
    }
  ],
  "minPrice": 1000,
  "maxPrice": 2000
}
```

**Resultado esperado:** Laptops HP, Dell y Apple con procesador Intel i7 o i9 y 16GB o 32GB RAM, entre $1000 y $2000

---

### 3. Filtrar Zapatillas por Talla y Color

**Request Body:**
```json
{
  "brandIds": [105, 106],
  "attributeFilters": [
    {
      "attributeId": 105,
      "valueIds": [1026, 1027]
    },
    {
      "attributeId": 101,
      "valueIds": [1001, 1003]
    }
  ],
  "sortBy": "price",
  "sortDirection": "asc"
}
```

**Resultado esperado:** Zapatillas Nike y Adidas en tallas 41 y 42, colores negro y azul, ordenadas por precio ascendente

---

### 4. Filtrar TVs por Tamaño de Pantalla y Resolución

**Request Body:**
```json
{
  "attributeFilters": [
    {
      "attributeId": 104,
      "numericRange": {
        "min": 50,
        "max": 65
      }
    },
    {
      "attributeId": 107,
      "valueIds": [1036, 1037]
    }
  ],
  "minPrice": 600,
  "sortBy": "name"
}
```

**Resultado esperado:** TVs entre 50" y 65", con resolución 4K o 8K, precio mínimo $600

---

### 5. Filtrar por Rating Mínimo

**Request Body:**
```json
{
  "minRating": 4.5,
  "sortBy": "rating",
  "sortDirection": "desc",
  "pageNumber": 1,
  "pageSize": 10
}
```

**Resultado esperado:** Top 10 productos con rating 4.5 estrellas o superior

---

### 6. Buscar Accesorios por Conectividad (MultiSelect)

**Request Body:**
```json
{
  "brandIds": [109],
  "attributeFilters": [
    {
      "attributeId": 108,
      "valueIds": [1039, 1041]
    }
  ]
}
```

**Resultado esperado:** Accesorios Logitech con Bluetooth e Inalámbrico

---

### 7. Búsqueda con Texto + Filtros

**Request Body:**
```json
{
  "searchTerm": "gaming",
  "brandIds": [109],
  "attributeFilters": [
    {
      "attributeId": 101,
      "valueIds": [1001]
    }
  ],
  "minPrice": 50,
  "maxPrice": 150
}
```

**Resultado esperado:** Productos gaming de Logitech en color negro entre $50 y $150

---

### 8. Filtro Completo con Facetas

**Request Body:**
```json
{
  "categoryIds": [1],
  "brandIds": [101, 102, 103],
  "attributeFilters": [
    {
      "attributeId": 102,
      "valueIds": [1009, 1010]
    },
    {
      "attributeId": 103,
      "valueIds": [1013, 1014]
    }
  ],
  "minPrice": 500,
  "maxPrice": 1500,
  "minRating": 4.0,
  "sortBy": "rating",
  "sortDirection": "desc",
  "includeFacets": true,
  "pageNumber": 1,
  "pageSize": 20
}
```

**Response incluirá:**
- Lista de productos filtrados
- Facetas con contadores por:
  - Marcas disponibles
  - Atributos disponibles (Color, Almacenamiento, RAM)
  - Rangos de precio
  - Distribución de ratings

---

## 📝 Notas Importantes

1. **IDs de Prueba:**
   - Marcas: 101-110
   - Productos: 201-250
   - Atributos: 101-108
   - Valores: 1001-1050

2. **Base de Datos:**
   - Connection String: `Server=localhost\\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;`
   - Schema: `Catalog`

3. **Endpoints Disponibles:**
   - `GET /api/products/advanced-search` - Búsqueda avanzada con filtros
   - `GET /api/products/{id}/reviews` - Reviews de un producto
   - `GET /api/products/{id}/rating` - Rating de un producto

4. **Scripts SQL Disponibles:**
   - `SeedTestDataSimple.sql` - Insertar datos de prueba
   - `QueryTestData.sql` - Consultar y verificar datos

---

## 🔍 Verificación en Base de Datos

### Ver todos los atributos:
```sql
SELECT * FROM Catalog.ProductAttributes WHERE AttributeId BETWEEN 101 AND 108;
```

### Ver valores de un atributo específico:
```sql
SELECT av.*
FROM Catalog.AttributeValues av
WHERE av.AttributeId = 101 -- Color
ORDER BY av.DisplayOrder;
```

### Ver productos con sus ratings:
```sql
SELECT p.ProductId, p.NameSpanish, pr.AverageRating, pr.TotalReviews
FROM Catalog.Products p
LEFT JOIN Catalog.ProductRatings pr ON p.ProductId = pr.ProductId
WHERE p.ProductId BETWEEN 201 AND 250
ORDER BY pr.AverageRating DESC;
```

### Ver reviews de un producto:
```sql
SELECT *
FROM Catalog.ProductReviews
WHERE ProductId = 201
ORDER BY CreatedAt DESC;
```

---

## ✅ Checklist de Pruebas

- [ ] Filtrado por una marca
- [ ] Filtrado por múltiples marcas
- [ ] Filtrado por atributo Select (Color)
- [ ] Filtrado por atributo Numeric (Pantalla)
- [ ] Filtrado por rango de precio
- [ ] Filtrado por rating mínimo
- [ ] Ordenamiento por precio
- [ ] Ordenamiento por rating
- [ ] Ordenamiento por nombre
- [ ] Paginación (diferentes tamaños de página)
- [ ] Búsqueda por texto + filtros
- [ ] Múltiples filtros combinados
- [ ] Facetas en respuesta
- [ ] Performance con 50+ productos

---

¡Todos los datos están listos para probar el sistema de filtrado avanzado! 🚀

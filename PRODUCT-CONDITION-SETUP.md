# Setup: Atributo de Condición de Productos (Nuevo/Usado)

## Descripción

Este script agrega un atributo "Condición" a todos los productos activos, asignando:
- **75%** de productos como **"Nuevo"**
- **25%** de productos como **"Usado"** (selección aleatoria)

## Archivos Creados

1. **`scripts/add-product-condition.sql`** - Script SQL que crea el atributo y asigna valores
2. **`add-product-condition.bat`** - Script batch para Windows (auto-detecta Docker/local)
3. **`add-product-condition.ps1`** - Script PowerShell (recomendado, más robusto)

## Ejecución

### Opción 1: PowerShell (Recomendado)

```powershell
.\add-product-condition.ps1
```

### Opción 2: Batch

```cmd
add-product-condition.bat
```

### Opción 3: Manual con Docker

```bash
# Copiar script al contenedor
docker cp scripts/add-product-condition.sql sqlserver:/tmp/

# Ejecutar script
docker exec sqlserver /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i /tmp/add-product-condition.sql
```

### Opción 4: Manual con SQL Server local

```bash
sqlcmd -S localhost -U sa -P "MyComplexPassword123!" -d "ecommerce-db" -i "scripts\add-product-condition.sql"
```

## Qué Hace el Script

1. **Verifica** si el atributo "Condition" ya existe (lo elimina si existe)
2. **Crea** el atributo "Condition" de tipo "Select" (filtrable y buscable)
3. **Crea** dos valores:
   - "Nuevo" / "New" (ValueId generado automáticamente)
   - "Usado" / "Used" (ValueId generado automáticamente)
4. **Cuenta** el total de productos activos
5. **Calcula** el 25% de productos para marcar como usados
6. **Asigna** aleatoriamente el 25% como "Usado"
7. **Asigna** el resto (75%) como "Nuevo"
8. **Muestra** estadísticas y ejemplos de uso

## Resultado Esperado

```
=== ✅ Proceso Completado ===

Resumen:
  - AttributeId: 110
  - Valor "Nuevo": 1070
  - Valor "Usado": 1071
  - Total productos: 1000
  - Productos nuevos: 750
  - Productos usados: 250

Uso en filtros:
  GET /products/search?filter_attr_110=1070  (Nuevos)
  GET /products/search?filter_attr_110=1071  (Usados)
```

## Distribución Final

| Condición | Total | Porcentaje |
|-----------|-------|------------|
| Nuevo     | 750   | 75.00%     |
| Usado     | 250   | 25.00%     |

## Uso en la API

### Filtrar productos nuevos

```bash
# Gateway
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1070" -k

# Catalog Service directo
curl -X GET "https://localhost:44364/v1/products/search?query=laptop&filter_attr_110=1070" -k
```

### Filtrar productos usados

```bash
# Gateway
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1071" -k

# Catalog Service directo
curl -X GET "https://localhost:44364/v1/products/search?query=laptop&filter_attr_110=1071" -k
```

### Filtrar nuevos y usados (OR)

```bash
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1070&filter_attr_110=1071" -k
```

### Combinar con otros filtros

```bash
# Laptops usadas entre $500 y $1000
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1071&minPrice=500&maxPrice=1000" -k

# TVs nuevos con pantalla 55-65 pulgadas
curl -X GET "https://localhost:45000/products/search?query=tv&filter_attr_110=1070&filter_attr_107=1056" -k
```

## Uso en el Frontend

### TypeScript/Angular

```typescript
// Servicio de búsqueda
export class ProductSearchService {
  
  // Enum para condición
  enum ProductCondition {
    New = 1070,    // Reemplazar con ValueId real
    Used = 1071    // Reemplazar con ValueId real
  }
  
  // Buscar productos por condición
  searchByCondition(keyword: string, condition: ProductCondition, page: number = 1) {
    const params = new HttpParams()
      .set('query', keyword)
      .set('filter_attr_110', condition.toString())  // Reemplazar 110 con AttributeId real
      .set('page', page.toString());
    
    return this.http.get<ProductSearchResponse>('/products/search', { params });
  }
  
  // Buscar solo productos nuevos
  searchNewProducts(keyword: string, page: number = 1) {
    return this.searchByCondition(keyword, ProductCondition.New, page);
  }
  
  // Buscar solo productos usados
  searchUsedProducts(keyword: string, page: number = 1) {
    return this.searchByCondition(keyword, ProductCondition.Used, page);
  }
}
```

### Componente de filtros

```typescript
export class ProductFiltersComponent {
  conditionAttributeId = 110;  // Reemplazar con AttributeId real
  newConditionValueId = 1070;  // Reemplazar con ValueId real
  usedConditionValueId = 1071; // Reemplazar con ValueId real
  
  selectedConditions: number[] = [];
  
  // Toggle condición
  toggleCondition(valueId: number) {
    const index = this.selectedConditions.indexOf(valueId);
    if (index > -1) {
      this.selectedConditions.splice(index, 1);
    } else {
      this.selectedConditions.push(valueId);
    }
    this.applyFilters();
  }
  
  // Construir URL con filtros
  buildSearchUrl(keyword: string): string {
    const params = new URLSearchParams();
    params.set('query', keyword);
    
    this.selectedConditions.forEach(valueId => {
      params.append(`filter_attr_${this.conditionAttributeId}`, valueId.toString());
    });
    
    return `/products/search?${params.toString()}`;
  }
}
```

### HTML Template

```html
<!-- Filtro de condición -->
<div class="filter-group">
  <h3>Condición</h3>
  
  <mat-checkbox 
    [checked]="selectedConditions.includes(newConditionValueId)"
    (change)="toggleCondition(newConditionValueId)">
    Nuevo
  </mat-checkbox>
  
  <mat-checkbox 
    [checked]="selectedConditions.includes(usedConditionValueId)"
    (change)="toggleCondition(usedConditionValueId)">
    Usado
  </mat-checkbox>
</div>
```

## Después de Ejecutar

### 1. Limpiar Cache de Redis

```powershell
.\clear-redis-cache.ps1
```

O manualmente:

```bash
docker exec -it redis redis-cli FLUSHALL
```

### 2. Verificar en la Base de Datos

```sql
-- Ver distribución
SELECT 
    av.ValueSpanish,
    COUNT(*) as Total,
    CAST(COUNT(*) * 100.0 / (SELECT COUNT(*) FROM Products WHERE IsActive = 1) AS DECIMAL(5,2)) as Porcentaje
FROM ProductAttributeValues pav
INNER JOIN AttributeValues av ON pav.ValueId = av.ValueId
INNER JOIN ProductAttributes pa ON pav.AttributeId = pa.AttributeId
WHERE pa.AttributeName = 'Condition'
GROUP BY av.ValueSpanish;

-- Listar productos usados
SELECT TOP 10
    p.ProductId,
    p.NameSpanish,
    p.Price,
    av.ValueSpanish as Condicion
FROM Products p
INNER JOIN ProductAttributeValues pav ON p.ProductId = pav.ProductId
INNER JOIN AttributeValues av ON pav.ValueId = av.ValueId
INNER JOIN ProductAttributes pa ON pav.AttributeId = pa.AttributeId
WHERE pa.AttributeName = 'Condition'
  AND av.ValueEnglish = 'Used'
ORDER BY p.NameSpanish;
```

### 3. Probar en la API

```bash
# Swagger
https://localhost:45000/swagger

# Endpoint
GET /products/search?filter_attr_110=1071

# cURL
curl -X GET "https://localhost:45000/products/search?query=laptop&filter_attr_110=1071" -k
```

## Revertir Cambios

Si necesitas eliminar el atributo de condición:

```sql
USE [ecommerce-db];

DECLARE @ConditionAttributeId INT = (SELECT AttributeId FROM ProductAttributes WHERE AttributeName = 'Condition');

-- Eliminar valores de productos
DELETE FROM ProductAttributeValues WHERE AttributeId = @ConditionAttributeId;

-- Eliminar valores del atributo
DELETE FROM AttributeValues WHERE AttributeId = @ConditionAttributeId;

-- Eliminar el atributo
DELETE FROM ProductAttributes WHERE AttributeId = @ConditionAttributeId;

PRINT 'Atributo "Condition" eliminado correctamente';
```

## Notas Importantes

1. **Aleatorización**: La selección de productos usados es completamente aleatoria usando `NEWID()`
2. **Idempotencia**: El script puede ejecutarse múltiples veces, elimina datos anteriores antes de crear nuevos
3. **Solo productos activos**: Solo se asigna condición a productos con `IsActive = 1`
4. **Cache**: Después de ejecutar, limpia el cache de Redis para ver los cambios inmediatamente
5. **IDs dinámicos**: Los IDs de atributo y valores son generados automáticamente, revisa el output del script

## Troubleshooting

### Error: AttributeId ya existe

El script elimina automáticamente datos anteriores. Si el error persiste, ejecuta manualmente:

```sql
DELETE FROM ProductAttributeValues WHERE AttributeId = (SELECT AttributeId FROM ProductAttributes WHERE AttributeName = 'Condition');
DELETE FROM AttributeValues WHERE AttributeId = (SELECT AttributeId FROM ProductAttributes WHERE AttributeName = 'Condition');
DELETE FROM ProductAttributes WHERE AttributeName = 'Condition';
```

### Error: No se pueden ver los cambios

1. Limpia cache de Redis: `.\clear-redis-cache.ps1`
2. Reinicia Catalog Service: `docker-compose restart catalog.api`
3. Verifica en la base de datos que los datos existen

### Error: Docker no responde

1. Verifica que Docker Desktop esté ejecutándose
2. Verifica que el contenedor SQL Server esté activo: `docker ps`
3. Reinicia el contenedor: `docker restart sqlserver`

## Ventajas de Este Enfoque

✅ **Flexible**: Fácil agregar más condiciones (Refurbished, Open Box, etc.)  
✅ **Filtrable**: Se integra automáticamente con el sistema de filtros  
✅ **Bilingüe**: Soporta español e inglés  
✅ **Escalable**: Usa la arquitectura de atributos existente  
✅ **Consistente**: Mantiene el patrón del resto de atributos  

## Extensiones Futuras

Puedes agregar más condiciones fácilmente:

```sql
-- Reacondicionado
INSERT INTO AttributeValues (AttributeId, ValueSpanish, ValueEnglish, SortOrder)
VALUES (@ConditionAttributeId, 'Reacondicionado', 'Refurbished', 3);

-- Caja Abierta
INSERT INTO AttributeValues (AttributeId, ValueSpanish, ValueEnglish, SortOrder)
VALUES (@ConditionAttributeId, 'Caja Abierta', 'Open Box', 4);
```

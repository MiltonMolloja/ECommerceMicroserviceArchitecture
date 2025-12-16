# Fix: Filtros Avanzados Faltantes en Búsqueda de TVs

## 🔍 Problema

Al buscar "tv" en `https://localhost:4200/s?k=tv`, solo aparecen los filtros básicos:
- ✅ Precio
- ✅ Marca
- ✅ Disponibilidad
- ✅ Ofertas
- ✅ Calificación

**Faltan los filtros avanzados** (atributos dinámicos):
- ❌ Resolución de pantalla (8K, 4K, 1080p, 720p)
- ❌ Año del modelo (2024, 2023, 2022, etc.)
- ❌ Condición (Nuevo, Renovado, Usado)
- ❌ Tipo de montaje (Mesa, Pared)
- ❌ Conectividad (HDMI, Wi-Fi, USB, Bluetooth, Ethernet)

## 🔎 Diagnóstico

### Logs del Frontend
```
🔍 DEBUG: Facetas del backend: {brands: Array(8), categories: Array(0), priceRanges: {…}, ratings: {…}, attributes: {…}}
🔍 DEBUG: Atributos: {}  ← ⚠️ PROBLEMA: Objeto vacío
```

### Causa Raíz
El backend devuelve `attributes: {}` (vacío) porque:
1. **No hay atributos marcados como `IsFilterable = true`** en la base de datos, O
2. **No hay productos con atributos asignados** en `ProductAttributeValues`, O
3. **Los productos de TV no tienen atributos asignados**

## ✅ Solución

### Opción 1: Script Automatizado (RECOMENDADO)

```bash
cd C:\Source\ECommerceMicroserviceArchitecture

# Ejecutar script de instalación
install-tv-filters.bat
```

Este script ejecuta:
1. `add-product-filter-attributes.sql` - Crea los atributos filtrables
2. `assign-tv-attributes.sql` - Asigna atributos a productos de TV
3. `diagnose-tv-attributes.sql` - Verifica la instalación

### Opción 2: Manual (Paso a Paso)

#### Paso 1: Crear Atributos Filtrables

```bash
sqlcmd -S localhost -d ECommerce -E -i scripts\add-product-filter-attributes.sql
```

Este script crea:
- **Resolución** (8K, 4K, 1080p, 720p)
- **Año del Modelo** (2024-2018)
- **Condición** (Nuevo, Renovado, Usado)
- **Tipo de Montaje** (Mesa, Pared)
- **Conectividad** (HDMI, Wi-Fi, USB, Bluetooth, Ethernet)

#### Paso 2: Asignar Atributos a Productos

```bash
sqlcmd -S localhost -d ECommerce -E -i scripts\assign-tv-attributes.sql
```

Este script:
- Detecta productos de TV (nombre contiene "TV", "television", "televisor")
- Asigna atributos basándose en el nombre del producto:
  - **4K** → productos con "4K", "UHD", "Ultra HD"
  - **8K** → productos con "8K"
  - **1080p** → productos con "1080", "Full HD", "FHD"
  - **Año** → productos con "2024", "2023", etc.
  - **Smart TV** → Wi-Fi, Bluetooth, Ethernet
  - **Todos** → HDMI, USB (estándar)

#### Paso 3: Verificar Instalación

```bash
sqlcmd -S localhost -d ECommerce -E -i scripts\diagnose-tv-attributes.sql -o diagnose-tv-output.txt
type diagnose-tv-output.txt
```

Deberías ver:
```
ATRIBUTOS FILTRABLES:
- Resolución (IsFilterable = 1)
- Año del Modelo (IsFilterable = 1)
- Condición (IsFilterable = 1)
- Tipo de Montaje (IsFilterable = 1)
- Conectividad (IsFilterable = 1)

PRODUCTOS DE TV CON ATRIBUTOS:
- ProductId 1: Samsung 4K TV → Resolución: 4K, Año: 2024, etc.
- ProductId 2: LG OLED TV → Resolución: 4K, Conectividad: HDMI, Wi-Fi, etc.
```

#### Paso 4: Reiniciar Servicios

```bash
# Limpiar caché de Redis
clear-redis-cache.ps1

# O reiniciar servicios con Docker
docker-compose restart catalog-api
```

#### Paso 5: Verificar en el Frontend

1. Abrir `https://localhost:4200/s?k=tv`
2. Deberías ver los filtros avanzados en el sidebar:
   - ✅ Resolución de pantalla
   - ✅ Año del modelo
   - ✅ Condición
   - ✅ Tipo de montaje
   - ✅ Conectividad

## 🔧 Troubleshooting

### Problema 1: "attributes: {}" sigue vacío

**Causa**: Los productos no tienen atributos asignados.

**Solución**:
```sql
-- Verificar productos con atributos
SELECT COUNT(DISTINCT ProductId) FROM Catalog.ProductAttributeValues;

-- Si devuelve 0, ejecutar:
sqlcmd -S localhost -d ECommerce -E -i scripts\assign-tv-attributes.sql
```

### Problema 2: Atributos no aparecen en el frontend

**Causa**: Caché de Redis o del navegador.

**Solución**:
```bash
# Limpiar caché de Redis
clear-redis-cache.ps1

# Limpiar caché del navegador
# Chrome: Ctrl+Shift+Delete → Borrar caché
# O abrir en modo incógnito: Ctrl+Shift+N
```

### Problema 3: SQL Server no está corriendo

**Causa**: Docker no está iniciado o SQL Server no está corriendo.

**Solución**:
```bash
# Verificar Docker
docker ps

# Si no hay containers, iniciar:
docker-compose up -d

# Verificar SQL Server
docker exec -it <sql-container> /opt/mssql-tools/bin/sqlcmd -S localhost -U sa -P <password> -Q "SELECT @@VERSION"
```

### Problema 4: Error "IsFilterable not found"

**Causa**: La columna `IsFilterable` no existe en la tabla `ProductAttributes`.

**Solución**:
```sql
-- Verificar estructura de tabla
SELECT COLUMN_NAME, DATA_TYPE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'ProductAttributes' AND TABLE_SCHEMA = 'Catalog';

-- Si falta IsFilterable, agregar:
ALTER TABLE Catalog.ProductAttributes 
ADD IsFilterable BIT NOT NULL DEFAULT 0;

-- Luego ejecutar el script de atributos
```

## 📊 Verificación Final

### Query SQL para verificar todo

```sql
USE ECommerce;
GO

-- 1. Verificar atributos filtrables
SELECT 
    pa.AttributeName,
    pa.IsFilterable,
    COUNT(DISTINCT pav.ProductId) AS ProductCount
FROM Catalog.ProductAttributes pa
LEFT JOIN Catalog.ProductAttributeValues pav ON pa.AttributeId = pav.AttributeId
WHERE pa.IsFilterable = 1
GROUP BY pa.AttributeName, pa.IsFilterable
ORDER BY pa.AttributeName;

-- 2. Verificar productos de TV con atributos
SELECT 
    p.ProductId,
    p.Name,
    COUNT(DISTINCT pav.AttributeId) AS AttributeCount
FROM Catalog.Products p
LEFT JOIN Catalog.ProductAttributeValues pav ON p.ProductId = pav.ProductId
WHERE p.Name LIKE '%TV%' OR p.Name LIKE '%television%'
GROUP BY p.ProductId, p.Name
ORDER BY AttributeCount DESC;
```

### Test HTTP para verificar API

```http
POST http://localhost:5011/products/search/advanced
Content-Type: application/json
Accept-Language: es

{
  "query": "tv",
  "page": 1,
  "pageSize": 24,
  "includeBrandFacets": true,
  "includeCategoryFacets": true,
  "includePriceFacets": true,
  "includeRatingFacets": true,
  "includeAttributeFacets": true
}
```

**Respuesta esperada**:
```json
{
  "facets": {
    "brands": [...],
    "categories": [...],
    "priceRanges": {...},
    "ratings": {...},
    "attributes": {
      "Resolución": {
        "attributeId": 1,
        "attributeName": "Resolución",
        "attributeType": "Select",
        "values": [
          { "id": 1, "name": "4K Ultra HD", "count": 15 },
          { "id": 2, "name": "8K Ultra HD", "count": 3 },
          { "id": 3, "name": "Full HD 1080p", "count": 8 }
        ]
      },
      "Año del Modelo": {
        "attributeId": 2,
        "attributeName": "Año del Modelo",
        "attributeType": "Select",
        "values": [
          { "id": 5, "name": "2024", "count": 10 },
          { "id": 6, "name": "2023", "count": 8 }
        ]
      }
      // ... más atributos
    }
  }
}
```

## 🎯 Resultado Esperado

Después de aplicar la solución, el frontend debería mostrar:

```
Filtros de Búsqueda
├── Precio
│   └── [Slider de rango]
├── Marca
│   ├── ☐ Samsung (15)
│   ├── ☐ LG (12)
│   └── ☐ Sony (8)
├── Disponibilidad
│   └── ☐ En stock
├── Ofertas
│   └── ☐ Con descuento
├── Calificación
│   ├── ☐ ⭐⭐⭐⭐ 4 estrellas o más
│   └── ☐ ⭐⭐⭐ 3 estrellas o más
├── Resolución de pantalla ← ✅ NUEVO
│   ├── ☐ 8K (3)
│   ├── ☐ 4K (15)
│   ├── ☐ 1080p (8)
│   └── ☐ 720p (2)
├── Año del modelo ← ✅ NUEVO
│   ├── ☐ 2024 (10)
│   ├── ☐ 2023 (8)
│   ├── ☐ 2022 (5)
│   └── ▼ Ver más
├── Condición ← ✅ NUEVO
│   ├── ☐ Nuevo (20)
│   ├── ☐ Renovado (5)
│   └── ☐ Usado (2)
├── Tipo de montaje ← ✅ NUEVO
│   ├── ☐ Montaje en Mesa (25)
│   └── ☐ Montaje en Pared (25)
└── Conectividad ← ✅ NUEVO
    ├── ☐ HDMI (28)
    ├── ☐ Wi-Fi (20)
    ├── ☐ USB (25)
    ├── ☐ Bluetooth (18)
    └── ☐ Ethernet (15)
```

## 📝 Notas Adicionales

### Logging Agregado

Se agregó logging detallado en `FacetService.cs` para diagnosticar problemas:

```csharp
Console.WriteLine($"🔍 DEBUG FacetService: ProductIds count = {productIds.Count}");
Console.WriteLine($"🔍 DEBUG FacetService: Filterable attributes count = {filterableAttributes.Count}");
Console.WriteLine($"  🔍 Attribute '{attribute.AttributeName}': {attributeFacet.Values?.Count ?? 0} values found");
Console.WriteLine($"  ✅ Added attribute '{attribute.AttributeName}' to facets");
Console.WriteLine($"  ❌ Skipped attribute '{attribute.AttributeName}' (no values or range)");
```

Estos logs aparecerán en la consola del servicio Catalog cuando se ejecute una búsqueda.

### Archivos Creados

- ✅ `scripts/diagnose-tv-attributes.sql` - Diagnóstico de atributos
- ✅ `scripts/assign-tv-attributes.sql` - Asignación de atributos a productos
- ✅ `install-tv-filters.bat` - Script de instalación automatizado
- ✅ `test-tv-attributes.http` - Tests HTTP para verificar API
- ✅ `FILTROS-AVANZADOS-FIX.md` - Esta documentación

### Referencias

- **Backend**: `Catalog.Service.Queries/Services/FacetService.cs`
- **Frontend**: `product-search.service.ts`, `facet-mapper.service.ts`
- **Modelos**: `SearchFacetsDto.cs`, `AttributeFacetDto.cs`
- **Documentación**: `FILTROS_CATALOGO_README.md`

## 🚀 Próximos Pasos

1. Ejecutar `install-tv-filters.bat`
2. Verificar con `diagnose-tv-output.txt`
3. Limpiar caché con `clear-redis-cache.ps1`
4. Probar en el frontend: `https://localhost:4200/s?k=tv`
5. Si hay problemas, revisar logs del servicio Catalog

---

**Fecha**: 2025-12-03  
**Autor**: Angular Expert Agent  
**Versión**: 1.0

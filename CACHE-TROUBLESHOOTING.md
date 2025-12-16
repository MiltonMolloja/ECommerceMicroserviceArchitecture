# Cache Troubleshooting Guide

## 🔴 Problema Reportado

**URL**: `https://localhost:4200/s?k=apple&filter_discount=true`  
**Síntoma**: Los resultados no se filtran correctamente por descuento

## 🔍 Análisis del Problema

### Arquitectura de Cache (2 Capas)

El sistema tiene **DOS capas de caché**:

```
Cliente (Frontend)
    ↓ 
Gateway.WebClient (Puerto 4200) 
    ↓ [CACHE LAYER 1] - Redis con claves "gateway:products:search:*"
    ↓
CatalogProxy
    ↓
Catalog.API (Microservicio)
    ↓ [CACHE LAYER 2] - Redis con claves "products:search:*"
    ↓
ProductQueryService
    ↓
Database
```

### Posibles Causas

#### 1. ⚠️ Nombre de Parámetro Incorrecto

La URL usa `filter_discount=true` pero el backend espera `hasDiscount=true`.

**Solución**: Cambiar el frontend para usar el parámetro correcto:

```typescript
// ❌ INCORRECTO
const url = `/s?k=apple&filter_discount=true`;

// ✅ CORRECTO
const url = `/s?k=apple&hasDiscount=true`;
```

#### 2. 🔄 Cache Desincronizado

Ambas capas de cache pueden tener datos desactualizados o conflictivos.

**Claves de Cache Generadas**:

```csharp
// Gateway Cache Key
gateway:products:search:q=apple:page=1:size=20:sort=0:0:cat=all:brands=all:price=0-max:stock=all:featured=all:discount=True:rating=all_lang=es

// Catalog API Cache Key
products:search:q=apple:page=1:size=20:sort=0:0:cat=all:brands=all:price=0-max:stock=all:featured=all:discount=True:rating=all_lang=es
```

#### 3. 📝 Binding del Modelo

ASP.NET Core puede tener problemas con el binding de `bool?` desde query strings.

**Verificar Request DTO**:

```csharp
public class ProductSearchRequest
{
    public bool? HasDiscount { get; set; }  // ✅ Nullable bool
}
```

**Query String válida**:
- `hasDiscount=true` → `HasDiscount = true`
- `hasDiscount=false` → `HasDiscount = false`  
- Sin parámetro → `HasDiscount = null`

## ✅ Soluciones

### Solución 1: Limpiar Cache de Redis

Ejecuta el script PowerShell para limpiar el cache:

```powershell
.\clear-redis-cache.ps1
```

**Opción 1**: Limpiar solo cache de búsquedas (recomendado)
**Opción 2**: Limpiar toda la base de datos de Redis

### Solución 2: Verificar Logs

Habilita logs detallados para ver qué está pasando:

**Catalog.API - ProductController.cs**:
```csharp
_logger.LogInformation($"Search request: {@request}");
_logger.LogInformation($"Cache key generated: {cacheKey}");
_logger.LogInformation($"Cache hit: {cachedResult != null}");
```

**Gateway.WebClient - ProductController.cs**:
```csharp
_logger.LogInformation($"Gateway: Search request: {@request}");
_logger.LogInformation($"Gateway: Cache key: {cacheKey}");
```

### Solución 3: Deshabilitar Cache Temporalmente

Para debugging, puedes deshabilitar el cache temporalmente:

**appsettings.Development.json** (Catalog.API y Gateway.WebClient):

```json
{
  "CacheSettings": {
    "CacheExpirationMinutes": 0  // 0 = Cache deshabilitado
  }
}
```

### Solución 4: Usar Headers HTTP para Bypass de Cache

Agrega soporte para bypass de cache con headers:

```csharp
// En ProductController.cs
[HttpGet("search")]
public async Task<ActionResult<ProductSearchResponse>> Search([FromQuery] ProductSearchRequest request)
{
    // Bypass cache si el header X-Bypass-Cache está presente
    var bypassCache = Request.Headers.ContainsKey("X-Bypass-Cache");
    
    if (!bypassCache)
    {
        var cachedResult = await _cacheService.GetAsync<ProductSearchResponse>(cacheKey);
        if (cachedResult != null)
            return Ok(cachedResult);
    }
    
    // ... resto del código
}
```

**Uso**:
```bash
curl -H "X-Bypass-Cache: true" "https://localhost:5001/api/v1/products/search?k=apple&hasDiscount=true"
```

### Solución 5: Endpoint de Invalidación de Cache

Ya existe un endpoint en Catalog.API para limpiar cache:

```bash
POST https://localhost:5001/api/v1/products/admin/clear-search-cache
```

**Con curl**:
```bash
curl -X POST https://localhost:5001/api/v1/products/admin/clear-search-cache
```

**Con PowerShell**:
```powershell
Invoke-RestMethod -Method POST -Uri "https://localhost:5001/api/v1/products/admin/clear-search-cache"
```

## 🔧 Verificación del Problema

### 1. Verificar Parámetros en Request

Agrega logging temporal:

```csharp
[HttpGet("search")]
public async Task<ActionResult<ProductSearchResponse>> Search([FromQuery] ProductSearchRequest request)
{
    // ⚠️ DEBUGGING - Remover en producción
    Console.WriteLine($"===== SEARCH REQUEST =====");
    Console.WriteLine($"Query: {request.Query}");
    Console.WriteLine($"HasDiscount: {request.HasDiscount}");
    Console.WriteLine($"HasDiscount.HasValue: {request.HasDiscount.HasValue}");
    Console.WriteLine($"HasDiscount.Value: {request.HasDiscount?.ToString() ?? "null"}");
    Console.WriteLine($"==========================");
    
    // ... resto del código
}
```

### 2. Verificar Claves de Redis

Conecta a Redis CLI y busca claves:

```bash
# Conectar a Redis
redis-cli

# Buscar claves de búsqueda
KEYS *search*

# Ver el contenido de una clave específica
GET "gateway:products:search:q=apple:page=1:size=20:sort=0:0:cat=all:brands=all:price=0-max:stock=all:featured=all:discount=True:rating=all_lang=es"

# Ver TTL de una clave
TTL "gateway:products:search:..."

# Eliminar una clave específica
DEL "gateway:products:search:..."
```

### 3. Verificar SQL Query Generado

Habilita SQL logging en EF Core:

**appsettings.Development.json**:
```json
{
  "Logging": {
    "LogLevel": {
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

Esto mostrará el SQL generado en los logs:

```sql
SELECT [p].[ProductId], [p].[NameSpanish], ...
FROM [Catalog].[Products] AS [p]
WHERE [p].[DiscountPercentage] > 0  -- ✅ Filtro de descuento aplicado
```

## 📋 Checklist de Diagnóstico

- [ ] ¿El parámetro de URL es correcto? (`hasDiscount` no `filter_discount`)
- [ ] ¿El valor es booleano válido? (`true`, `false`, no `1` o `0`)
- [ ] ¿Redis está corriendo? (`redis-cli PING` debe responder `PONG`)
- [ ] ¿Las claves de cache son diferentes para cada combinación de filtros?
- [ ] ¿El filtro se está aplicando correctamente en el query SQL?
- [ ] ¿Hay errores en los logs de Catalog.API o Gateway?

## 🎯 Mejoras Recomendadas

### 1. Agregar Versionado a Claves de Cache

```csharp
private const string CACHE_VERSION = "v2";

private string GenerateSearchCacheKey(ProductSearchRequest request)
{
    var keyBuilder = new StringBuilder($"products:search:{CACHE_VERSION}:");
    // ... resto del código
}
```

**Beneficio**: Puedes invalidar todo el cache cambiando la versión.

### 2. Implementar Cache Tags

```csharp
// Al guardar en cache, agregar tags
await _cacheService.SetAsync(
    cacheKey, 
    result, 
    TimeSpan.FromMinutes(5),
    tags: new[] { "products", "search", $"category:{request.CategoryId}" }
);

// Invalidar por tag
await _cacheService.InvalidateByTagAsync("products");
```

### 3. Monitoreo de Cache Hit/Miss Ratio

```csharp
private void LogCacheMetrics(bool cacheHit)
{
    var metrics = new
    {
        CacheHit = cacheHit,
        Timestamp = DateTime.UtcNow,
        Endpoint = "Search"
    };
    
    _logger.LogInformation("Cache metrics: {@Metrics}", metrics);
}
```

## 🔗 Referencias

- `src/Gateways/Api.Gateway.WebClient/Controllers/ProductController.cs` (línea 103)
- `src/Services/Catalog/Catalog.Api/Controllers/ProductController.cs` (línea 117)
- `src/Services/Catalog/Catalog.Service.Queries/ProductQueryService.cs` (línea 205)
- `clear-redis-cache.ps1` (script de limpieza)

## ✅ Solución Rápida

```powershell
# 1. Limpiar cache
.\clear-redis-cache.ps1
# Seleccionar opción 1 (solo búsquedas)

# 2. Reiniciar servicios
docker-compose restart

# 3. Probar con la URL correcta
# https://localhost:4200/s?k=apple&hasDiscount=true
```

## 📞 Soporte Adicional

Si el problema persiste:

1. Captura los logs de ambos servicios
2. Ejecuta una búsqueda con `hasDiscount=true`
3. Busca en los logs:
   - "Search request:" - Ver el request completo
   - "Cache key generated:" - Ver la clave de cache
   - "Cache hit:" - Ver si usó cache
   - SQL queries - Ver si el filtro se aplicó

4. Comparte los logs para análisis

---

## 🔧 Nueva Funcionalidad: Deshabilitar Cache Temporalmente

### Scripts Disponibles

#### Deshabilitar Cache
```bash
# PowerShell
.\disable-cache.ps1

# Batch (Windows)
disable-cache.bat
```

#### Habilitar Cache
```bash
# PowerShell
.\enable-cache.ps1

# Batch (Windows)
enable-cache.bat
```

#### Probar Configuración
```bash
.\test-cache-disable.ps1
```

### Cómo Funciona

1. **NoCacheService**: Implementación de `ICacheService` que no almacena nada
2. **Configuración Dinámica**: Verifica `CacheSettings:Disabled` en `appsettings.json`
3. **Scripts Automáticos**: Modifican la configuración en todos los servicios

### Servicios Afectados

- `Catalog.Api` - Cache de productos y búsquedas
- `Api.Gateway.WebClient` - Cache del gateway
- `Order.Api` - Cache de órdenes
- `Payment.Api` - Cache de pagos

### Verificación

Con cache deshabilitado verás:
- Logs: "Products retrieved from database" (no "cache hit")
- Respuestas: `CacheHit: false` en metadatos
- Tiempo: 100-300ms (vs 10-50ms con cache)

### Problemas Comunes

#### Cache sigue funcionando
```bash
# Verificar configuración
.\test-cache-disable.ps1

# Reiniciar servicios
# Los cambios requieren restart
```

#### Error de PowerShell
```bash
# Cambiar política de ejecución
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

#### Scripts no encuentran archivos
```bash
# Ejecutar desde raíz del proyecto
cd C:\Source\ECommerceMicroserviceArchitecture
.\disable-cache.ps1
```

### Restaurar Cache

```bash
# Opción 1: Script
.\enable-cache.ps1

# Opción 2: Git
git checkout -- src/Services/*/appsettings.json

# Opción 3: Manual
# Cambiar "Disabled": true a "Disabled": false
```

**⚠️ Importante**: Siempre reinicia los servicios después de cambiar la configuración.

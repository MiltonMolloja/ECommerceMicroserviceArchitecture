# Debug: Filtro de Descuento No Funciona

## 🔴 Problema Reportado

**URL del Frontend**: `https://localhost:4200/s?k=tv&filter_discount=true`

**Request Generado**: `https://localhost:4200/api/products/search?Page=1&PageSize=5&Query=tv&SortBy=0&SortOrder=0`

**Problema**: Falta el parámetro `HasDiscount=true` en el request al API

## 🔍 Análisis del Flujo

### 1. Frontend (Razor Page)
**Archivo**: `src/Clients/Clients.WebClient/Pages/Products/Index.cshtml.cs`

```csharp
// Línea 41-51: Parámetros del método OnGetAsync
public async Task OnGetAsync(
    string query = "",
    string k = "",
    int page = 1,
    string resolutions = "",
    string years = "",
    string conditions = "",
    string mountTypes = "",
    string connectivity = "",
    bool hasDiscount = false,        // ⚠️ Parámetro hasDiscount
    bool filter_discount = false)    // ⚠️ Parámetro filter_discount
{
    // Línea 57: Combina ambos parámetros
    HasDiscount = hasDiscount || filter_discount;
    
    // Línea 67-75: Construcción del request
    var searchRequest = new ProductSearchRequest
    {
        Query = SearchQuery,
        Page = page,
        PageSize = 24,
        SortBy = ProductSortField.Relevance,
        SortOrder = SortOrder.Descending,
        HasDiscount = HasDiscount ? (bool?)true : null  // ✅ Correcto
    };
}
```

**Estado**: ✅ La lógica parece correcta

### 2. ProductProxy (Client-side Proxy)
**Archivo**: `src/Gateways/Api.Gateway.WebClient.Proxy/ProductProxy.cs`

```csharp
// Línea 80-81: Construcción de query string
if (searchRequest.HasDiscount.HasValue)
    queryParams.Add($"hasDiscount={searchRequest.HasDiscount.Value}");
```

**Estado**: ✅ La lógica parece correcta

### 3. Posibles Causas

#### Causa 1: El parámetro `filter_discount` no se está capturando
- **Verificar**: ¿ASP.NET Core está leyendo el parámetro `filter_discount` de la URL?
- **Query String**: `?k=tv&filter_discount=true`
- **Binding**: Por defecto, ASP.NET Core hace binding case-insensitive

#### Causa 2: El parámetro se está perdiendo en el routing
- **Verificar**: ¿La ruta `/s` está mapeada correctamente?
- **Verificar**: ¿Hay algún middleware que esté modificando los parámetros?

#### Causa 3: JavaScript está haciendo un nuevo request sin parámetros
- **Verificar**: ¿Hay algún código JavaScript que esté sobrescribiendo el request?
- **Archivo potencial**: `wwwroot/js/*.js`

## 🧪 Pruebas de Debugging

### Test 1: Verificar Parámetros en OnGetAsync
Agregué logging en `Index.cshtml.cs` líneas 59-62:

```csharp
_logger.LogInformation($"🔍 OnGetAsync - Parámetros recibidos:");
_logger.LogInformation($"   hasDiscount: {hasDiscount}");
_logger.LogInformation($"   filter_discount: {filter_discount}");
_logger.LogInformation($"   HasDiscount (propiedad): {HasDiscount}");
```

### Test 2: Verificar URL Generada en ProductProxy
Agregué logging en `ProductProxy.cs` líneas 90-91:

```csharp
_logger.LogInformation($"🌐 ProductProxy - URL generada: {url}");
_logger.LogInformation($"🔍 ProductProxy - HasDiscount en request: {searchRequest.HasDiscount}");
```

### Test 3: Prueba Manual
```bash
# Test directo con parámetro hasDiscount
https://localhost:4200/s?k=tv&hasDiscount=true

# Test con filter_discount
https://localhost:4200/s?k=tv&filter_discount=true

# Test con ambos
https://localhost:4200/s?k=tv&hasDiscount=true&filter_discount=true
```

## 📋 Checklist de Verificación

- [ ] Verificar logs del método `OnGetAsync`
- [ ] Verificar logs del `ProductProxy`
- [ ] Verificar la URL que se muestra en los logs
- [ ] Verificar si hay JavaScript modificando el request
- [ ] Verificar el archivo `.cshtml` (la vista)
- [ ] Verificar routing en `Startup.cs` o `Program.cs`

## 🔧 Posibles Soluciones

### Solución 1: Verificar el archivo .cshtml
El problema puede estar en el HTML/JavaScript que genera los links o hace las peticiones.

**Archivo a revisar**: `src/Clients/Clients.WebClient/Pages/Products/Index.cshtml`

Buscar:
- Links a productos con filtros
- Código JavaScript que haga requests AJAX
- Formularios que envíen parámetros

### Solución 2: Verificar Routing
**Archivo a revisar**: `src/Clients/Clients.WebClient/Startup.cs` o `Program.cs`

Verificar que la ruta `/s` esté mapeada a `Pages/Products/Index.cshtml`

### Solución 3: Agregar Logging Adicional
Ya agregado en los archivos anteriores. Ejecutar la aplicación y revisar logs.

## 📊 Resultado Esperado

Después de aplicar los cambios de logging, deberías ver en los logs:

```
🔍 OnGetAsync - Parámetros recibidos:
   hasDiscount: False
   filter_discount: True
   HasDiscount (propiedad): True
📤 Request a enviar - HasDiscount: True
🌐 ProductProxy - URL generada: https://localhost:4200/api/products/search?query=tv&page=1&pageSize=24&sortBy=Relevance&sortOrder=Descending&hasDiscount=True
🔍 ProductProxy - HasDiscount en request: True
```

Si `filter_discount: False` en los logs, entonces el problema es que el parámetro no se está capturando desde la URL.

## 🚀 Próximos Pasos

1. Compilar y ejecutar la aplicación
2. Navegar a `https://localhost:4200/s?k=tv&filter_discount=true`
3. Revisar los logs en la consola
4. Compartir los logs para análisis

## 📁 Archivos Modificados

- ✅ `src/Clients/Clients.WebClient/Pages/Products/Index.cshtml.cs` - Agregado logging
- ✅ `src/Gateways/Api.Gateway.WebClient.Proxy/ProductProxy.cs` - Agregado logging

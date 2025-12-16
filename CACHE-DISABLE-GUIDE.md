# Guía para Deshabilitar Cache Temporalmente

Esta guía te explica cómo deshabilitar el cache temporalmente en todos los microservicios para pruebas y debugging.

## 🚀 Uso Rápido

### Deshabilitar Cache
```bash
# Opción 1: Script PowerShell
.\disable-cache.ps1

# Opción 2: Script Batch (Windows)
disable-cache.bat
```

### Habilitar Cache
```bash
# Opción 1: Script PowerShell
.\enable-cache.ps1

# Opción 2: Script Batch (Windows)
enable-cache.bat
```

### Probar Configuración
```bash
.\test-cache-disable.ps1
```

## 📋 Servicios Afectados

Los siguientes servicios tienen cache habilitado y serán afectados:

1. **Catalog.Api** - Cache de productos, búsquedas y facetas
2. **Api.Gateway.WebClient** - Cache de respuestas del gateway
3. **Order.Api** - Cache de órdenes
4. **Payment.Api** - Cache de pagos

## 🔧 Cómo Funciona

### 1. NoCacheService
Se creó una implementación de `ICacheService` que simula el cache pero no almacena nada:

```csharp
public class NoCacheService : ICacheService
{
    public Task<T> GetAsync<T>(string key)
    {
        // Siempre retorna default (como si no hubiera cache)
        return Task.FromResult(default(T)!);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan? expiration = null)
    {
        // No hace nada (no guarda en cache)
        return Task.CompletedTask;
    }
    // ... más métodos
}
```

### 2. Configuración Dinámica
Se modificó `RedisCacheExtensions.cs` para verificar la configuración:

```csharp
public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
{
    // Verificar si el cache está deshabilitado
    var cacheDisabled = configuration.GetValue<bool>("CacheSettings:Disabled", false);
    
    if (cacheDisabled)
    {
        // Usar NoCacheService cuando está deshabilitado
        services.AddSingleton<ICacheService, NoCacheService>();
        return services;
    }
    
    // ... configuración normal de Redis
}
```

### 3. Scripts de Automatización
Los scripts modifican los archivos `appsettings.json` agregando:

```json
{
  "CacheSettings": {
    "Disabled": true,
    "CacheExpirationMinutes": 1
  }
}
```

## 📝 Pasos Manuales (Alternativa)

Si prefieres hacerlo manualmente, agrega esta configuración a cada `appsettings.json`:

### Catalog.Api
```json
{
  "CacheSettings": {
    "Disabled": true
  }
}
```

### Gateway.WebClient
```json
{
  "CacheSettings": {
    "Disabled": true
  }
}
```

### Order.Api
```json
{
  "CacheSettings": {
    "Disabled": true
  }
}
```

### Payment.Api
```json
{
  "CacheSettings": {
    "Disabled": true
  }
}
```

## ⚠️ Importante

1. **Reinicia los servicios** después de cambiar la configuración
2. **No olvides habilitar el cache** cuando termines las pruebas
3. **El cache de facetas** (MemoryCache) en Catalog.Api no se ve afectado por esta configuración
4. **Redis sigue funcionando** - solo se deshabilita el uso del cache en la aplicación

## 🧪 Verificación

### Comprobar que el cache está deshabilitado:
1. Ejecuta una búsqueda de productos
2. Verifica en los logs que no aparezcan mensajes de "cached" o "cache hit"
3. Las respuestas deberían tener `CacheHit: false` en los metadatos

### Ejemplo de log sin cache:
```
[INFO] Products retrieved from database: 25 items
[INFO] Search completed in 150ms
```

### Ejemplo de log con cache:
```
[INFO] Products cached: gateway:products:search:q=laptop for 1 minutes
[INFO] Search completed in 15ms (cache hit)
```

## 🔄 Restaurar Cache

Para volver a la configuración original:

```bash
.\enable-cache.ps1
```

O manualmente, cambia `"Disabled": true` a `"Disabled": false` en todos los archivos.

## 🐛 Troubleshooting

### El cache sigue funcionando después de deshabilitar
- Verifica que reiniciaste todos los servicios
- Comprueba que los archivos `appsettings.json` tienen `"Disabled": true`

### Error al ejecutar scripts
- Ejecuta PowerShell como administrador
- Verifica la política de ejecución: `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`

### Los scripts no encuentran los archivos
- Ejecuta los scripts desde la raíz del proyecto
- Verifica que las rutas en los scripts sean correctas

## 📊 Impacto en Performance

Con cache deshabilitado:
- **Búsquedas de productos**: +100-200ms por request
- **Obtener producto por ID**: +50-100ms por request
- **Listado de productos**: +80-150ms por request
- **Órdenes y pagos**: +30-80ms por request

Esto es normal y esperado para pruebas y debugging.
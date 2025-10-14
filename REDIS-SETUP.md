# Redis Cache - Setup Guide

## 🚀 Inicio Rápido

### Opción 1: Docker (Recomendado)

1. **Iniciar Redis con Docker Compose:**
```bash
docker-compose up -d
```

2. **Verificar que Redis está corriendo:**
```bash
docker ps
```

3. **Ver logs de Redis:**
```bash
docker logs ecommerce_redis
```

4. **Detener Redis:**
```bash
docker-compose down
```

### Opción 2: Instalación Local

#### Windows:
1. Descargar Redis for Windows desde: https://github.com/microsoftarchive/redis/releases
2. Ejecutar redis-server.exe

#### Linux/Mac:
```bash
# Ubuntu/Debian
sudo apt-get install redis-server
sudo systemctl start redis

# Mac
brew install redis
brew services start redis
```

## 📊 Monitoreo de Redis

### Conectar a Redis CLI:
```bash
docker exec -it ecommerce_redis redis-cli
```

### Comandos útiles:
```bash
# Ver todas las keys
KEYS *

# Ver una key específica
GET Catalog_product_1

# Ver información del servidor
INFO

# Limpiar toda la caché
FLUSHALL

# Ver memoria usada
INFO memory
```

## 🎯 Configuración por Servicio

### Catalog API (Puerto 20000)
- **InstanceName:** `Catalog_`
- **Cachea:** Productos, Stock

### Customer API (Puerto 30000)
- **InstanceName:** `Customer_`
- **Cachea:** Datos de clientes

### Order API (Puerto 40000)
- **InstanceName:** `Order_`
- **Cachea:** Órdenes recientes

## 🔧 Configuración

La configuración de Redis está en `appsettings.json` de cada API:

```json
"Redis": {
  "ConnectionString": "localhost:6379",
  "InstanceName": "ServiceName_"
}
```

## 💡 Uso del Cache Service

### Inyectar ICacheService:
```csharp
public class ProductController : ControllerBase
{
    private readonly ICacheService _cacheService;

    public ProductController(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }
}
```

### Obtener datos del caché:
```csharp
var cachedProduct = await _cacheService.GetAsync<ProductDto>("product_123");
if (cachedProduct != null)
{
    return cachedProduct;
}
```

### Guardar en caché:
```csharp
await _cacheService.SetAsync("product_123", product, TimeSpan.FromMinutes(10));
```

### Eliminar del caché:
```csharp
await _cacheService.RemoveAsync("product_123");
```

## 🐛 Troubleshooting

### Redis no se conecta:
- Verificar que Redis está corriendo: `docker ps` o `redis-cli ping`
- Verificar puerto 6379 disponible
- Revisar firewall

### Caché no funciona:
- Los servicios caen back a InMemoryCache si Redis no está disponible
- Revisar logs de la aplicación

## 📈 Performance

- **Tiempo de expiración por defecto:** 5 minutos
- **Serialización:** System.Text.Json
- **Persistencia:** Activada (appendonly yes)

## 🔒 Seguridad (Producción)

Para producción, considera:
1. Agregar password a Redis
2. Usar Redis en red privada
3. Configurar SSL/TLS
4. Limitar conexiones

```json
"Redis": {
  "ConnectionString": "server:6379,password=your-password,ssl=true"
}
```

# Guía de Rate Limiting - ECommerce Microservices

Se ha implementado exitosamente **Rate Limiting** en todos los microservicios de la arquitectura para prevenir abuso de API, ataques de fuerza bruta y garantizar la disponibilidad del servicio.

## Qué es Rate Limiting

Rate Limiting es una técnica que limita el número de peticiones que un cliente puede hacer a una API en un período de tiempo específico. Esto ayuda a:

- **Prevenir abuso**: Evita que clientes maliciosos saturen el servidor
- **Proteger contra ataques**: Mitiga ataques de fuerza bruta y DDoS
- **Garantizar disponibilidad**: Asegura que los recursos estén disponibles para todos los usuarios
- **Controlar costos**: Previene uso excesivo de recursos computacionales

## Arquitectura implementada

### Componente Common.RateLimiting

Se creó un componente reutilizable que todos los microservicios pueden usar:

```
src/Common/Common.RateLimiting/
├── Common.RateLimiting.csproj
├── RateLimitingSettings.cs         # Configuración de políticas
└── RateLimitingExtensions.cs       # Extensiones para registrar el middleware
```

## Políticas de Rate Limiting

Se han definido **4 políticas** principales con diferentes límites:

### 1. Authentication Policy (más restrictiva)
- **Límite**: 5 peticiones
- **Ventana**: 60 segundos (1 minuto)
- **Uso**: Endpoints de autenticación (login, register, refresh-token)
- **Propósito**: Prevenir ataques de fuerza bruta

### 2. Write Policy (restrictiva)
- **Límite**: 50 peticiones
- **Ventana**: 60 segundos (1 minuto)
- **Uso**: Endpoints de escritura (POST, PUT, DELETE)
- **Propósito**: Proteger operaciones que modifican datos

### 3. Read Policy (permisiva)
- **Límite**: 200 peticiones
- **Ventana**: 60 segundos (1 minuto)
- **Uso**: Endpoints de lectura (GET)
- **Propósito**: Permitir consultas frecuentes pero con límite

### 4. General Policy
- **Límite**: 100 peticiones
- **Ventana**: 60 segundos (1 minuto)
- **Uso**: Endpoints sin política específica
- **Propósito**: Límite general por defecto

### Política Global (por IP)
- **Límite**: 500 peticiones
- **Ventana**: 60 segundos (1 minuto)
- **Aplicación**: Automática para todas las peticiones
- **Partición**: Por dirección IP del cliente

## Configuración

### appsettings.json

Todos los microservicios tienen la siguiente configuración:

```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "Authentication": {
      "PermitLimit": 5,
      "WindowInSeconds": 60
    },
    "General": {
      "PermitLimit": 100,
      "WindowInSeconds": 60
    },
    "Read": {
      "PermitLimit": 200,
      "WindowInSeconds": 60
    },
    "Write": {
      "PermitLimit": 50,
      "WindowInSeconds": 60
    }
  }
}
```

### Configuración por entorno

Puedes ajustar los límites según el entorno:

**Development** (más permisivo):
```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "Authentication": {
      "PermitLimit": 10,
      "WindowInSeconds": 60
    },
    "Write": {
      "PermitLimit": 100,
      "WindowInSeconds": 60
    },
    "Read": {
      "PermitLimit": 500,
      "WindowInSeconds": 60
    }
  }
}
```

**Production** (más restrictivo):
```json
{
  "RateLimiting": {
    "EnableRateLimiting": true,
    "Authentication": {
      "PermitLimit": 3,
      "WindowInSeconds": 60
    },
    "Write": {
      "PermitLimit": 30,
      "WindowInSeconds": 60
    },
    "Read": {
      "PermitLimit": 100,
      "WindowInSeconds": 60
    }
  }
}
```

## Aplicación en endpoints

### Aplicar políticas con atributos

En los controladores, usa el atributo `[EnableRateLimiting]`:

```csharp
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("v1/products")]
public class ProductController : ControllerBase
{
    // Endpoint de lectura - 200 req/min
    [HttpGet]
    [EnableRateLimiting("read")]
    public async Task<IActionResult> GetAll()
    {
        // ...
    }

    // Endpoint de escritura - 50 req/min
    [HttpPost]
    [EnableRateLimiting("write")]
    public async Task<IActionResult> Create([FromBody] CreateProductCommand command)
    {
        // ...
    }

    // Endpoint de autenticación - 5 req/min
    [HttpPost("authentication")]
    [EnableRateLimiting("authentication")]
    public async Task<IActionResult> Login([FromBody] LoginCommand command)
    {
        // ...
    }
}
```

### Deshabilitar rate limiting en un endpoint

```csharp
using Microsoft.AspNetCore.RateLimiting;

[HttpGet("health")]
[DisableRateLimiting]  // No aplica rate limiting
public IActionResult HealthCheck()
{
    return Ok("Healthy");
}
```

## Microservicios con Rate Limiting

| Microservicio | Rate Limiting | Configuración |
|--------------|---------------|---------------|
| **Identity.Api** | ✅ Habilitado | authentication, read, write |
| **Catalog.Api** | ✅ Habilitado | read, write |
| **Customer.Api** | ✅ Habilitado | read, write |
| **Order.Api** | ✅ Habilitado | read, write |

## Respuestas de Rate Limiting

### Cuando se excede el límite

**Status Code**: `429 Too Many Requests`

**Response Body**:
```json
{
  "error": "Too many requests",
  "message": "Rate limit exceeded. Please try again later.",
  "retryAfter": 30.5
}
```

### Headers de respuesta

```http
HTTP/1.1 429 Too Many Requests
X-RateLimit-Limit: 100
X-RateLimit-Remaining: 0
X-RateLimit-Reset: 1634567890
Retry-After: 30
```

## Ejemplos de uso

### 1. Probar rate limiting en Login

```bash
# Hacer 6 intentos de login en menos de 1 minuto
for i in {1..6}; do
  curl -X POST http://localhost:10000/v1/identity/authentication \
    -H "Content-Type: application/json" \
    -d '{"email":"test@test.com","password":"wrong"}' \
    -w "\nRequest $i: %{http_code}\n"
  sleep 1
done
```

**Resultado esperado**:
- Requests 1-5: `400 Bad Request` (contraseña incorrecta)
- Request 6: `429 Too Many Requests` (rate limit excedido)

### 2. Probar rate limiting en GET

```bash
# Hacer 201 peticiones a productos
for i in {1..201}; do
  curl http://localhost:20000/v1/products \
    -H "Authorization: Bearer {token}" \
    -w "\nRequest $i: %{http_code}\n" &
done
wait
```

**Resultado esperado**:
- Requests 1-200: `200 OK`
- Request 201: `429 Too Many Requests`

### 3. Manejar rate limiting en cliente JavaScript

```javascript
async function makeRequestWithRetry(url, options, maxRetries = 3) {
  let retries = 0;

  while (retries < maxRetries) {
    try {
      const response = await fetch(url, options);

      if (response.status === 429) {
        const data = await response.json();
        const retryAfter = data.retryAfter || 60;

        console.log(`Rate limited. Retrying after ${retryAfter} seconds...`);
        await new Promise(resolve => setTimeout(resolve, retryAfter * 1000));

        retries++;
        continue;
      }

      return response;
    } catch (error) {
      console.error('Request failed:', error);
      throw error;
    }
  }

  throw new Error('Max retries exceeded');
}

// Uso
const response = await makeRequestWithRetry('http://localhost:10000/v1/identity/authentication', {
  method: 'POST',
  headers: { 'Content-Type': 'application/json' },
  body: JSON.stringify({ email: 'user@test.com', password: 'password123' })
});
```

### 4. Cliente C# con Polly

```csharp
using Polly;
using Polly.Extensions.Http;

public class ApiClient
{
    private readonly HttpClient _httpClient;

    public ApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: (retryAttempt, response, context) =>
                {
                    // Si hay header Retry-After, usarlo
                    if (response.Result?.Headers.RetryAfter?.Delta.HasValue == true)
                    {
                        return response.Result.Headers.RetryAfter.Delta.Value;
                    }

                    // Exponential backoff por defecto
                    return TimeSpan.FromSeconds(Math.Pow(2, retryAttempt));
                },
                onRetryAsync: async (outcome, timespan, retryAttempt, context) =>
                {
                    Console.WriteLine($"Rate limited. Retry {retryAttempt} after {timespan.TotalSeconds}s");
                });
    }

    public async Task<string> LoginAsync(string email, string password)
    {
        var policy = GetRetryPolicy();

        var response = await policy.ExecuteAsync(async () =>
        {
            return await _httpClient.PostAsJsonAsync("/v1/identity/authentication",
                new { email, password });
        });

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

## Monitoreo y métricas

### Logs de rate limiting

El sistema automáticamente registra cuando se excede un límite:

```
[2025-10-14 10:30:45] [WARN] Rate limit exceeded for IP 192.168.1.100 on endpoint /v1/identity/authentication
[2025-10-14 10:30:45] [INFO] Client will retry after 35 seconds
```

### Métricas recomendadas

Implementa métricas para monitorear:

1. **Número de peticiones rechazadas por rate limiting**
2. **IPs más afectadas por rate limiting**
3. **Endpoints más limitados**
4. **Promedio de tiempo de espera (Retry-After)**

## Consideraciones de seguridad

### 1. Rate Limiting no reemplaza autenticación

Rate limiting es una capa adicional de seguridad, no reemplaza:
- Autenticación JWT
- Autorización basada en roles
- Validación de entrada

### 2. Ataques distribuidos

Si un atacante usa múltiples IPs, considera:
- Rate limiting basado en cuenta de usuario (además de IP)
- Integración con servicios anti-DDoS (Cloudflare, AWS Shield)
- Análisis de comportamiento

### 3. Ambientes de prueba

En ambientes de desarrollo/testing, puedes:
```json
{
  "RateLimiting": {
    "EnableRateLimiting": false  // Deshabilitar para testing
  }
}
```

## Mejores prácticas

### 1. Informar a los clientes

Documenta los límites en tu API:
```markdown
## Rate Limits

| Endpoint | Limit | Window |
|----------|-------|--------|
| POST /v1/identity/authentication | 5 requests | 1 minute |
| GET /v1/products | 200 requests | 1 minute |
| POST /v1/orders | 50 requests | 1 minute |
```

### 2. Usar headers informativos

Los clientes pueden verificar su estado:
```javascript
const response = await fetch('/v1/products');
const remaining = response.headers.get('X-RateLimit-Remaining');
const reset = response.headers.get('X-RateLimit-Reset');

console.log(`${remaining} requests remaining until ${new Date(reset * 1000)}`);
```

### 3. Implementar colas del lado del cliente

```javascript
class RateLimitedQueue {
  constructor(maxRequestsPerMinute) {
    this.maxRequests = maxRequestsPerMinute;
    this.queue = [];
    this.processing = false;
  }

  async enqueue(requestFn) {
    return new Promise((resolve, reject) => {
      this.queue.push({ requestFn, resolve, reject });
      this.process();
    });
  }

  async process() {
    if (this.processing || this.queue.length === 0) return;

    this.processing = true;
    const { requestFn, resolve, reject } = this.queue.shift();

    try {
      const result = await requestFn();
      resolve(result);
    } catch (error) {
      reject(error);
    } finally {
      setTimeout(() => {
        this.processing = false;
        this.process();
      }, 60000 / this.maxRequests);
    }
  }
}

// Uso
const queue = new RateLimitedQueue(5); // 5 requests per minute

for (let i = 0; i < 10; i++) {
  queue.enqueue(() =>
    fetch('/v1/identity/authentication', { method: 'POST', body: data })
  );
}
```

## Personalización avanzada

### Rate limiting por usuario autenticado

```csharp
services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
    {
        // Si está autenticado, usar userId, sino IP
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var partitionKey = userId ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = userId != null ? 1000 : 100, // Más límite para usuarios autenticados
                Window = TimeSpan.FromMinutes(1)
            });
    });
});
```

### Rate limiting con Redis (distribuido)

Para ambientes con múltiples instancias, considera usar Redis:

```bash
dotnet add package AspNetCoreRateLimit
dotnet add package AspNetCoreRateLimit.Redis
```

## Troubleshooting

### Problema: Rate limiting muy restrictivo en desarrollo

**Solución**: Usa appsettings.Development.json:
```json
{
  "RateLimiting": {
    "EnableRateLimiting": false
  }
}
```

### Problema: Tests automatizados fallan por rate limiting

**Solución**: Deshabilita en ambiente de test:
```csharp
if (env.IsEnvironment("Testing"))
{
    // No usar rate limiting en tests
}
else
{
    app.UseCustomRateLimiting();
}
```

### Problema: Load balancer cuenta como una sola IP

**Solución**: Configura X-Forwarded-For:
```csharp
app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});

// Antes de UseRateLimiting
app.UseCustomRateLimiting();
```

## Próximos pasos

1. **Implementar métricas**: Integrar con Prometheus/Grafana para visualizar rate limiting
2. **Rate limiting dinámico**: Ajustar límites basado en carga del servidor
3. **Listas blancas**: Permitir IPs específicas sin límite
4. **Notificaciones**: Alertar cuando ciertos umbrales se exceden
5. **Redis distribuido**: Para ambientes con múltiples instancias

## Conclusión

Rate Limiting está completamente implementado en todos los microservicios, proporcionando:
- ✅ Protección contra abuso de API
- ✅ Prevención de ataques de fuerza bruta
- ✅ Control de recursos y costos
- ✅ Configuración flexible por entorno
- ✅ Respuestas claras al cliente
- ✅ Fácil personalización

Los clientes de la API deben implementar lógica de reintento para manejar respuestas 429 y respetar los headers `Retry-After`.

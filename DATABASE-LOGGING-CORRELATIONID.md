# Database Logging con Correlation ID

## Resumen

Se ha agregado exitosamente el campo **CorrelationId** al sistema de logging en base de datos. Ahora todos los logs guardados en la tabla `[Logging].[Logs]` incluyen el Correlation ID del request HTTP que generó ese log.

## Cambios realizados

### 1. Modificación de Common.Logging

**Archivo**: `src/Common/Common.Logging/DatabaseLogger.cs`

#### Cambios principales:

1. **Agregado IHttpContextAccessor** para acceder al contexto HTTP:
   ```csharp
   private readonly IHttpContextAccessor _httpContextAccessor;

   public DatabaseLogger(
       string categoryName,
       string connectionString,
       string serviceName,
       IHttpContextAccessor httpContextAccessor,
       Func<string, LogLevel, bool> filter)
   {
       _httpContextAccessor = httpContextAccessor;
       // ...
   }
   ```

2. **Captura del Correlation ID** en el método `SaveToDatabase`:
   ```csharp
   // Get Correlation ID from HttpContext
   string correlationId = null;
   try
   {
       var httpContext = _httpContextAccessor?.HttpContext;
       if (httpContext != null && httpContext.Items.ContainsKey("X-Correlation-ID"))
       {
           correlationId = httpContext.Items["X-Correlation-ID"]?.ToString();
       }
   }
   catch
   {
       // If we can't get the correlation ID, just continue without it
   }
   ```

3. **Actualizado el INSERT SQL** para incluir CorrelationId:
   ```csharp
   command.CommandText = @"
       INSERT INTO [Logging].[Logs]
       (Timestamp, LogLevel, Category, Message, Exception, Environment, MachineName, ServiceName, CorrelationId)
       VALUES
       (@Timestamp, @LogLevel, @Category, @Message, @Exception, @Environment, @MachineName, @ServiceName, @CorrelationId)";

   command.Parameters.AddWithValue("@CorrelationId", correlationId ?? (object)DBNull.Value);
   ```

4. **Actualizada la extensión AddDatabase**:
   ```csharp
   public static ILoggerFactory AddDatabase(
       this ILoggerFactory factory,
       string connectionString,
       string serviceName,
       IHttpContextAccessor httpContextAccessor = null,
       Func<string, LogLevel, bool> filter = null)
   {
       factory.AddProvider(new DatabaseLoggerProvider(connectionString, serviceName, httpContextAccessor, filter));
       return factory;
   }
   ```

### 2. Actualización de todos los Microservicios

Se actualizaron los archivos `Startup.cs` de los 4 microservicios para pasar el `IHttpContextAccessor` al logger:

#### Identity.Api/Startup.cs
```csharp
using Microsoft.AspNetCore.Http;  // ← Agregado

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
    loggerFactory.AddDatabase(
        Configuration.GetConnectionString("DefaultConnection"),
        "Identity.Api",
        httpContextAccessor);  // ← Agregado
}
```

#### Catalog.Api/Startup.cs
```csharp
using Microsoft.AspNetCore.Http;  // ← Agregado

public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpContextAccessor();  // ← Agregado
    // ...
}

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
    loggerFactory.AddDatabase(
        Configuration.GetConnectionString("DefaultConnection"),
        "Catalog.Api",
        httpContextAccessor);  // ← Agregado
}
```

#### Customer.Api/Startup.cs
```csharp
using Microsoft.AspNetCore.Http;  // ← Agregado

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
    loggerFactory.AddDatabase(
        Configuration.GetConnectionString("DefaultConnection"),
        "Customer.Api",
        httpContextAccessor);  // ← Agregado
}
```

#### Order.Api/Startup.cs
```csharp
using Microsoft.AspNetCore.Http;  // ← Agregado

public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
{
    var httpContextAccessor = app.ApplicationServices.GetService<IHttpContextAccessor>();
    loggerFactory.AddDatabase(
        Configuration.GetConnectionString("DefaultConnection"),
        "Order.Api",
        httpContextAccessor);  // ← Agregado
}
```

### 3. Actualización de la tabla de base de datos

**Script SQL**: `scripts/add-correlationid-to-logs.sql`

```sql
-- Add CorrelationId column to Logging.Logs table
USE [ECommerce]
GO

ALTER TABLE [Logging].[Logs]
ADD [CorrelationId] NVARCHAR(100) NULL;

-- Create index on CorrelationId for faster queries
CREATE NONCLUSTERED INDEX [IX_Logs_CorrelationId]
ON [Logging].[Logs] ([CorrelationId])
INCLUDE ([Timestamp], [LogLevel], [Message]);
```

**Ejecutar el script**:
```bash
sqlcmd -S localhost -U sa -P "P@ssw0rd123" -i scripts/add-correlationid-to-logs.sql
```

## Estructura de la tabla [Logging].[Logs]

Después de los cambios, la tabla tiene la siguiente estructura:

| Columna        | Tipo          | Descripción                                    |
|----------------|---------------|------------------------------------------------|
| Id             | INT           | Identificador único (auto-incremental)         |
| Timestamp      | DATETIME2     | Fecha y hora del log                           |
| LogLevel       | NVARCHAR(50)  | Nivel de log (Information, Warning, Error...)  |
| Category       | NVARCHAR(500) | Categoría del logger                           |
| Message        | NVARCHAR(MAX) | Mensaje del log                                |
| Exception      | NVARCHAR(MAX) | Excepción (si hay)                             |
| Environment    | NVARCHAR(50)  | Ambiente (Development, Production...)          |
| MachineName    | NVARCHAR(100) | Nombre de la máquina                           |
| ServiceName    | NVARCHAR(100) | Nombre del microservicio                       |
| **CorrelationId** | **NVARCHAR(100)** | **Correlation ID del request** ← **NUEVO**   |

## Uso

### Consultas con Correlation ID

#### 1. Obtener todos los logs de un request específico

```sql
SELECT
    Timestamp,
    ServiceName,
    LogLevel,
    Message,
    CorrelationId
FROM [Logging].[Logs]
WHERE CorrelationId = '20251014172530-abc123def456...'
ORDER BY Timestamp;
```

#### 2. Rastrear un request a través de múltiples servicios

```sql
SELECT
    Timestamp,
    ServiceName,
    LogLevel,
    Category,
    Message
FROM [Logging].[Logs]
WHERE CorrelationId = '20251014172530-abc123def456...'
ORDER BY Timestamp;

-- Resultado ejemplo:
-- 17:25:30 | Order.Api    | Information | Creating order
-- 17:25:31 | Order.Api    | Information | Calling Catalog.Api
-- 17:25:31 | Catalog.Api  | Information | Getting product 123
-- 17:25:32 | Catalog.Api  | Information | Product found
-- 17:25:32 | Order.Api    | Information | Order created
```

#### 3. Contar logs por servicio para un request

```sql
SELECT
    ServiceName,
    COUNT(*) AS LogCount,
    MIN(Timestamp) AS FirstLog,
    MAX(Timestamp) AS LastLog
FROM [Logging].[Logs]
WHERE CorrelationId = '20251014172530-abc123def456...'
GROUP BY ServiceName
ORDER BY FirstLog;
```

#### 4. Buscar errores con Correlation ID

```sql
SELECT
    Timestamp,
    ServiceName,
    Message,
    Exception,
    CorrelationId
FROM [Logging].[Logs]
WHERE LogLevel IN ('Error', 'Critical')
  AND Timestamp >= DATEADD(hour, -1, GETUTCDATE())
ORDER BY Timestamp DESC;
```

#### 5. Encontrar requests lentos (que generaron muchos logs)

```sql
SELECT
    CorrelationId,
    COUNT(*) AS LogCount,
    MIN(Timestamp) AS StartTime,
    MAX(Timestamp) AS EndTime,
    DATEDIFF(MILLISECOND, MIN(Timestamp), MAX(Timestamp)) AS DurationMs
FROM [Logging].[Logs]
WHERE Timestamp >= DATEADD(hour, -1, GETUTCDATE())
GROUP BY CorrelationId
HAVING COUNT(*) > 10
ORDER BY DurationMs DESC;
```

### Ejemplo de logs en base de datos

Después de ejecutar un request que crea un pedido:

```
| Timestamp           | ServiceName  | LogLevel    | Message                                          | CorrelationId                    |
|---------------------|--------------|-------------|--------------------------------------------------|----------------------------------|
| 2025-10-14 17:25:30 | Order.Api    | Information | Creating order for customer 123                  | 20251014172530-abc123def456...   |
| 2025-10-14 17:25:31 | Order.Api    | Information | Validating order items                           | 20251014172530-abc123def456...   |
| 2025-10-14 17:25:31 | Order.Api    | Information | Calling Catalog.Api to get product availability  | 20251014172530-abc123def456...   |
| 2025-10-14 17:25:31 | Catalog.Api  | Information | Getting product 123                              | 20251014172530-abc123def456...   |
| 2025-10-14 17:25:32 | Catalog.Api  | Information | Product found: Laptop Dell XPS 15                | 20251014172530-abc123def456...   |
| 2025-10-14 17:25:32 | Order.Api    | Information | Product available, proceeding with order         | 20251014172530-abc123def456...   |
| 2025-10-14 17:25:32 | Order.Api    | Information | Order 456 created successfully                   | 20251014172530-abc123def456...   |
```

## Beneficios

1. **Trazabilidad Completa**: Puedes rastrear un request desde que llega al gateway hasta que sale, pasando por todos los microservicios.

2. **Debugging Más Rápido**: Cuando un usuario reporta un problema, solo necesitas el Correlation ID del response header para ver todos los logs relacionados.

3. **Análisis de Rendimiento**: Puedes analizar cuánto tiempo toma un request en cada servicio y dónde están los cuellos de botella.

4. **Detección de Errores**: Identifica fácilmente en qué servicio falló una transacción distribuida.

5. **Auditoría**: Mantén un registro completo de todas las acciones realizadas para un request específico.

## Integración con herramientas de monitoreo

### Application Insights / Azure Monitor

```csharp
// Agregar Correlation ID como custom dimension
using (logger.BeginScope(new Dictionary<string, object>
{
    ["CorrelationId"] = correlationId
}))
{
    logger.LogInformation("Processing order");
}
```

### Elasticsearch / Kibana

Query para buscar todos los logs de un request:
```
correlationId:"20251014172530-abc123def456"
```

### Seq

```
correlationId = "20251014172530-abc123def456"
```

## Próximos pasos

1. **Dashboard de monitoreo**: Crear un dashboard que muestre:
   - Requests más lentos (por Correlation ID)
   - Requests con errores
   - Distribución de logs por servicio

2. **Alertas**: Configurar alertas cuando:
   - Un Correlation ID tiene muchos logs de error
   - Un request toma más de X segundos
   - Un servicio no logea durante un periodo

3. **Análisis de tendencias**: Analizar patrones de logs por Correlation ID para optimizar rutas críticas.

4. **Limpieza automática**: Implementar un job que elimine logs antiguos periódicamente, manteniendo solo los últimos 30-90 días.

## Conclusión

El sistema de logging ahora incluye Correlation IDs en la base de datos, proporcionando trazabilidad completa de requests a través de la arquitectura de microservicios. Esto facilita el debugging, análisis de rendimiento y auditoría del sistema.

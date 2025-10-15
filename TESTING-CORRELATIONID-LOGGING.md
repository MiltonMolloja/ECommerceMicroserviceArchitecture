# Testing CorrelationId in Database Logging

## Estado Actual

✅ **Columna CorrelationId agregada** a la tabla `[Logging].[Logs]` en la base de datos `ECommerceDb`
✅ **DatabaseLogger modificado** para capturar y guardar el Correlation ID del HttpContext
✅ **Todos los microservicios actualizados** con los cambios necesarios
✅ **Build exitoso** de toda la solución

## Cómo Probar

### 1. Reiniciar todos los servicios

Primero, **detén todos los procesos** de IIS Express y servicios que estén corriendo:

```bash
# Matar todos los procesos de IIS Express
cmd /c "taskkill /F /IM iisexpress.exe"

# Matar todos los procesos de dotnet
cmd /c "taskkill /F /IM dotnet.exe"
```

### 2. Iniciar un microservicio

Inicia cualquiera de los microservicios (por ejemplo, Identity.Api):

```bash
cd "C:\Source\ECommerceMicroserviceArchitecture\src\Services\Identity\Identity.Api"
dotnet run
```

Espera a que veas el mensaje:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

### 3. Hacer un request de prueba

Abre otra terminal y ejecuta un request al health check:

```bash
curl http://localhost:5000/hc
```

Esto debería generar logs en la base de datos.

### 4. Verificar logs en la base de datos

Conéctate a SQL Server y consulta los logs recientes:

```sql
-- Ver los últimos 10 logs con CorrelationId
SELECT TOP 10
    Id,
    Timestamp,
    ServiceName,
    LogLevel,
    LEFT(Message, 100) as Message,
    CorrelationId
FROM [ECommerceDb].[Logging].[Logs]
ORDER BY Id DESC;
```

**Deberías ver**:
- La columna `CorrelationId` con valores como `20251015030000-abc123...`
- Todos los logs del mismo request tienen el mismo `CorrelationId`

### 5. Probar con múltiples requests

Haz varios requests para generar diferentes Correlation IDs:

```bash
curl http://localhost:5000/hc
curl http://localhost:5000/hc
curl http://localhost:5000/hc
```

Luego consulta:

```sql
-- Ver todos los CorrelationIds únicos
SELECT
    CorrelationId,
    COUNT(*) as LogCount,
    MIN(Timestamp) as FirstLog,
    MAX(Timestamp) as LastLog
FROM [ECommerceDb].[Logging].[Logs]
WHERE CorrelationId IS NOT NULL
GROUP BY CorrelationId
ORDER BY MIN(Timestamp) DESC;
```

### 6. Probar el flujo completo (opcional)

Si quieres probar el flujo completo de Correlation ID a través de múltiples servicios:

**Terminal 1 - Identity.Api:**
```bash
cd "C:\Source\ECommerceMicroserviceArchitecture\src\Services\Identity\Identity.Api"
dotnet run
```

**Terminal 2 - Catalog.Api:**
```bash
cd "C:\Source\ECommerceMicroserviceArchitecture\src\Services\Catalog\Catalog.Api"
dotnet run --urls "http://localhost:20000"
```

**Terminal 3 - Order.Api:**
```bash
cd "C:\Source\ECommerceMicroserviceArchitecture\src\Services\Order\Order.Api"
dotnet run --urls "http://localhost:40000"
```

Luego consulta los logs para ver cómo el mismo Correlation ID aparece en múltiples servicios:

```sql
SELECT
    Timestamp,
    ServiceName,
    LogLevel,
    LEFT(Message, 150) as Message,
    CorrelationId
FROM [ECommerceDb].[Logging].[Logs]
WHERE CorrelationId = '<tu-correlation-id-aquí>'
ORDER BY Timestamp;
```

## Troubleshooting

### Problema: No se guardan logs en la base de datos

**Posible causa**: El `DatabaseLogger` falla silenciosamente si no puede conectarse a SQL Server.

**Solución**:
1. Verifica que SQL Server esté corriendo
2. Verifica la cadena de conexión en `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
3. Prueba la conexión manualmente:
   ```bash
   sqlcmd -S "localhost\SQLEXPRESS" -E -d ECommerceDb -Q "SELECT 1"
   ```

### Problema: CorrelationId es NULL en los logs

**Posible causa**: El servicio fue iniciado antes de agregar la columna a la base de datos.

**Solución**:
1. Detén el servicio
2. Verifica que la columna existe:
   ```sql
   SELECT COLUMN_NAME
   FROM INFORMATION_SCHEMA.COLUMNS
   WHERE TABLE_SCHEMA = 'Logging'
     AND TABLE_NAME = 'Logs'
     AND COLUMN_NAME = 'CorrelationId';
   ```
3. Si no existe, ejecuta el script:
   ```bash
   sqlcmd -S "localhost\SQLEXPRESS" -E -d ECommerceDb -i "scripts/add-correlationid-to-logs.sql"
   ```
4. Reinicia el servicio

### Problema: Error "The process cannot access the file because it is being used by another process"

**Causa**: Hay un proceso de IIS Express o dotnet corriendo que bloquea los archivos DLL.

**Solución**:
```bash
# Matar todos los procesos
cmd /c "taskkill /F /IM iisexpress.exe"
cmd /c "taskkill /F /IM dotnet.exe"

# Esperar 5 segundos
cmd /c "timeout /t 5 /nobreak"

# Intentar de nuevo
dotnet run
```

## Queries útiles

### Buscar logs por Correlation ID específico
```sql
SELECT
    Timestamp,
    ServiceName,
    LogLevel,
    Category,
    Message,
    Exception
FROM [ECommerceDb].[Logging].[Logs]
WHERE CorrelationId = '20251015030000-abc123...'
ORDER BY Timestamp;
```

### Buscar requests con errores
```sql
SELECT
    CorrelationId,
    COUNT(*) as ErrorCount,
    MIN(Timestamp) as FirstError,
    MAX(Timestamp) as LastError
FROM [ECommerceDb].[Logging].[Logs]
WHERE LogLevel IN ('Error', 'Critical')
  AND CorrelationId IS NOT NULL
GROUP BY CorrelationId
ORDER BY ErrorCount DESC;
```

### Ver requests más lentos (más logs)
```sql
SELECT
    CorrelationId,
    COUNT(*) as LogCount,
    MIN(Timestamp) as StartTime,
    MAX(Timestamp) as EndTime,
    DATEDIFF(MILLISECOND, MIN(Timestamp), MAX(Timestamp)) as DurationMs
FROM [ECommerceDb].[Logging].[Logs]
WHERE CorrelationId IS NOT NULL
GROUP BY CorrelationId
ORDER BY DurationMs DESC;
```

### Limpiar logs antiguos
```sql
-- Borrar logs más antiguos de 30 días
DELETE FROM [ECommerceDb].[Logging].[Logs]
WHERE Timestamp < DATEADD(DAY, -30, GETUTCDATE());
```

## Conclusión

Una vez que veas logs en la base de datos con valores en la columna `CorrelationId`, la implementación estará funcionando correctamente. Esto te permitirá rastrear requests completos a través de todos los microservicios.

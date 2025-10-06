# 🏥 Health Checks UI - Configuración Completa

## ✅ Configuración Aplicada

Se ha habilitado correctamente el **Health Checks UI** en el servicio **Catalog.Api**.

---

## 📦 Paquetes NuGet Instalados

```xml
<PackageReference Include="AspNetCore.HealthChecks.UI" Version="8.0.2" />
<PackageReference Include="AspNetCore.HealthChecks.UI.Client" Version="8.0.1" />
<PackageReference Include="AspNetCore.HealthChecks.UI.InMemory.Storage" Version="9.0.0" />
```

---

## ⚙️ Configuración en Startup.cs

### ConfigureServices

```csharp
// Health check
services.AddHealthChecks()
    .AddCheck("self", () => HealthCheckResult.Healthy())
    .AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name);

// Health Checks UI
services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(10); // Evalúa cada 10 segundos
    setup.MaximumHistoryEntriesPerEndpoint(50); // Mantiene historial de 50 entradas
    setup.AddHealthCheckEndpoint("Catalog API", "/hc");
})
.AddInMemoryStorage(); // Usa almacenamiento en memoria
```

### Configure

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    
    // Endpoint de Health Check (JSON)
    endpoints.MapHealthChecks("/hc", new HealthCheckOptions()
    {
        Predicate = _ => true,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
    
    // UI Dashboard de Health Checks
    endpoints.MapHealthChecksUI();
});
```

---

## 🎯 Endpoints Disponibles

| Endpoint | Descripción | URL |
|----------|-------------|-----|
| **Health Check JSON** | API endpoint con estado de salud | `http://localhost:20000/hc` |
| **Health Checks UI** | Dashboard visual interactivo | `http://localhost:20000/healthchecks-ui` |
| **Health Checks API** | API del UI | `http://localhost:20000/healthchecks-api` |

---

## 🚀 Cómo Usar

### 1. Iniciar el Servicio

```powershell
cd src\Services\Catalog\Catalog.Api
dotnet run --urls "http://localhost:20000"
```

### 2. Verificar Health Check (JSON)

**PowerShell:**
```powershell
Invoke-RestMethod -Uri "http://localhost:20000/hc" | ConvertTo-Json -Depth 5
```

**curl:**
```bash
curl http://localhost:20000/hc
```

**Respuesta Esperada:**
```json
{
  "status": "Healthy",
  "totalDuration": "00:00:00.0123456",
  "entries": {
    "self": {
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0001234"
    },
    "ApplicationDbContext": {
      "status": "Healthy",
      "description": null,
      "duration": "00:00:00.0098765"
    }
  }
}
```

### 3. Acceder al Dashboard UI

Abre tu navegador en:
```
http://localhost:20000/healthchecks-ui
```

El dashboard te mostrará:
- ✅ Estado actual de salud (Healthy/Degraded/Unhealthy)
- 📊 Historial de checks
- ⏱️ Tiempos de respuesta
- 🔄 Actualizaciones automáticas cada 10 segundos
- 📈 Gráficos de disponibilidad

---

## 🎨 Características del Dashboard

### Estado Visual
- 🟢 **Healthy** - Todo funciona correctamente
- 🟡 **Degraded** - Funcionando con problemas menores
- 🔴 **Unhealthy** - Servicio con problemas críticos

### Información Mostrada
- Estado general del servicio
- Estado de la base de datos (ApplicationDbContext)
- Tiempo de respuesta de cada check
- Historial de los últimos 50 checks
- Último tiempo de evaluación

---

## 📝 Configuración Adicional (appsettings.json)

El archivo `appsettings.json` ya tiene la configuración:

```json
{
  "HealthChecksUI": {
    "HealthChecks": [
      {
        "Name": "Catalog.Api",
        "Uri": "http://localhost:20000/hc"
      }
    ],
    "EvaluationTimeInSeconds": 10,
    "MinimumSecondsBetweenFailureNotifications": 60
  }
}
```

### Parámetros Configurables

| Parámetro | Descripción | Valor Actual |
|-----------|-------------|--------------|
| `EvaluationTimeInSeconds` | Frecuencia de evaluación | 10 segundos |
| `MinimumSecondsBetweenFailureNotifications` | Tiempo mínimo entre notificaciones de fallo | 60 segundos |
| `MaximumHistoryEntriesPerEndpoint` | Entradas de historial por endpoint | 50 |

---

## 🔧 Opciones de Almacenamiento

### Actual: In-Memory Storage
```csharp
.AddInMemoryStorage()
```
- ✅ Rápido y sin configuración adicional
- ⚠️ Los datos se pierden al reiniciar
- 💡 Ideal para desarrollo

### Alternativa: SQL Server Storage
```csharp
.AddSqlServerStorage("ConnectionString")
```
- ✅ Datos persistentes
- ✅ Mejor para producción
- ⚠️ Requiere base de datos adicional

---

## 📊 Checks Disponibles

### 1. Self Check
Verifica que el servicio esté respondiendo
```csharp
.AddCheck("self", () => HealthCheckResult.Healthy())
```

### 2. Database Check
Verifica la conectividad con la base de datos
```csharp
.AddDbContextCheck<ApplicationDbContext>(typeof(ApplicationDbContext).Name)
```

---

## 🚦 Agregar Más Checks (Ejemplos)

### Redis Check
```csharp
services.AddHealthChecks()
    .AddRedis("localhost:6379", name: "redis");
```

### RabbitMQ Check
```csharp
services.AddHealthChecks()
    .AddRabbitMQ("amqp://localhost", name: "rabbitmq");
```

### URL Check (API externa)
```csharp
services.AddHealthChecks()
    .AddUrlGroup(new Uri("https://api.externa.com/status"), name: "external-api");
```

---

## 🎯 Aplicar a Otros Servicios

Para habilitar Health Checks UI en los otros servicios, repite los pasos:

### 1. Identity.Api
```powershell
cd src\Services\Identity\Identity.Api
dotnet add package AspNetCore.HealthChecks.UI.InMemory.Storage
```

### 2. Customer.Api
```powershell
cd src\Services\Customer\Customer.Api
dotnet add package AspNetCore.HealthChecks.UI.InMemory.Storage
```

### 3. Order.Api
```powershell
cd src\Services\Order\Order.Api
dotnet add package AspNetCore.HealthChecks.UI.InMemory.Storage
```

Luego aplica la misma configuración en cada `Startup.cs`.

---

## 🧪 Prueba Completa

### Script de Prueba PowerShell

```powershell
# Verificar Health Check JSON
Write-Host "Verificando Health Check..." -ForegroundColor Cyan
$health = Invoke-RestMethod -Uri "http://localhost:20000/hc"
Write-Host "Estado: $($health.status)" -ForegroundColor Green

# Abrir Dashboard en navegador
Write-Host "`nAbriendo Health Checks UI Dashboard..." -ForegroundColor Cyan
Start-Process "http://localhost:20000/healthchecks-ui"
```

---

## 📚 Recursos Adicionales

- **Documentación Oficial:** https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
- **Paquetes NuGet:** https://www.nuget.org/packages/AspNetCore.HealthChecks.UI/
- **Health Checks en ASP.NET Core:** https://learn.microsoft.com/aspnet/core/host-and-deploy/health-checks

---

## ✅ Checklist de Configuración

- [x] Paquete `AspNetCore.HealthChecks.UI` instalado
- [x] Paquete `AspNetCore.HealthChecks.UI.InMemory.Storage` instalado
- [x] Servicio configurado en `ConfigureServices`
- [x] Endpoints mapeados en `Configure`
- [x] Configuración en `appsettings.json`
- [x] Checks básicos agregados (self, database)

---

**Estado:** ✅ **Health Checks UI configurado y listo para usar**

**Próximo paso:** Iniciar el servicio y acceder a `http://localhost:20000/healthchecks-ui`

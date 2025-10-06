# 🚀 Guía de Inicio Rápido - .NET 9

## ✅ Requisitos Previos

Antes de ejecutar el proyecto, asegúrate de tener instalado:

- ✅ **.NET 9 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/9.0)
- ✅ **SQL Server** (LocalDB, Express, o versión completa)
- ✅ **Visual Studio 2022** (17.8 o superior) o **VS Code**
- ✅ **Git** (para control de versiones)

### Verificar instalación de .NET 9

```powershell
dotnet --version
# Debe mostrar: 9.0.x
```

```powershell
dotnet --list-sdks
# Debe incluir una versión 9.0.x
```

---

## 📦 Paso 1: Restaurar Paquetes NuGet

Desde la raíz del proyecto, ejecuta:

```powershell
cd c:\Source\ECommerceMicroserviceArchitecture
dotnet restore
```

---

## 🔨 Paso 2: Compilar la Solución

```powershell
dotnet build
```

Si todo está correcto, deberías ver:

```
Build succeeded in X.Xs
```

---

## 🗄️ Paso 3: Configurar las Bases de Datos

### 3.1 Actualizar las Cadenas de Conexión

Actualiza las cadenas de conexión en cada `appsettings.json`:

**Ubicaciones:**
- `src/Services/Identity/Identity.Api/appsettings.json`
- `src/Services/Catalog/Catalog.Api/appsettings.json`
- `src/Services/Customer/Customer.Api/appsettings.json`
- `src/Services/Order/Order.Api/appsettings.json`

**Ejemplo de cadena de conexión:**

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ECommerce.Identity;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 3.2 Ejecutar las Migraciones

#### **Identity Service**

```powershell
cd src/Services/Identity/Identity.Api
dotnet ef database update --context ApplicationDbContext
```

#### **Catalog Service**

```powershell
cd src/Services/Catalog/Catalog.Api
dotnet ef database update --context ApplicationDbContext
```

#### **Customer Service**

```powershell
cd src/Services/Customer/Customer.Api
dotnet ef database update --context ApplicationDbContext
```

#### **Order Service**

```powershell
cd src/Services/Order/Order.Api
dotnet ef database update --context ApplicationDbContext
```

### 3.3 Insertar Usuario por Defecto

Ejecuta este script en la base de datos `ECommerce.Identity`:

```sql
INSERT INTO [Identity].[AspNetUsers] 
  ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], 
   [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], 
   [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], 
   [LockoutEnabled], [AccessFailedCount], [FirstName], [LastName]) 
VALUES 
  (N'cc7deafd-2977-4c1b-91ad-7b8d37a01ffe', 
   N'admin@gmail.com', N'admin@gmail.com', 
   N'admin@gmail.com', N'admin@gmail.com', 
   0, N'AQAAAAEAACcQAAAAEL5faIXPhAOdXYU+vAAKbF32yd2ONSGUdGJ6wo9jkhm8KKlLF/h5x0zjJbcPKt8WYg==', 
   N'PS7QHYXIO4NUC65ZYEP4SBEYOXP4DTWA', N'e955992b-abf5-41d3-b504-ec6dc0632989', 
   NULL, 0, 0, NULL, 1, 0, N'Milton', N'Molloja');
```

**Credenciales:**
- **Usuario:** admin@gmail.com
- **Contraseña:** Pa$$w0rd! (según el hash proporcionado)

---

## ⚙️ Paso 4: Configurar los Puertos

Verifica que los siguientes puertos estén disponibles:

| Servicio | Puerto | URL |
|----------|--------|-----|
| **Identity.Api** | 10000 | https://localhost:10000 |
| **Catalog.Api** | 20000 | https://localhost:20000 |
| **Customer.Api** | 30000 | https://localhost:30000 |
| **Order.Api** | 40000 | https://localhost:40000 |
| **Api.Gateway.WebClient** | 45000 | https://localhost:45000 |
| **Clients.Authentication** | 60000 | https://localhost:60000 |
| **Clients.WebClient** | 60001 | https://localhost:60001 |

### Configurar Puertos en Visual Studio

1. Abre las propiedades de cada proyecto
2. Ve a **Debug** → **Launch Profiles**
3. Configura la URL de la aplicación según la tabla anterior

### Configurar Puertos con `launchSettings.json`

Edita cada archivo `Properties/launchSettings.json`:

```json
{
  "profiles": {
    "Catalog.Api": {
      "commandName": "Project",
      "applicationUrl": "https://localhost:20000;http://localhost:20001",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

---

## 🚀 Paso 5: Ejecutar los Servicios

### Opción A: Ejecutar todos desde Visual Studio

1. Abre la solución `ECommerce.sln` en Visual Studio
2. Click derecho en la solución → **Set Startup Projects**
3. Selecciona **Multiple startup projects**
4. Configura los siguientes proyectos como **Start**:
   - Identity.Api
   - Catalog.Api
   - Customer.Api
   - Order.Api
   - Api.Gateway.WebClient
   - Clients.WebClient
   - Clients.Authentication

5. Presiona **F5** para ejecutar

### Opción B: Ejecutar manualmente desde terminal

Abre **7 terminales diferentes** y ejecuta cada servicio:

**Terminal 1 - Identity:**
```powershell
cd src/Services/Identity/Identity.Api
dotnet run
```

**Terminal 2 - Catalog:**
```powershell
cd src/Services/Catalog/Catalog.Api
dotnet run
```

**Terminal 3 - Customer:**
```powershell
cd src/Services/Customer/Customer.Api
dotnet run
```

**Terminal 4 - Order:**
```powershell
cd src/Services/Order/Order.Api
dotnet run
```

**Terminal 5 - API Gateway:**
```powershell
cd src/Gateways/Api.Gateway.WebClient
dotnet run
```

**Terminal 6 - Authentication Client:**
```powershell
cd src/Clients/Clients.Authentication
dotnet run
```

**Terminal 7 - Web Client:**
```powershell
cd src/Clients/Clients.WebClient
dotnet run
```

---

## 🧪 Paso 6: Probar la Aplicación

### Health Checks

Verifica que todos los servicios estén en línea:

- Identity: https://localhost:10000/hc
- Catalog: https://localhost:20000/hc
- Customer: https://localhost:30000/hc
- Order: https://localhost:40000/hc

Deberías ver un JSON con el estado `Healthy`.

### Acceder a la Aplicación

1. Abre el navegador en: https://localhost:60001
2. Inicia sesión con las credenciales del usuario por defecto
3. Explora las funcionalidades del e-commerce

---

## 🧪 Paso 7: Ejecutar las Pruebas

Para ejecutar todas las pruebas unitarias:

```powershell
dotnet test
```

Para ver resultados detallados:

```powershell
dotnet test --logger "console;verbosity=detailed"
```

---

## 📊 Monitoreo y Diagnóstico

### Swagger/OpenAPI

Cada microservicio expone documentación Swagger (si está configurada):

- Identity: https://localhost:10000/swagger
- Catalog: https://localhost:20000/swagger
- Customer: https://localhost:30000/swagger
- Order: https://localhost:40000/swagger

### Logs

Los logs se escriben en la consola durante el desarrollo. Para producción, están configurados con Syslog (Papertrail).

---

## ⚠️ Solución de Problemas Comunes

### Error: "The ConnectionString property has not been initialized"

**Solución:** Verifica que el `appsettings.json` tenga la cadena de conexión correcta.

### Error: "A network-related or instance-specific error occurred"

**Solución:** 
1. Verifica que SQL Server esté ejecutándose
2. Verifica que la cadena de conexión sea correcta
3. Para LocalDB: `sqllocaldb start mssqllocaldb`

### Error: "Unable to bind to https://localhost:XXXXX"

**Solución:** El puerto está ocupado. Usa otro puerto o libera el puerto actual:

```powershell
# Ver qué proceso usa el puerto
netstat -ano | findstr :20000

# Terminar el proceso (reemplaza PID con el número del proceso)
taskkill /PID <PID> /F
```

### Error: "Certificate not valid"

**Solución:** Confía en el certificado de desarrollo de ASP.NET:

```powershell
dotnet dev-certs https --trust
```

### Los servicios no se comunican entre sí

**Solución:** Verifica que:
1. Todos los servicios estén ejecutándose
2. Los puertos en `appsettings.json` coincidan con los servicios en ejecución
3. Las URLs en `ApiUrls` estén configuradas correctamente

---

## 🔧 Configuración Adicional

### Configurar Secrets (Opcional pero Recomendado)

Para no exponer claves secretas en `appsettings.json`:

```powershell
cd src/Services/Identity/Identity.Api
dotnet user-secrets init
dotnet user-secrets set "SecretKey" "molloja-ecommerce-secret-key-super-secure-2025-minimum-32chars"
```

Repite para cada microservicio.

### Configurar CORS (Si es necesario)

Edita el `Startup.cs` de cada API para permitir orígenes específicos:

```csharp
services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});
```

---

## 📚 Recursos Adicionales

- [Documentación oficial de .NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/overview)
- [Entity Framework Core 9](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew)
- [ASP.NET Core 9 Documentation](https://learn.microsoft.com/en-us/aspnet/core/)
- [Curso original en Udemy](https://www.udemy.com/course/microservicios-con-net-core-3-hasta-su-publicacion-en-azure/)

---

## 🐛 Reporte de Problemas

Si encuentras algún problema:

1. Verifica que hayas seguido todos los pasos
2. Revisa los logs en la consola de cada servicio
3. Consulta la sección de solución de problemas

---

## ✅ Checklist de Verificación

- [ ] .NET 9 SDK instalado y verificado
- [ ] SQL Server instalado y funcionando
- [ ] Todas las cadenas de conexión actualizadas
- [ ] Migraciones ejecutadas en todas las bases de datos
- [ ] Usuario por defecto insertado
- [ ] Todos los servicios compilados sin errores
- [ ] Certificados HTTPS confiables
- [ ] Puertos disponibles y configurados
- [ ] Health checks retornan "Healthy"
- [ ] Aplicación web accesible y funcional
- [ ] Pruebas unitarias pasando exitosamente

---

**¡Listo! Tu aplicación de microservicios con .NET 9 está funcionando. 🎉**

---

**Última actualización:** Octubre 4, 2025  
**Versión .NET:** 9.0

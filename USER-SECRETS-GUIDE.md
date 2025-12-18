# User Secrets Configuration Guide

## ¿Qué son los User Secrets?

**User Secrets** es una característica de .NET que permite almacenar información sensible (contraseñas, API keys, connection strings) **fuera del código fuente** del proyecto, en tu perfil de usuario.

### Ventajas
✅ **Seguridad:** Las credenciales nunca se suben al repositorio  
✅ **Simplicidad:** Fácil de configurar y usar  
✅ **Por desarrollador:** Cada dev tiene sus propias credenciales  
✅ **Integrado:** Funciona automáticamente con .NET  

### Ubicación de los Secrets
- **Windows:** `%APPDATA%\Microsoft\UserSecrets\<user_secrets_id>\secrets.json`
- **Linux/Mac:** `~/.microsoft/usersecrets/<user_secrets_id>/secrets.json`

---

## Quick Start

### 1. Configuración Automática (Recomendado)

```powershell
# Inicializar y configurar todos los servicios con valores por defecto
.\scripts\setup-user-secrets.ps1 -Init -SetDefaults

# Ver los secrets configurados
.\scripts\setup-user-secrets.ps1 -List
```

### 2. Configuración Manual

```bash
# Inicializar User Secrets en un proyecto
dotnet user-secrets init --project src/Services/Order/Order.Api

# Agregar un secret
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..." --project src/Services/Order/Order.Api

# Ver todos los secrets
dotnet user-secrets list --project src/Services/Order/Order.Api

# Eliminar un secret
dotnet user-secrets remove "ConnectionStrings:DefaultConnection" --project src/Services/Order/Order.Api

# Limpiar todos los secrets
dotnet user-secrets clear --project src/Services/Order/Order.Api
```

---

## Secrets Requeridos por Servicio

### 🔹 Todos los Servicios

```bash
# JWT Secret Key (mínimo 32 caracteres)
dotnet user-secrets set "SecretKey" "your-super-secure-jwt-secret-key-min-32-chars" --project <path>

# SQL Server Connection String
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True" --project <path>

# Redis
dotnet user-secrets set "Redis:ConnectionString" "localhost:6379" --project <path>

# API Key (para comunicación entre servicios)
dotnet user-secrets set "ApiKey:ApiKey" "your-secure-api-key" --project <path>

# RabbitMQ
dotnet user-secrets set "RabbitMQ:Host" "localhost" --project <path>
dotnet user-secrets set "RabbitMQ:Username" "guest" --project <path>
dotnet user-secrets set "RabbitMQ:Password" "guest" --project <path>
```

### 📧 Notification.Api (Adicionales)

```bash
# SMTP Configuration
dotnet user-secrets set "SmtpSettings:Host" "smtp.gmail.com" --project src/Services/Notification/Notification.Api
dotnet user-secrets set "SmtpSettings:Port" "587" --project src/Services/Notification/Notification.Api
dotnet user-secrets set "SmtpSettings:Username" "your-email@gmail.com" --project src/Services/Notification/Notification.Api
dotnet user-secrets set "SmtpSettings:Password" "your-app-password" --project src/Services/Notification/Notification.Api
dotnet user-secrets set "SmtpSettings:FromEmail" "noreply@yourdomain.com" --project src/Services/Notification/Notification.Api
dotnet user-secrets set "SmtpSettings:FromName" "ECommerce Platform" --project src/Services/Notification/Notification.Api
dotnet user-secrets set "SmtpSettings:EnableSsl" "true" --project src/Services/Notification/Notification.Api
```

**Nota para Gmail:** Necesitas crear una "App Password" en tu cuenta de Google:
1. Ve a https://myaccount.google.com/security
2. Activa "2-Step Verification"
3. Genera una "App Password" para "Mail"
4. Usa esa contraseña en `SmtpSettings:Password`

### 💳 Payment.Api (Adicionales)

```bash
# Stripe
dotnet user-secrets set "Stripe:SecretKey" "sk_test_your_stripe_secret_key" --project src/Services/Payment/Payment.Api
dotnet user-secrets set "Stripe:PublishableKey" "pk_test_your_stripe_publishable_key" --project src/Services/Payment/Payment.Api

# MercadoPago
dotnet user-secrets set "MercadoPago:AccessToken" "your_mercadopago_access_token" --project src/Services/Payment/Payment.Api
dotnet user-secrets set "MercadoPago:PublicKey" "your_mercadopago_public_key" --project src/Services/Payment/Payment.Api
```

---

## Comandos Útiles del Script

```powershell
# Inicializar todos los servicios
.\scripts\setup-user-secrets.ps1 -Init

# Configurar valores por defecto en todos los servicios
.\scripts\setup-user-secrets.ps1 -SetDefaults

# Inicializar y configurar en un solo comando
.\scripts\setup-user-secrets.ps1 -Init -SetDefaults

# Ver secrets de un servicio específico
.\scripts\setup-user-secrets.ps1 -List -Service Order

# Ver secrets de todos los servicios
.\scripts\setup-user-secrets.ps1 -List

# Configurar solo un servicio
.\scripts\setup-user-secrets.ps1 -Init -SetDefaults -Service Payment

# Limpiar secrets de un servicio
.\scripts\setup-user-secrets.ps1 -Clear -Service Order

# Limpiar todos los secrets (¡cuidado!)
.\scripts\setup-user-secrets.ps1 -Clear
```

---

## Estructura del secrets.json

Cuando configuras User Secrets, se crea un archivo JSON en tu perfil:

```json
{
  "SecretKey": "your-super-secure-jwt-secret-key-min-32-chars",
  "ConnectionStrings:DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=ECommerceDb;...",
  "Redis:ConnectionString": "localhost:6379",
  "ApiKey:ApiKey": "your-secure-api-key",
  "RabbitMQ:Host": "localhost",
  "RabbitMQ:Username": "guest",
  "RabbitMQ:Password": "guest",
  "SmtpSettings:Host": "smtp.gmail.com",
  "SmtpSettings:Username": "your-email@gmail.com",
  "SmtpSettings:Password": "your-app-password"
}
```

**Nota:** Los dos puntos (`:`) en las claves representan jerarquía en el JSON.

---

## Cómo Funciona en el Código

User Secrets se integra automáticamente con el sistema de configuración de .NET:

```csharp
// En Program.cs o Startup.cs
public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}
```

`CreateDefaultBuilder` automáticamente:
1. Lee `appsettings.json`
2. Lee `appsettings.{Environment}.json`
3. **Lee User Secrets (solo en Development)**
4. Lee variables de entorno
5. Lee argumentos de línea de comandos

**Orden de prioridad:** Argumentos > Variables de Entorno > User Secrets > appsettings.json

---

## Verificar que Funciona

```bash
# 1. Configurar un secret
dotnet user-secrets set "TestSecret" "HelloWorld" --project src/Services/Order/Order.Api

# 2. Verificar que se guardó
dotnet user-secrets list --project src/Services/Order/Order.Api

# 3. En el código, acceder al secret
// Startup.cs
var testSecret = Configuration["TestSecret"];
Console.WriteLine($"Test Secret: {testSecret}"); // Output: HelloWorld
```

---

## Migrar de appsettings.json a User Secrets

Si ya tienes credenciales en `appsettings.json`:

```powershell
# 1. Copiar los valores sensibles a User Secrets
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "tu-connection-string" --project <path>

# 2. Eliminar los valores sensibles de appsettings.json
# Dejar solo valores por defecto o placeholders

# 3. Verificar que funciona
dotnet run --project <path>
```

---

## Producción y Otros Ambientes

⚠️ **IMPORTANTE:** User Secrets **SOLO funcionan en Development**.

Para otros ambientes:

### Staging/Production
Usa **Variables de Entorno**:

```bash
# Linux/Docker
export ConnectionStrings__DefaultConnection="Server=prod-server;..."
export SecretKey="production-secret-key"

# Windows
$env:ConnectionStrings__DefaultConnection = "Server=prod-server;..."
$env:SecretKey = "production-secret-key"

# Azure App Service
# Configurar en: Configuration > Application Settings

# AWS
# Usar AWS Secrets Manager o Parameter Store
```

### Docker Compose
```yaml
services:
  order-api:
    environment:
      - ConnectionStrings__DefaultConnection=Server=db;...
      - SecretKey=${SECRET_KEY}
    env_file:
      - .env.production  # No subir al repo
```

---

## Troubleshooting

### ❌ "No user secrets ID found"
```bash
# Solución: Inicializar User Secrets
dotnet user-secrets init --project <path>
```

### ❌ "Could not find the global property 'UserSecretsId'"
```bash
# Verificar que el .csproj tiene:
<PropertyGroup>
  <UserSecretsId>guid-generado-automaticamente</UserSecretsId>
</PropertyGroup>

# Si no existe, ejecutar:
dotnet user-secrets init --project <path>
```

### ❌ Los secrets no se aplican
```bash
# 1. Verificar que estás en modo Development
echo $env:ASPNETCORE_ENVIRONMENT  # Windows
echo $ASPNETCORE_ENVIRONMENT       # Linux

# 2. Debe ser "Development" para que User Secrets funcione

# 3. Verificar que los secrets existen
dotnet user-secrets list --project <path>
```

### ❌ Quiero compartir secrets con mi equipo
User Secrets son **por desarrollador**, no se comparten. Opciones:

1. **Documentar** los secrets necesarios (este archivo)
2. Cada dev configura sus propios secrets
3. Para valores compartidos no sensibles, usar `appsettings.Development.json` (gitignoreado)
4. Para equipos, considerar **Azure Key Vault** o **HashiCorp Vault**

---

## Mejores Prácticas

✅ **DO:**
- Usar User Secrets para desarrollo local
- Documentar qué secrets se necesitan
- Generar secrets únicos por desarrollador
- Rotar secrets regularmente
- Usar variables de entorno en producción

❌ **DON'T:**
- Subir `secrets.json` al repositorio (está fuera del proyecto)
- Compartir secrets por email/chat
- Usar los mismos secrets en dev y prod
- Hardcodear secrets en el código
- Commitear `appsettings.Development.json` con credenciales

---

## Recursos Adicionales

- [Documentación oficial de User Secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)
- [Safe storage of app secrets in development](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets?view=aspnetcore-9.0&tabs=windows)
- [Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)

---

## Resumen de Comandos

```bash
# Setup completo
.\scripts\setup-user-secrets.ps1 -Init -SetDefaults

# Ver todos los secrets
.\scripts\setup-user-secrets.ps1 -List

# Agregar/modificar un secret
dotnet user-secrets set "Key" "Value" --project <path>

# Ver secrets de un proyecto
dotnet user-secrets list --project <path>

# Eliminar un secret
dotnet user-secrets remove "Key" --project <path>

# Limpiar todos los secrets
dotnet user-secrets clear --project <path>
```

---

**¿Necesitas ayuda?** Revisa la sección de Troubleshooting o consulta la documentación oficial.

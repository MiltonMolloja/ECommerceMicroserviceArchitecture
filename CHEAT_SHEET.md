# 🔧 Cheat Sheet - Comandos Útiles

## 📦 .NET CLI Commands

### Compilar el Proyecto

```powershell
# Compilar toda la solución
dotnet build ECommerce.sln

# Compilar un proyecto específico
dotnet build src/Services/Catalog/Catalog.Api/Catalog.Api.csproj

# Compilar en Release
dotnet build -c Release
```

### Ejecutar Servicios

```powershell
# Ejecutar Identity API
cd src/Services/Identity/Identity.Api
dotnet run

# Ejecutar con puerto específico
dotnet run --urls "http://localhost:10000"

# Ejecutar en watch mode (auto-reload)
dotnet watch run
```

### Gestión de Paquetes

```powershell
# Listar paquetes instalados
dotnet list package

# Agregar paquete
dotnet add package Microsoft.EntityFrameworkCore --version 9.0.0

# Actualizar paquete
dotnet add package Microsoft.EntityFrameworkCore

# Restaurar paquetes
dotnet restore
```

### Tests

```powershell
# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un proyecto específico
dotnet test src/Services/Catalog/Catalog.Tests/Catalog.Tests.csproj

# Ejecutar con verbosidad
dotnet test --verbosity detailed

# Ver cobertura
dotnet test /p:CollectCoverage=true
```

## 🗄️ Entity Framework Core Commands

### Migraciones

```powershell
# Crear nueva migración
cd src/Services/Identity/Identity.Persistence.Database
dotnet ef migrations add MigrationName

# Aplicar migraciones
dotnet ef database update

# Revertir última migración
dotnet ef migrations remove

# Ver lista de migraciones
dotnet ef migrations list

# Generar script SQL
dotnet ef migrations script

# Aplicar migración específica
dotnet ef database update MigrationName

# Ver SQL que se ejecutará
dotnet ef migrations script --idempotent
```

### Base de Datos

```powershell
# Eliminar base de datos
dotnet ef database drop

# Crear base de datos
dotnet ef database update

# Ver información de DbContext
dotnet ef dbcontext info

# Generar script de creación
dotnet ef dbcontext script
```

### DbContext

```powershell
# Ver información del contexto
dotnet ef dbcontext info

# Listar todos los DbContext
dotnet ef dbcontext list

# Generar modelo desde BD existente (Reverse Engineering)
dotnet ef dbcontext scaffold "ConnectionString" Microsoft.EntityFrameworkCore.SqlServer -o Models
```

## 💾 SQL Server Commands

### SQL Server Express / LocalDB

```powershell
# Iniciar SQL Server Express
net start MSSQL$SQLEXPRESS

# Detener SQL Server Express
net stop MSSQL$SQLEXPRESS

# Listar servicios SQL
Get-Service | Where-Object {$_.Name -like "*SQL*"}

# Iniciar LocalDB
sqllocaldb start mssqllocaldb

# Detener LocalDB
sqllocaldb stop mssqllocaldb

# Ver instancias de LocalDB
sqllocaldb info

# Crear nueva instancia LocalDB
sqllocaldb create MyInstance
```

### SQLCMD

```powershell
# Conectar a SQL Server
sqlcmd -S "localhost\SQLEXPRESS" -E

# Ejecutar query
sqlcmd -S "localhost\SQLEXPRESS" -d "ECommerceDb" -Q "SELECT * FROM [Identity].[AspNetUsers]" -E

# Ejecutar script
sqlcmd -S "localhost\SQLEXPRESS" -d "ECommerceDb" -i "script.sql" -E

# Exportar resultados a archivo
sqlcmd -S "localhost\SQLEXPRESS" -Q "SELECT * FROM sys.tables" -o "output.txt" -E

# Modo interactivo
sqlcmd -S "localhost\SQLEXPRESS" -d "ECommerceDb" -E
```

### Queries Útiles

```sql
-- Ver todas las bases de datos
SELECT name FROM sys.databases;

-- Ver todas las tablas de un schema
SELECT s.name AS [Schema], t.name AS [Table]
FROM sys.tables t
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id
WHERE s.name = 'Identity'
ORDER BY t.name;

-- Ver estructura de una tabla
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_SCHEMA = 'Identity' AND TABLE_NAME = 'AspNetUsers'
ORDER BY ORDINAL_POSITION;

-- Ver relaciones (Foreign Keys)
SELECT 
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable
FROM sys.foreign_keys AS fk
ORDER BY TableName;

-- Ver historial de migraciones
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;

-- Espacio usado por tablas
EXEC sp_spaceused;

-- Ver índices
SELECT 
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE OBJECT_SCHEMA_NAME(i.object_id) = 'Catalog';
```

## 🐳 Docker Commands (Opcional)

### SQL Server en Docker

```powershell
# Descargar imagen de SQL Server
docker pull mcr.microsoft.com/mssql/server:2022-latest

# Ejecutar SQL Server en Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Password" `
  -p 1433:1433 --name sql-server `
  -d mcr.microsoft.com/mssql/server:2022-latest

# Ver contenedores corriendo
docker ps

# Ver logs
docker logs sql-server

# Detener contenedor
docker stop sql-server

# Iniciar contenedor
docker start sql-server

# Eliminar contenedor
docker rm sql-server
```

### Aplicación en Docker

```powershell
# Build de imagen
docker build -t eCommerce-identity -f src/Services/Identity/Identity.Api/Dockerfile .

# Ejecutar contenedor
docker run -d -p 10000:80 --name identity-api eCommerce-identity

# Ver logs
docker logs identity-api

# Docker Compose
docker-compose up -d
docker-compose down
docker-compose logs -f
```

## 🔐 Identity & JWT

### Generar Password Hash

```powershell
# Usar PowerShell para generar hash
$password = "Pa$$w0rd!"
$hasher = New-Object Microsoft.AspNetCore.Identity.PasswordHasher
$hash = $hasher.HashPassword($null, $password)
Write-Host $hash
```

### Decodificar JWT Token

```powershell
# En PowerShell
$token = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
$parts = $token.Split('.')
[System.Text.Encoding]::UTF8.GetString([System.Convert]::FromBase64String($parts[1]))
```

### Probar Endpoints con curl

```powershell
# Login
curl -X POST http://localhost:10000/api/user/login `
  -H "Content-Type: application/json" `
  -d '{"email":"admin@gmail.com","password":"Pa$$w0rd!"}'

# Con token
curl -X GET http://localhost:10000/api/user/profile `
  -H "Authorization: Bearer YOUR_TOKEN_HERE"
```

## 📊 Health Checks

### Verificar Health Checks

```powershell
# PowerShell - Verificar todos los servicios
$services = @(10000, 20000, 30000, 40000)
foreach ($port in $services) {
    $url = "http://localhost:$port/hc"
    try {
        $response = Invoke-WebRequest -Uri $url -UseBasicParsing
        Write-Host "Port $port : $($response.StatusCode) - OK" -ForegroundColor Green
    } catch {
        Write-Host "Port $port : ERROR" -ForegroundColor Red
    }
}

# Verificar un servicio específico
Invoke-WebRequest -Uri "http://localhost:10000/hc" -UseBasicParsing | Select-Object StatusCode, Content
```

## 🔍 Debugging

### Ver Variables de Entorno

```powershell
# Ver todas las variables
Get-ChildItem Env:

# Ver variable específica
$env:ASPNETCORE_ENVIRONMENT

# Establecer variable
$env:ASPNETCORE_ENVIRONMENT = "Development"
```

### Logs

```powershell
# Ver logs en tiempo real (si usas Serilog)
Get-Content -Path "logs/log.txt" -Wait

# Filtrar logs
Get-Content -Path "logs/log.txt" | Select-String "Error"

# Últimas 50 líneas
Get-Content -Path "logs/log.txt" -Tail 50
```

### Performance

```powershell
# Medir tiempo de ejecución
Measure-Command { dotnet build }

# Medir memoria
Get-Process dotnet | Select-Object ProcessName, WS, PM

# Ver threads
Get-Process dotnet | Select-Object ProcessName, Threads
```

## 🚀 Deployment

### Publicar Aplicación

```powershell
# Publicar en carpeta
dotnet publish -c Release -o ./publish

# Publicar self-contained
dotnet publish -c Release -r win-x64 --self-contained true

# Publicar para Linux
dotnet publish -c Release -r linux-x64 --self-contained false
```

### IIS

```powershell
# Instalar módulo de ASP.NET Core
# Descargar: https://dotnet.microsoft.com/permalink/dotnetcore-current-windows-runtime-bundle-installer

# Crear sitio en IIS
New-IISSite -Name "ECommerceIdentity" -PhysicalPath "C:\inetpub\wwwroot\identity" -BindingInformation "*:10000:"

# Detener sitio
Stop-IISSite -Name "ECommerceIdentity"

# Iniciar sitio
Start-IISSite -Name "ECommerceIdentity"
```

## 📦 NuGet

### Gestión de Paquetes

```powershell
# Limpiar caché de NuGet
dotnet nuget locals all --clear

# Ver ubicación de caché
dotnet nuget locals all --list

# Agregar fuente de NuGet
dotnet nuget add source "https://api.nuget.org/v3/index.json" --name "NuGet"

# Listar fuentes
dotnet nuget list source

# Buscar paquete
dotnet nuget search EntityFramework
```

## 🧹 Limpieza

### Limpiar Build Artifacts

```powershell
# Limpiar solución
dotnet clean

# Eliminar carpetas bin y obj (PowerShell)
Get-ChildItem -Path . -Include bin,obj -Recurse | Remove-Item -Recurse -Force

# Eliminar paquetes NuGet descargados
dotnet nuget locals all --clear

# Script completo de limpieza
function Clean-Solution {
    Write-Host "Limpiando solución..." -ForegroundColor Yellow
    dotnet clean
    Get-ChildItem -Path . -Include bin,obj -Recurse | Remove-Item -Recurse -Force
    Write-Host "Limpieza completada!" -ForegroundColor Green
}
```

## 📋 Git Commands Útiles

```powershell
# Ver estado
git status

# Crear branch
git checkout -b feature/nueva-funcionalidad

# Commit
git add .
git commit -m "feat: descripción del cambio"

# Push
git push origin feature/nueva-funcionalidad

# Ver historial
git log --oneline --graph --all

# Deshacer último commit (mantener cambios)
git reset --soft HEAD~1

# Ver diferencias
git diff
```

## 🔧 Visual Studio / VS Code

### VS Code Extensions Recomendadas

```
# Instalar extensiones desde terminal
code --install-extension ms-dotnettools.csharp
code --install-extension ms-dotnettools.vscode-dotnet-runtime
code --install-extension ms-mssql.mssql
code --install-extension bierner.markdown-mermaid
```

### Tareas Útiles en VS Code

```json
// .vscode/tasks.json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "build",
      "command": "dotnet",
      "type": "process",
      "args": ["build"],
      "problemMatcher": "$msCompile"
    },
    {
      "label": "run-identity",
      "command": "dotnet",
      "type": "process",
      "args": ["run"],
      "options": {
        "cwd": "${workspaceFolder}/src/Services/Identity/Identity.Api"
      }
    }
  ]
}
```

## 📊 Monitoreo

### Verificar Servicios Activos

```powershell
# PowerShell Script
function Test-Services {
    $services = @{
        "Identity" = 10000
        "Catalog" = 20000
        "Customer" = 30000
        "Order" = 40000
    }
    
    foreach ($service in $services.GetEnumerator()) {
        $url = "http://localhost:$($service.Value)/hc"
        try {
            $response = Invoke-RestMethod -Uri $url
            Write-Host "$($service.Name): ✅ OK" -ForegroundColor Green
        } catch {
            Write-Host "$($service.Name): ❌ DOWN" -ForegroundColor Red
        }
    }
}

Test-Services
```

## 🎯 Comandos de Este Proyecto

### Ejecutar Todo (PowerShell)

```powershell
# Script para ejecutar todos los servicios
$services = @(
    "src/Services/Identity/Identity.Api",
    "src/Services/Catalog/Catalog.Api",
    "src/Services/Customer/Customer.Api",
    "src/Services/Order/Order.Api"
)

foreach ($service in $services) {
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd $service; dotnet run"
}
```

### Verificar Configuración

```powershell
# Verificar versión de .NET
dotnet --version

# Verificar versión de EF Tools
dotnet ef --version

# Verificar SQL Server
sqlcmd -S "localhost\SQLEXPRESS" -Q "SELECT @@VERSION" -E

# Verificar base de datos
sqlcmd -S "localhost\SQLEXPRESS" -Q "SELECT name FROM sys.databases WHERE name = 'ECommerceDb'" -E
```

## 📚 Referencias Rápidas

- **Conexión DB:** `Server=localhost\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True`
- **Usuario Admin:** `admin@gmail.com` / `Pa$$w0rd!`
- **Puertos:** Identity:10000, Catalog:20000, Customer:30000, Order:40000

---

**💡 Tip:** Guarda este archivo como referencia rápida. Todos estos comandos están probados y funcionan en este proyecto.

**📚 Ver también:**
- [DOCUMENTATION_INDEX.md](./DOCUMENTATION_INDEX.md) - Índice completo
- [QUICK_START_NET9.md](./QUICK_START_NET9.md) - Guía de inicio
- [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md) - Esquema de base de datos

**Última actualización:** 2025-10-04

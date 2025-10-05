# KODOTI Commerce - Microservice Architecture .NET 9 🚀

Proyecto de ejemplo en el cual veremos como crear órdenes de compra usando una arquitectura orientada a microservicios y **.NET 9**.

![kodoti course](https://anexsoft.com/storage/app/media/common/kodoti-microservice-architecture.jpg "Curso de Microservicios con .NET Core")

> **⚡ ACTUALIZADO A .NET 9** - Este proyecto ha sido migrado exitosamente de .NET Core 3.1 a .NET 9.0. Ver [MIGRATION_TO_NET9.md](MIGRATION_TO_NET9.md) para detalles completos.

## ¿Cómo levantar el proyecto?
### 1. Cambiar las cadenas de conexión
Actualicen las cadenas de conexión de cada Microservicio por la de ustedes.

### 2. Actualizar los puertos de los proyectos web
* Clients.Authentication: localhost:60000
* Clients.WebClient: localhost:60001
* Api.Gateway.WebClient: localhost:50000
* Identity.Api: localhost:10000
* Catalog.Api: localhost:20000
* Customer.Api: localhost:30000
* Order.Api: localhost:40000

### 3. Ejecutar las migraciones
```
update-database -context ApplicationDbContext
```

### 4. Agregar un usuario por defecto a la base de datos
```
INSERT [Identity].[AspNetUsers] ([Id], [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash], [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled], [LockoutEnd], [LockoutEnabled], [AccessFailedCount], [FirstName], [LastName]) VALUES (N'cc7deafd-2977-4c1b-91ad-7b8d37a01ffe', N'admin@kodoti.com', N'ADMIN@KODOTI.COM', N'admin@kodoti.com', N'ADMIN@KODOTI.COM', 0, N'AQAAAAEAACcQAAAAEL5faIXPhAOdXYU+vAAKbF32yd2ONSGUdGJ6wo9jkhm8KKlLF/h5x0zjJbcPKt8WYg==', N'PS7QHYXIO4NUC65ZYEP4SBEYOXP4DTWA', N'e955992b-abf5-41d3-b504-ec6dc0632989', NULL, 0, 0, NULL, 1, 0, N'Eduardo', N'Rodríguez Patiño')
```

## 📋 Requisitos

- **.NET 9 SDK** - [Descargar aquí](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server** (LocalDB, Express, o versión completa)
- **Visual Studio 2022** (17.8+) o **VS Code**

## 📚 Documentación de Migración

- **[MIGRATION_SUMMARY.md](MIGRATION_SUMMARY.md)** - Resumen ejecutivo de la migración
- **[MIGRATION_TO_NET9.md](MIGRATION_TO_NET9.md)** - Documentación técnica detallada
- **[QUICK_START_NET9.md](QUICK_START_NET9.md)** - Guía rápida de inicio

## 🎯 ¿Quiéres aprender más sobre esto?
Pues tenemos un curso completo donde vemos la teoría y la llevamos a la práctica realizando este proyecto.
https://www.udemy.com/course/microservicios-con-net-core-3-hasta-su-publicacion-en-azure/?referralCode=396CB83F117C145C4D6A

> **Nota:** El curso original está en .NET Core 3.1. Este repositorio ha sido actualizado a .NET 9 con toda la documentación necesaria.

# 🗄️ Esquema de Base de Datos - ECommerceDb

## 📊 Diagrama Entidad-Relación

```mermaid
erDiagram
    %% Identity Schema
    AspNetUsers {
        nvarchar Id PK
        nvarchar UserName
        nvarchar NormalizedUserName
        nvarchar Email
        nvarchar NormalizedEmail
        bit EmailConfirmed
        nvarchar PasswordHash
        nvarchar SecurityStamp
        nvarchar ConcurrencyStamp
        nvarchar PhoneNumber
        bit PhoneNumberConfirmed
        bit TwoFactorEnabled
        datetimeoffset LockoutEnd
        bit LockoutEnabled
        int AccessFailedCount
        nvarchar FirstName
        nvarchar LastName
    }
    
    AspNetRoles {
        nvarchar Id PK
        nvarchar Name
        nvarchar NormalizedName
        nvarchar ConcurrencyStamp
    }
    
    AspNetUserRoles {
        nvarchar UserId PK,FK
        nvarchar RoleId PK,FK
    }
    
    AspNetUserClaims {
        int Id PK
        nvarchar UserId FK
        nvarchar ClaimType
        nvarchar ClaimValue
    }
    
    AspNetRoleClaims {
        int Id PK
        nvarchar RoleId FK
        nvarchar ClaimType
        nvarchar ClaimValue
    }
    
    AspNetUserLogins {
        nvarchar LoginProvider PK
        nvarchar ProviderKey PK
        nvarchar ProviderDisplayName
        nvarchar UserId FK
    }
    
    AspNetUserTokens {
        nvarchar UserId PK,FK
        nvarchar LoginProvider PK
        nvarchar Name PK
        nvarchar Value
    }
    
    %% Catalog Schema
    Products {
        int ProductId PK
        nvarchar Name
        nvarchar Description
        decimal Price
    }
    
    Stocks {
        int ProductInStockId PK
        int ProductId FK
        int Stock
    }
    
    %% Customer Schema
    Clients {
        int ClientId PK
        nvarchar Name
        nvarchar Email
        nvarchar Phone
    }
    
    %% Order Schema
    Orders {
        int OrderId PK
        int ClientId FK
        datetime CreatedAt
        nvarchar Status
        decimal Total
    }
    
    OrderDetail {
        int OrderDetailId PK
        int OrderId FK
        int ProductId FK
        int Quantity
        decimal UnitPrice
        decimal Total
    }
    
    %% Relationships - Identity Schema
    AspNetUsers ||--o{ AspNetUserRoles : "has"
    AspNetRoles ||--o{ AspNetUserRoles : "has"
    AspNetUsers ||--o{ AspNetUserClaims : "has"
    AspNetRoles ||--o{ AspNetRoleClaims : "has"
    AspNetUsers ||--o{ AspNetUserLogins : "has"
    AspNetUsers ||--o{ AspNetUserTokens : "has"
    
    %% Relationships - Business Logic
    Products ||--o| Stocks : "has stock"
    Clients ||--o{ Orders : "places"
    Orders ||--o{ OrderDetail : "contains"
    Products ||--o{ OrderDetail : "in"
```

## 🏗️ Arquitectura por Esquemas

### 🔐 Identity Schema
**Propósito:** Gestión de autenticación y autorización de usuarios

| Tabla | Descripción | Registros Iniciales |
|-------|-------------|---------------------|
| `AspNetUsers` | Usuarios del sistema | 1 (admin@gmail.com) |
| `AspNetRoles` | Roles de usuario | 0 |
| `AspNetUserRoles` | Relación usuarios-roles | 0 |
| `AspNetUserClaims` | Claims personalizados de usuarios | 0 |
| `AspNetRoleClaims` | Claims de roles | 0 |
| `AspNetUserLogins` | Logins externos (Google, Facebook, etc.) | 0 |
| `AspNetUserTokens` | Tokens de autenticación | 0 |

### 📦 Catalog Schema
**Propósito:** Gestión del catálogo de productos

| Tabla | Descripción | Campos Principales |
|-------|-------------|-------------------|
| `Products` | Productos disponibles | ProductId, Name, Description, Price |
| `Stocks` | Inventario de productos | ProductInStockId, ProductId, Stock |

**Relación:** Un producto tiene un registro de stock (1:1)

### 👥 Customer Schema
**Propósito:** Gestión de clientes

| Tabla | Descripción | Campos Principales |
|-------|-------------|-------------------|
| `Clients` | Información de clientes | ClientId, Name, Email, Phone |

### 🛒 Order Schema
**Propósito:** Gestión de pedidos y ventas

| Tabla | Descripción | Campos Principales |
|-------|-------------|-------------------|
| `Orders` | Pedidos realizados | OrderId, ClientId, CreatedAt, Status, Total |
| `OrderDetail` | Detalle de cada pedido | OrderDetailId, OrderId, ProductId, Quantity, UnitPrice, Total |

**Relaciones:**
- Un cliente puede tener múltiples pedidos (1:N)
- Un pedido contiene múltiples detalles (1:N)
- Un producto puede estar en múltiples detalles (1:N)

## 📈 Diagrama de Arquitectura Microservicios

```mermaid
graph TB
    subgraph "Identity Microservice"
        IdentityAPI[Identity.Api :10000]
        IdentityDB[(Identity Schema)]
        IdentityAPI --> IdentityDB
    end
    
    subgraph "Catalog Microservice"
        CatalogAPI[Catalog.Api :20000]
        CatalogDB[(Catalog Schema)]
        CatalogAPI --> CatalogDB
    end
    
    subgraph "Customer Microservice"
        CustomerAPI[Customer.Api :30000]
        CustomerDB[(Customer Schema)]
        CustomerAPI --> CustomerDB
    end
    
    subgraph "Order Microservice"
        OrderAPI[Order.Api :40000]
        OrderDB[(Order Schema)]
        OrderAPI --> OrderDB
    end
    
    Gateway[API Gateway :45000]
    WebClient[Web Client]
    
    WebClient --> Gateway
    Gateway --> IdentityAPI
    Gateway --> CatalogAPI
    Gateway --> CustomerAPI
    Gateway --> OrderAPI
    
    OrderAPI -.->|Consulta productos| CatalogAPI
    OrderAPI -.->|Consulta clientes| CustomerAPI
    
    style IdentityDB fill:#ff9999
    style CatalogDB fill:#99ccff
    style CustomerDB fill:#99ff99
    style OrderDB fill:#ffcc99
```

## 🔗 Relaciones Entre Microservicios

### Order → Catalog
- **Propósito:** Validar productos y obtener precios
- **Método:** HTTP API calls
- **Endpoint:** GET /api/products/{id}

### Order → Customer
- **Propósito:** Validar información del cliente
- **Método:** HTTP API calls
- **Endpoint:** GET /api/clients/{id}

### Identity → Todos
- **Propósito:** Autenticación y autorización
- **Método:** JWT Token validation
- **Flujo:** Todos los servicios validan tokens generados por Identity

## 📊 Estadísticas de la Base de Datos

### Resumen por Schema

| Schema | Tablas | Tipo |
|--------|--------|------|
| **Identity** | 7 | Sistema (ASP.NET Core Identity) |
| **Catalog** | 2 | Negocio |
| **Customer** | 1 | Negocio |
| **Order** | 2 | Negocio |
| **dbo** | 1 | Sistema (EF Migrations) |
| **TOTAL** | **13** | - |

### Índices y Claves

```mermaid
graph LR
    A[Primary Keys] -->|13| B[Todas las tablas]
    C[Foreign Keys] -->|8| D[Relaciones]
    E[Unique Constraints] -->|3| F[Identity tables]
```

## 🔐 Datos Iniciales

### Usuario Administrador
```sql
Email: admin@gmail.com
Password: Pa$$w0rd!
FirstName: Admin
LastName: Administrator
EmailConfirmed: true
```

## 🛠️ Comandos Útiles

### Ver todas las tablas por schema
```sql
SELECT 
    s.name AS [Schema], 
    t.name AS [Table],
    (SELECT COUNT(*) FROM sys.columns WHERE object_id = t.object_id) AS [Columns]
FROM sys.tables t 
INNER JOIN sys.schemas s ON t.schema_id = s.schema_id 
ORDER BY s.name, t.name;
```

### Ver relaciones (Foreign Keys)
```sql
SELECT 
    fk.name AS ForeignKey,
    OBJECT_NAME(fk.parent_object_id) AS TableName,
    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) AS ColumnName,
    OBJECT_NAME(fk.referenced_object_id) AS ReferencedTable,
    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) AS ReferencedColumn
FROM sys.foreign_keys AS fk
INNER JOIN sys.foreign_key_columns AS fkc 
    ON fk.object_id = fkc.constraint_object_id
ORDER BY TableName;
```

### Ver índices por tabla
```sql
SELECT 
    OBJECT_SCHEMA_NAME(i.object_id) AS SchemaName,
    OBJECT_NAME(i.object_id) AS TableName,
    i.name AS IndexName,
    i.type_desc AS IndexType
FROM sys.indexes i
WHERE i.object_id IN (SELECT object_id FROM sys.tables)
    AND i.name IS NOT NULL
ORDER BY SchemaName, TableName, IndexName;
```

## 📚 Documentación Relacionada

- [DATABASE_CONNECTION_VERIFIED.md](./DATABASE_CONNECTION_VERIFIED.md) - Verificación de conexión
- [DATABASE_MIGRATION_COMPLETE.md](./DATABASE_MIGRATION_COMPLETE.md) - Proceso de migración
- [MIGRATION_TO_NET9.md](./MIGRATION_TO_NET9.md) - Migración a .NET 9
- [README.md](./README.md) - Documentación principal

## 🔄 Actualizaciones del Schema

**Última actualización:** 2025-10-04  
**Versión:** 1.0  
**Estado:** ✅ Producción

---

**Conexión:**
```
Server=localhost\SQLEXPRESS;Database=ECommerceDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True
```

# 🎨 Visualización Rápida de la Base de Datos

## 📊 Vista Simplificada de Esquemas

```mermaid
graph TB
    subgraph DB[ECommerceDb]
        subgraph Identity["🔐 Identity Schema (7 tablas)"]
            Users[👤 AspNetUsers<br/>Usuarios del sistema]
            Roles[🎭 AspNetRoles<br/>Roles]
            UserRoles[🔗 AspNetUserRoles]
            UserClaims[📋 AspNetUserClaims]
            RoleClaims[📋 AspNetRoleClaims]
            UserLogins[🔑 AspNetUserLogins]
            UserTokens[🎫 AspNetUserTokens]
        end
        
        subgraph Catalog["📦 Catalog Schema (2 tablas)"]
            Products[🛍️ Products<br/>Productos]
            Stocks[📊 Stocks<br/>Inventario]
        end
        
        subgraph Customer["👥 Customer Schema (1 tabla)"]
            Clients[👤 Clients<br/>Clientes]
        end
        
        subgraph Order["🛒 Order Schema (2 tablas)"]
            Orders[📝 Orders<br/>Pedidos]
            OrderDetails[📋 OrderDetail<br/>Detalle de pedidos]
        end
    end
    
    Users -.-> UserRoles
    Roles -.-> UserRoles
    Products --> Stocks
    Clients --> Orders
    Orders --> OrderDetails
    Products -.-> OrderDetails
    
    style Identity fill:#ffe6e6,stroke:#ff0000,stroke-width:2px
    style Catalog fill:#e6f3ff,stroke:#0066cc,stroke-width:2px
    style Customer fill:#e6ffe6,stroke:#00cc00,stroke-width:2px
    style Order fill:#fff0e6,stroke:#ff9900,stroke-width:2px
```

## 🔄 Flujo de Datos Principal

```mermaid
sequenceDiagram
    participant C as 👤 Cliente
    participant I as 🔐 Identity
    participant Cat as 📦 Catalog
    participant Cust as 👥 Customer
    participant O as 🛒 Order
    
    C->>I: 1. Login
    I-->>C: JWT Token
    
    C->>Cat: 2. Ver Productos
    Cat-->>C: Lista de Productos
    
    C->>Cust: 3. Registrar Info
    Cust-->>C: Cliente Creado
    
    C->>O: 4. Crear Pedido
    O->>Cat: Validar Productos
    Cat-->>O: Productos OK
    O->>Cust: Validar Cliente
    Cust-->>O: Cliente OK
    O-->>C: Pedido Creado
```

## 📋 Tabla Resumen por Schema

| Schema | Icono | Tablas | Propósito | Puerto API |
|--------|-------|--------|-----------|------------|
| **Identity** | 🔐 | 7 | Autenticación y Autorización | :10000 |
| **Catalog** | 📦 | 2 | Gestión de Productos | :20000 |
| **Customer** | 👥 | 1 | Gestión de Clientes | :30000 |
| **Order** | 🛒 | 2 | Gestión de Pedidos | :40000 |

## 🗂️ Estructura de Tablas Detallada

### 🔐 Identity Schema

```
AspNetUsers (Tabla Principal)
├── Id (PK)
├── UserName
├── Email
├── PasswordHash
├── FirstName
├── LastName
└── ... (11 campos más)
    │
    ├──→ AspNetUserRoles (Relación con Roles)
    ├──→ AspNetUserClaims (Claims del Usuario)
    ├──→ AspNetUserLogins (Logins Externos)
    └──→ AspNetUserTokens (Tokens)

AspNetRoles
├── Id (PK)
├── Name
└──→ AspNetRoleClaims (Claims del Rol)
```

### 📦 Catalog Schema

```
Products
├── ProductId (PK)
├── Name
├── Description
├── Price
└──→ Stocks (1:1)
    └── Stock (cantidad)
```

### 👥 Customer Schema

```
Clients
├── ClientId (PK)
├── Name
├── Email
└── Phone
```

### 🛒 Order Schema

```
Orders
├── OrderId (PK)
├── ClientId (FK → Clients)
├── CreatedAt
├── Status
├── Total
└──→ OrderDetail (1:N)
    ├── OrderDetailId (PK)
    ├── OrderId (FK)
    ├── ProductId (FK → Products)
    ├── Quantity
    ├── UnitPrice
    └── Total
```

## 🎯 Cardinalidades

```mermaid
graph LR
    A[Client] -->|1:N| B[Order]
    B -->|1:N| C[OrderDetail]
    D[Product] -->|1:1| E[Stock]
    D -->|1:N| C
    
    F[User] -->|N:M| G[Role]
    F -->|1:N| H[UserClaim]
    G -->|1:N| I[RoleClaim]
```

**Leyenda:**
- `1:1` = Uno a Uno
- `1:N` = Uno a Muchos
- `N:M` = Muchos a Muchos

## 📊 Métricas de la Base de Datos

```mermaid
pie title Distribución de Tablas por Schema
    "Identity" : 7
    "Order" : 2
    "Catalog" : 2
    "Customer" : 1
    "Sistema" : 1
```

## 🔍 Queries de Ejemplo

### Obtener pedidos de un cliente con detalles
```sql
SELECT 
    c.Name AS Cliente,
    o.OrderId,
    o.CreatedAt,
    o.Status,
    od.Quantity,
    p.Name AS Producto,
    od.UnitPrice,
    od.Total
FROM [Customer].Clients c
INNER JOIN [Order].Orders o ON c.ClientId = o.ClientId
INNER JOIN [Order].OrderDetail od ON o.OrderId = od.OrderId
INNER JOIN [Catalog].Products p ON od.ProductId = p.ProductId
WHERE c.ClientId = 1;
```

### Productos con bajo stock
```sql
SELECT 
    p.ProductId,
    p.Name,
    p.Price,
    s.Stock
FROM [Catalog].Products p
INNER JOIN [Catalog].Stocks s ON p.ProductId = s.ProductId
WHERE s.Stock < 10
ORDER BY s.Stock ASC;
```

### Usuarios con roles
```sql
SELECT 
    u.UserName,
    u.Email,
    r.Name AS Role
FROM [Identity].AspNetUsers u
INNER JOIN [Identity].AspNetUserRoles ur ON u.Id = ur.UserId
INNER JOIN [Identity].AspNetRoles r ON ur.RoleId = r.Id;
```

## 🎨 Convenciones de Nombres

### Schemas
- ✅ PascalCase: `Identity`, `Catalog`, `Customer`, `Order`
- ✅ Singular para representar el dominio

### Tablas
- ✅ PascalCase: `Products`, `Orders`, `Clients`
- ✅ Plural cuando representa colecciones
- ✅ Singular para tablas de relación: `OrderDetail`

### Columnas
- ✅ PascalCase: `ProductId`, `FirstName`, `CreatedAt`
- ✅ Sufijo `Id` para claves primarias
- ✅ Nombres descriptivos

## 🔗 Enlaces Rápidos

- [📊 Esquema Completo](./DATABASE_SCHEMA.md)
- [✅ Verificación de Conexión](./DATABASE_CONNECTION_VERIFIED.md)
- [🔄 Historial de Migraciones](./DATABASE_MIGRATION_COMPLETE.md)

---

**Última actualización:** 2025-10-04  
**Base de datos:** ECommerceDb  
**Servidor:** localhost\SQLEXPRESS

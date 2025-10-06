# 🎯 Diagramas por Microservicio

## 📊 Vista General

```mermaid
graph TB
    subgraph "🔐 Identity Microservice - Port 10000"
        IA[Identity.Api]
        ID[Identity.Domain]
        IP[Identity.Persistence.Database]
        IQ[Identity.Service.Queries]
        IE[Identity.Service.EventHandlers]
        
        IA --> ID
        IA --> IP
        IA --> IQ
        IA --> IE
        IP --> IDB[(Identity Schema)]
    end
    
    subgraph "📦 Catalog Microservice - Port 20000"
        CA[Catalog.Api]
        CD[Catalog.Domain]
        CP[Catalog.Persistence.Database]
        CQ[Catalog.Service.Queries]
        CE[Catalog.Service.EventHandlers]
        
        CA --> CD
        CA --> CP
        CA --> CQ
        CA --> CE
        CP --> CDB[(Catalog Schema)]
    end
    
    subgraph "👥 Customer Microservice - Port 30000"
        CUA[Customer.Api]
        CUD[Customer.Domain]
        CUP[Customer.Persistence.Database]
        CUQ[Customer.Service.Queries]
        CUE[Customer.Service.EventHandlers]
        
        CUA --> CUD
        CUA --> CUP
        CUA --> CUQ
        CUA --> CUE
        CUP --> CUDB[(Customer Schema)]
    end
    
    subgraph "🛒 Order Microservice - Port 40000"
        OA[Order.Api]
        OD[Order.Domain]
        OP[Order.Persistence.Database]
        OQ[Order.Service.Queries]
        OE[Order.Service.EventHandlers]
        OPR[Order.Service.Proxies]
        
        OA --> OD
        OA --> OP
        OA --> OQ
        OA --> OE
        OA --> OPR
        OP --> ODB[(Order Schema)]
    end
    
    OPR -.->|HTTP| CA
    OPR -.->|HTTP| CUA
    
    style IDB fill:#ffebee
    style CDB fill:#e8f5e9
    style CUDB fill:#f3e5f5
    style ODB fill:#fff9c4
```

## 🔐 Identity Microservice - Detalle

### Estructura del Proyecto

```
Identity/
├── Identity.Api (Puerto 10000)
│   ├── Controllers/
│   │   └── UserController.cs
│   ├── Startup.cs
│   └── Program.cs
├── Identity.Domain
│   ├── ApplicationUser.cs
│   └── ApplicationRole.cs
├── Identity.Persistence.Database
│   ├── ApplicationDbContext.cs
│   ├── ApplicationDbContextFactory.cs
│   └── Configuration/
├── Identity.Service.Queries
│   └── UserQueryService.cs
└── Identity.Service.EventHandlers
    └── UserEventHandler.cs
```

### Flujo de Autenticación

```mermaid
sequenceDiagram
    participant C as Client
    participant API as Identity.Api
    participant H as EventHandler
    participant DB as Identity Schema
    
    C->>API: POST /api/user/login
    API->>H: LoginCommand
    H->>DB: ValidateUser
    DB-->>H: User + Roles
    H->>H: GenerateJWT
    H-->>API: JWT Token
    API-->>C: Token + UserInfo
```

### Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| POST | `/api/user/login` | Iniciar sesión |
| POST | `/api/user/register` | Registrar usuario |
| GET | `/api/user/profile` | Obtener perfil |
| PUT | `/api/user/profile` | Actualizar perfil |

## 📦 Catalog Microservice - Detalle

### Estructura del Proyecto

```
Catalog/
├── Catalog.Api (Puerto 20000)
│   ├── Controllers/
│   │   ├── ProductController.cs
│   │   └── StockController.cs
│   └── Startup.cs
├── Catalog.Domain
│   ├── Product.cs
│   └── ProductInStock.cs
├── Catalog.Persistence.Database
│   ├── ApplicationDbContext.cs
│   └── Configuration/
├── Catalog.Service.Queries
│   ├── ProductQueryService.cs
│   └── DTOs/
└── Catalog.Service.EventHandlers
    ├── ProductCreateEventHandler.cs
    └── ProductUpdateStockEventHandler.cs
```

### Flujo de Gestión de Productos

```mermaid
sequenceDiagram
    participant C as Client
    participant API as Catalog.Api
    participant CMD as Command Handler
    participant Q as Query Service
    participant DB as Catalog Schema
    
    Note over C,DB: Crear Producto
    C->>API: POST /api/product
    API->>CMD: CreateProductCommand
    CMD->>DB: Insert Product
    CMD->>DB: Insert Stock (0)
    DB-->>CMD: Product Created
    CMD-->>API: Success
    API-->>C: ProductId
    
    Note over C,DB: Actualizar Stock
    C->>API: PUT /api/stock/{id}
    API->>CMD: UpdateStockCommand
    CMD->>DB: Update Stock
    DB-->>CMD: Updated
    CMD-->>API: Success
    API-->>C: 200 OK
    
    Note over C,DB: Consultar Productos
    C->>API: GET /api/product
    API->>Q: GetProducts()
    Q->>DB: SELECT Products + Stock
    DB-->>Q: Product List
    Q-->>API: DTOs
    API-->>C: Product List JSON
```

### Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/product` | Listar productos |
| GET | `/api/product/{id}` | Obtener producto |
| POST | `/api/product` | Crear producto |
| PUT | `/api/product/{id}` | Actualizar producto |
| DELETE | `/api/product/{id}` | Eliminar producto |
| PUT | `/api/stock/{productId}` | Actualizar stock |

### Modelo de Dominio

```mermaid
classDiagram
    class Product {
        +int ProductId
        +string Name
        +string Description
        +decimal Price
        +ProductInStock Stock
    }
    
    class ProductInStock {
        +int ProductInStockId
        +int ProductId
        +int Stock
        +Product Product
    }
    
    Product "1" --> "1" ProductInStock : has
```

## 👥 Customer Microservice - Detalle

### Estructura del Proyecto

```
Customer/
├── Customer.Api (Puerto 30000)
│   ├── Controllers/
│   │   └── ClientController.cs
│   └── Startup.cs
├── Customer.Domain
│   └── Client.cs
├── Customer.Persistence.Database
│   ├── ApplicationDbContext.cs
│   └── Configuration/
├── Customer.Service.Queries
│   └── ClientQueryService.cs
└── Customer.Service.EventHandlers
    └── ClientEventHandler.cs
```

### Flujo de Gestión de Clientes

```mermaid
sequenceDiagram
    participant C as Client App
    participant API as Customer.Api
    participant CMD as Command Handler
    participant Q as Query Service
    participant DB as Customer Schema
    
    Note over C,DB: Crear Cliente
    C->>API: POST /api/client
    API->>CMD: CreateClientCommand
    CMD->>DB: Insert Client
    DB-->>CMD: Client Created
    CMD-->>API: Success
    API-->>C: ClientId
    
    Note over C,DB: Consultar Cliente
    C->>API: GET /api/client/{id}
    API->>Q: GetClientById(id)
    Q->>DB: SELECT Client
    DB-->>Q: Client Data
    Q-->>API: ClientDTO
    API-->>C: Client JSON
```

### Modelo de Dominio

```mermaid
classDiagram
    class Client {
        +int ClientId
        +string Name
        +string Email
        +string Phone
    }
```

### Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/client` | Listar clientes |
| GET | `/api/client/{id}` | Obtener cliente |
| POST | `/api/client` | Crear cliente |
| PUT | `/api/client/{id}` | Actualizar cliente |

## 🛒 Order Microservice - Detalle

### Estructura del Proyecto

```
Order/
├── Order.Api (Puerto 40000)
│   ├── Controllers/
│   │   └── OrderController.cs
│   └── Startup.cs
├── Order.Domain
│   ├── Order.cs
│   └── OrderDetail.cs
├── Order.Persistence.Database
│   ├── ApplicationDbContext.cs
│   └── Configuration/
├── Order.Service.Queries
│   └── OrderQueryService.cs
├── Order.Service.EventHandlers
│   └── OrderCreateEventHandler.cs
└── Order.Service.Proxies
    ├── CatalogProxy.cs
    └── CustomerProxy.cs
```

### Flujo de Creación de Pedido (Complejo)

```mermaid
sequenceDiagram
    participant C as Client App
    participant O as Order.Api
    participant OP as Order Proxies
    participant Cat as Catalog.Api
    participant Cust as Customer.Api
    participant CMD as Command Handler
    participant DB as Order Schema
    
    Note over C,DB: Crear Pedido
    C->>O: POST /api/order
    O->>OP: ValidateClient(clientId)
    OP->>Cust: GET /api/client/{id}
    Cust-->>OP: Client Data
    OP-->>O: Client Valid
    
    loop For each product
        O->>OP: ValidateProduct(productId)
        OP->>Cat: GET /api/product/{id}
        Cat-->>OP: Product + Price
        OP-->>O: Product Valid
        
        O->>OP: CheckStock(productId, qty)
        OP->>Cat: GET /api/stock/{id}
        Cat-->>OP: Stock Available
        OP-->>O: Stock OK
    end
    
    O->>CMD: CreateOrderCommand
    CMD->>DB: Insert Order
    CMD->>DB: Insert OrderDetails
    DB-->>CMD: Order Created
    
    CMD->>OP: UpdateStock(productId, -qty)
    OP->>Cat: PUT /api/stock/{id}
    Cat-->>OP: Stock Updated
    
    CMD-->>O: Success
    O-->>C: OrderId
```

### Modelo de Dominio

```mermaid
classDiagram
    class Order {
        +int OrderId
        +int ClientId
        +DateTime CreatedAt
        +string Status
        +decimal Total
        +List~OrderDetail~ Details
    }
    
    class OrderDetail {
        +int OrderDetailId
        +int OrderId
        +int ProductId
        +int Quantity
        +decimal UnitPrice
        +decimal Total
        +Order Order
    }
    
    Order "1" --> "*" OrderDetail : contains
```

### Endpoints Principales

| Método | Endpoint | Descripción |
|--------|----------|-------------|
| GET | `/api/order` | Listar pedidos |
| GET | `/api/order/{id}` | Obtener pedido |
| POST | `/api/order` | Crear pedido |
| PUT | `/api/order/{id}/status` | Cambiar estado |

### Comunicación Entre Servicios

```mermaid
graph LR
    O[Order Service] -->|HTTP GET| C[Catalog Service]
    O -->|HTTP GET| CU[Customer Service]
    O -->|HTTP PUT| C
    
    style O fill:#fff9c4
    style C fill:#e8f5e9
    style CU fill:#f3e5f5
```

## 🚪 API Gateway

### Estructura

```mermaid
graph TB
    Client[Web Client :60001]
    Auth[Authentication :60000]
    
    Gateway[API Gateway :45000]
    
    Gateway --> Identity[Identity.Api :10000]
    Gateway --> Catalog[Catalog.Api :20000]
    Gateway --> Customer[Customer.Api :30000]
    Gateway --> Order[Order.Api :40000]
    
    Client --> Gateway
    Auth --> Gateway
    
    style Gateway fill:#fff3e0,stroke:#ff9800,stroke-width:3px
```

### Proxies del Gateway

```
Api.Gateway.Proxies/
├── CatalogProxy.cs
├── CustomerProxy.cs
├── OrderProxy.cs
└── Config/
    └── ApiUrls.cs
```

### Configuración de Rutas

```json
{
  "ApiUrls": {
    "Identity": "http://localhost:10000",
    "Catalog": "http://localhost:20000",
    "Customer": "http://localhost:30000",
    "Order": "http://localhost:40000"
  }
}
```

## 🔄 Patrón CQRS Implementado

```mermaid
graph TB
    subgraph "Command Side (Write)"
        API1[API Controller]
        CMD[Command Handler<br/>MediatR]
        DB1[(Database)]
        
        API1 -->|Command| CMD
        CMD -->|Write| DB1
    end
    
    subgraph "Query Side (Read)"
        API2[API Controller]
        QS[Query Service]
        DB2[(Database)]
        
        API2 -->|Query| QS
        QS -->|Read| DB2
    end
    
    style CMD fill:#ffcdd2
    style QS fill:#c8e6c9
```

### Ejemplo en Catalog

**Command (Escritura):**
```csharp
// CreateProductCommand
public class CreateProductCommand : IRequest<bool>
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
}

// Handler
public class CreateProductHandler : IRequestHandler<CreateProductCommand, bool>
{
    // Escribe en la base de datos
}
```

**Query (Lectura):**
```csharp
// ProductQueryService
public class ProductQueryService
{
    public async Task<List<ProductDto>> GetProducts()
    {
        // Lee de la base de datos
    }
}
```

## 📊 Stack Tecnológico por Capa

```mermaid
graph TB
    subgraph "Presentation Layer"
        MVC[ASP.NET Core MVC]
        API[ASP.NET Core Web API]
    end
    
    subgraph "Application Layer"
        MED[MediatR - CQRS]
        AUTO[AutoMapper]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        VO[Value Objects]
    end
    
    subgraph "Infrastructure Layer"
        EF[Entity Framework Core 9]
        ID[ASP.NET Core Identity]
        JWT[JWT Authentication]
    end
    
    subgraph "Database Layer"
        SQL[(SQL Server)]
    end
    
    API --> MED
    MED --> ENT
    ENT --> EF
    EF --> SQL
    
    API --> ID
    API --> JWT
```

## 🏗️ Clean Architecture

```mermaid
graph TB
    subgraph "Outer Layer"
        UI[UI / API]
    end
    
    subgraph "Application Layer"
        CQRS[Commands & Queries]
        HAND[Handlers]
    end
    
    subgraph "Domain Layer"
        ENT[Entities]
        RULES[Business Rules]
    end
    
    subgraph "Infrastructure"
        DB[Database]
        EXT[External Services]
    end
    
    UI --> CQRS
    CQRS --> HAND
    HAND --> ENT
    HAND --> DB
    HAND --> EXT
    
    ENT -.->|Independiente| RULES
    
    style ENT fill:#4caf50
    style RULES fill:#4caf50
```

**Principios aplicados:**
- ✅ Domain no depende de infraestructura
- ✅ Flujo de dependencias hacia adentro
- ✅ Business logic aislada
- ✅ Testeable en todos los niveles

## 🎯 Health Checks

```mermaid
graph LR
    HC[Health Check UI]
    
    HC -->|Check| I[Identity :10000/hc]
    HC -->|Check| C[Catalog :20000/hc]
    HC -->|Check| CU[Customer :30000/hc]
    HC -->|Check| O[Order :40000/hc]
    
    I -.->|Status| DB[(Database)]
    C -.->|Status| DB
    CU -.->|Status| DB
    O -.->|Status| DB
    
    style HC fill:#e3f2fd
    style I fill:#c8e6c9
    style C fill:#c8e6c9
    style CU fill:#c8e6c9
    style O fill:#c8e6c9
```

**Endpoints de Health Check:**
- `http://localhost:10000/hc` - Identity
- `http://localhost:20000/hc` - Catalog
- `http://localhost:30000/hc` - Customer
- `http://localhost:40000/hc` - Order

## 📝 Resumen de Puertos

| Servicio | Puerto | Tipo | URL |
|----------|--------|------|-----|
| Identity API | 10000 | Microservicio | http://localhost:10000 |
| Catalog API | 20000 | Microservicio | http://localhost:20000 |
| Customer API | 30000 | Microservicio | http://localhost:30000 |
| Order API | 40000 | Microservicio | http://localhost:40000 |
| API Gateway | 45000 | Gateway | http://localhost:45000 |
| Authentication | 60000 | Cliente | http://localhost:60000 |
| Web Client | 60001 | Cliente | http://localhost:60001 |

---

**📚 Documentación Relacionada:**
- [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md) - Esquema completo de base de datos
- [DATABASE_DIAGRAM.md](./DATABASE_DIAGRAM.md) - Diagramas simplificados
- [DOCUMENTATION_INDEX.md](./DOCUMENTATION_INDEX.md) - Índice de toda la documentación

**Última actualización:** 2025-10-04

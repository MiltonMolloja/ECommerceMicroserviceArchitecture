# 🎨 Mapas Visuales - Acceso Rápido

## 🗄️ Esquema de Base de Datos

### Vista Completa con Relaciones

**Ver en:** [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md)

```mermaid
erDiagram
    %% Business Logic
    Clients ||--o{ Orders : "places"
    Orders ||--o{ OrderDetail : "contains"
    Products ||--o{ OrderDetail : "in"
    Products ||--o| Stocks : "has stock"
    
    %% Identity (simplified)
    AspNetUsers ||--o{ AspNetUserRoles : "has"
    AspNetRoles ||--o{ AspNetUserRoles : "has"
```

### Vista Simplificada por Schemas

**Ver en:** [DATABASE_DIAGRAM.md](./DATABASE_DIAGRAM.md)

```mermaid
graph TB
    subgraph "KodotiCommerceDb"
        Identity["🔐 Identity<br/>7 tablas<br/>Autenticación"]
        Catalog["📦 Catalog<br/>2 tablas<br/>Productos"]
        Customer["👥 Customer<br/>1 tabla<br/>Clientes"]
        Order["🛒 Order<br/>2 tablas<br/>Pedidos"]
    end
    
    Customer -.->|ClientId| Order
    Catalog -.->|ProductId| Order
    
    style Identity fill:#ffe6e6
    style Catalog fill:#e6f3ff
    style Customer fill:#e6ffe6
    style Order fill:#fff0e6
```

## 🏗️ Arquitectura de Microservicios

**Ver en:** [MICROSERVICES_DIAGRAMS.md](./MICROSERVICES_DIAGRAMS.md)

```mermaid
graph TB
    subgraph "Frontend"
        WC[Web Client<br/>:60001]
        AUTH[Authentication<br/>:60000]
    end
    
    subgraph "Gateway Layer"
        GW[API Gateway<br/>:50000]
    end
    
    subgraph "Microservices"
        I[🔐 Identity<br/>:10000]
        C[📦 Catalog<br/>:20000]
        CU[👥 Customer<br/>:30000]
        O[🛒 Order<br/>:40000]
    end
    
    subgraph "Database"
        DB[(KodotiCommerceDb<br/>4 Schemas)]
    end
    
    WC --> GW
    AUTH --> GW
    GW --> I
    GW --> C
    GW --> CU
    GW --> O
    
    I --> DB
    C --> DB
    CU --> DB
    O --> DB
    
    O -.->|HTTP| C
    O -.->|HTTP| CU
    
    style GW fill:#fff3e0,stroke:#ff9800,stroke-width:3px
    style DB fill:#f3e5f5,stroke:#9c27b0,stroke-width:3px
```

## 📊 Tablas por Schema

| Schema | Tablas | Descripción | Microservicio |
|--------|--------|-------------|---------------|
| 🔐 **Identity** | 7 | AspNetUsers, AspNetRoles, AspNetUserRoles, AspNetUserClaims, AspNetRoleClaims, AspNetUserLogins, AspNetUserTokens | Identity.Api |
| 📦 **Catalog** | 2 | Products, Stocks | Catalog.Api |
| 👥 **Customer** | 1 | Clients | Customer.Api |
| 🛒 **Order** | 2 | Orders, OrderDetail | Order.Api |

## 🔄 Flujo de Creación de Pedido

```mermaid
sequenceDiagram
    participant Cliente
    participant Gateway
    participant Identity
    participant Catalog
    participant Customer
    participant Order
    participant DB
    
    Cliente->>Gateway: POST /order
    Gateway->>Identity: Validar Token
    Identity-->>Gateway: Token válido
    
    Gateway->>Order: Crear pedido
    Order->>Customer: Validar cliente
    Customer-->>Order: Cliente OK
    
    Order->>Catalog: Validar productos
    Catalog-->>Order: Productos OK
    
    Order->>Catalog: Verificar stock
    Catalog-->>Order: Stock disponible
    
    Order->>DB: Guardar pedido
    Order->>Catalog: Descontar stock
    
    Order-->>Gateway: Pedido creado
    Gateway-->>Cliente: OrderId
```

## 🎯 Endpoints Principales por Servicio

### 🔐 Identity Service (Port 10000)

| Método | Endpoint | Acción |
|--------|----------|--------|
| POST | `/api/user/login` | 🔑 Login |
| POST | `/api/user/register` | 📝 Registro |
| GET | `/api/user/profile` | 👤 Perfil |

### 📦 Catalog Service (Port 20000)

| Método | Endpoint | Acción |
|--------|----------|--------|
| GET | `/api/product` | 📋 Listar |
| GET | `/api/product/{id}` | 🔍 Detalle |
| POST | `/api/product` | ➕ Crear |
| PUT | `/api/stock/{id}` | 📊 Actualizar Stock |

### 👥 Customer Service (Port 30000)

| Método | Endpoint | Acción |
|--------|----------|--------|
| GET | `/api/client` | 📋 Listar |
| GET | `/api/client/{id}` | 🔍 Detalle |
| POST | `/api/client` | ➕ Crear |

### 🛒 Order Service (Port 40000)

| Método | Endpoint | Acción |
|--------|----------|--------|
| GET | `/api/order` | 📋 Listar |
| GET | `/api/order/{id}` | 🔍 Detalle |
| POST | `/api/order` | ➕ Crear |

## 📈 Tecnologías por Versión

### Antes (❌ .NET Core 3.1)
```
.NET Core:        3.1
EF Core:          3.1.1
MediatR:          8.0.0
AutoMapper:       9.0.0
JWT:              5.6.0
Health Checks:    3.0.9
```

### Ahora (✅ .NET 9.0)
```
.NET:             9.0
EF Core:          9.0.0
MediatR:          12.4.1
AutoMapper:       13.0.1
JWT:              8.2.1
Health Checks:    8.0.2
```

## 🚀 Inicio Rápido

### 1️⃣ Configurar Base de Datos

**Cadena de Conexión:**
```
Server=localhost\SQLEXPRESS;
Database=KodotiCommerceDb;
Trusted_Connection=True;
MultipleActiveResultSets=true;
TrustServerCertificate=True
```

**Aplicar Migraciones:** ✅ Ya aplicadas

### 2️⃣ Ejecutar Servicios

```powershell
# Terminal 1 - Identity
cd src\Services\Identity\Identity.Api
dotnet run

# Terminal 2 - Catalog
cd src\Services\Catalog\Catalog.Api
dotnet run

# Terminal 3 - Customer
cd src\Services\Customer\Customer.Api
dotnet run

# Terminal 4 - Order
cd src\Services\Order\Order.Api
dotnet run
```

### 3️⃣ Verificar Health Checks

- ✅ http://localhost:10000/hc
- ✅ http://localhost:20000/hc
- ✅ http://localhost:30000/hc
- ✅ http://localhost:40000/hc

### 4️⃣ Credenciales de Prueba

```
Email:    admin@kodoti.com
Password: Pa$$w0rd!
```

## 📚 Documentación Completa

| Documento | Contenido | Icono |
|-----------|-----------|-------|
| [DOCUMENTATION_INDEX.md](./DOCUMENTATION_INDEX.md) | 📑 Índice completo de toda la documentación | 🏠 |
| [DATABASE_SCHEMA.md](./DATABASE_SCHEMA.md) | 🗄️ Diagrama ER completo con relaciones | 📊 |
| [DATABASE_DIAGRAM.md](./DATABASE_DIAGRAM.md) | 🎨 Diagramas simplificados y visuales | 🖼️ |
| [MICROSERVICES_DIAGRAMS.md](./MICROSERVICES_DIAGRAMS.md) | 🏗️ Arquitectura detallada por microservicio | 🔧 |
| [MIGRATION_TO_NET9.md](./MIGRATION_TO_NET9.md) | 📝 Detalles técnicos de migración | 🔄 |
| [QUICK_START_NET9.md](./QUICK_START_NET9.md) | ⚡ Guía rápida de inicio | 🚀 |

## 🎯 Navegación Rápida

```mermaid
graph TB
    Start[📚 Comienza Aquí]
    
    Start --> New{¿Eres nuevo?}
    New -->|Sí| Quick[📖 QUICK_START_NET9.md]
    New -->|No| Exp[🔍 ¿Qué necesitas?]
    
    Exp --> DB{Base de Datos?}
    Exp --> Arch{Arquitectura?}
    Exp --> Mig{Migración?}
    
    DB -->|Ver tablas| Schema[📊 DATABASE_SCHEMA.md]
    DB -->|Vista rápida| Diag[🎨 DATABASE_DIAGRAM.md]
    
    Arch -->|Detalle| Micro[🏗️ MICROSERVICES_DIAGRAMS.md]
    Arch -->|General| Index[📑 DOCUMENTATION_INDEX.md]
    
    Mig -->|Técnico| Tech[📝 MIGRATION_TO_NET9.md]
    Mig -->|Resumen| Sum[📋 MIGRATION_SUMMARY.md]
    
    style Start fill:#4caf50,color:#fff
    style Quick fill:#2196f3,color:#fff
    style Schema fill:#ff9800,color:#fff
    style Micro fill:#9c27b0,color:#fff
```

## ✅ Estado del Proyecto

| Componente | Estado | Notas |
|------------|--------|-------|
| ✅ Migración a .NET 9 | 100% | 36/36 proyectos |
| ✅ Compilación | Sin errores | 0 warnings críticos |
| ✅ Tests Unitarios | 4/4 pasando | 100% éxito |
| ✅ Base de Datos | Configurada | 13 tablas creadas |
| ✅ Migraciones | Aplicadas | 4/4 servicios |
| ✅ Usuario Admin | Insertado | Credenciales funcionando |
| ✅ Documentación | Completa | 10+ documentos |

## 🎉 ¡Todo Listo!

El proyecto está completamente migrado a .NET 9 y listo para usar. Todos los diagramas se visualizan automáticamente en GitHub gracias a **Mermaid**.

**🔗 Enlaces Útiles:**
- 🏠 [Volver al README](./README.md)
- 📑 [Índice Completo](./DOCUMENTATION_INDEX.md)
- 📊 [Ver Esquema de BD](./DATABASE_SCHEMA.md)

---

**Última actualización:** 2025-10-04  
**Versión:** .NET 9.0  
**Estado:** ✅ Producción Ready

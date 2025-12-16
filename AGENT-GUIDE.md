# Guía del Agente Experto en .NET Backend

## 🤖 Descripción del Agente

Este proyecto está configurado con un **agente experto en backend .NET Core** que utiliza las mejores prácticas y herramientas modernas para desarrollo de microservicios.

## 🎯 Capacidades Principales

### Desarrollo Backend
- ✅ .NET 9 / ASP.NET Core
- ✅ Entity Framework Core
- ✅ SQL Server
- ✅ Redis para caché distribuido
- ✅ Docker y Docker Compose
- ✅ Arquitectura de Microservicios

### Patrones de Diseño
- ✅ Repository Pattern
- ✅ Unit of Work
- ✅ CQRS con MediatR
- ✅ Dependency Injection
- ✅ Factory Pattern
- ✅ Strategy Pattern

### Mejores Prácticas
- ✅ Principios SOLID
- ✅ Clean Architecture
- ✅ Async/Await
- ✅ Logging estructurado
- ✅ Validación de inputs
- ✅ Manejo de errores robusto

## 🔧 Configuración

### Archivos de Configuración

1. **`.clinerules`** - Reglas y estándares del proyecto
2. **`.claude/agent-config.json`** - Configuración detallada del agente
3. **`.mcp.json`** - Configuración de MCP Servers (Context7, MercadoPago)

### MCP Servers Disponibles

#### Context7
Proporciona información actualizada sobre:
- Mejores prácticas de .NET Core
- Patrones de diseño
- Nuevas características de Entity Framework Core
- Soluciones a problemas comunes

**Uso:**
```bash
npx -y @upshiftone/context7
```

#### MercadoPago
Para integración de pagos (si es necesario)

**Uso:**
```bash
npx -y mcp-remote https://mcp.mercadopago.com/mcp \
  --header "Authorization:Bearer <TOKEN>"
```

## 📋 Flujo de Trabajo del Agente

### 1. Antes de Programar
```
┌─────────────────────────────────────────┐
│ 1. Consultar Context7 MCP              │
│    - Verificar mejores prácticas       │
│    - Buscar soluciones similares       │
└─────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────┐
│ 2. Revisar Arquitectura Existente      │
│    - Analizar código relacionado       │
│    - Mantener consistencia             │
└─────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────┐
│ 3. Planificar Implementación           │
│    - Definir clases y métodos          │
│    - Elegir patrones apropiados        │
└─────────────────────────────────────────┘
```

### 2. Durante el Desarrollo
```csharp
// ✅ Buenas Prácticas Aplicadas

// 1. Async/Await para operaciones I/O
public async Task<ProductDto> GetProductAsync(int id)
{
    return await _repository.GetByIdAsync(id);
}

// 2. AsNoTracking para queries de solo lectura
public async Task<List<Product>> GetAllProductsAsync()
{
    return await _context.Products
        .AsNoTracking()
        .ToListAsync();
}

// 3. Manejo de errores con logging
try
{
    var result = await _service.ProcessOrderAsync(orderId);
    _logger.LogInformation($"Order {orderId} processed successfully");
    return Ok(result);
}
catch (Exception ex)
{
    _logger.LogError(ex, $"Error processing order {orderId}");
    return StatusCode(500, "Internal server error");
}

// 4. Validación de inputs
if (string.IsNullOrWhiteSpace(request.Query))
{
    return BadRequest("Query parameter is required");
}

// 5. Documentación XML
/// <summary>
/// Busca productos con filtros avanzados
/// </summary>
/// <param name="request">Parámetros de búsqueda</param>
/// <returns>Lista paginada de productos</returns>
[HttpGet("search")]
public async Task<ActionResult<ProductSearchResponse>> Search(
    [FromQuery] ProductSearchRequest request)
{
    // ...
}
```

### 3. Después de Programar
```
┌─────────────────────────────────────────┐
│ 1. Compilar y Verificar Warnings       │
│    dotnet build                        │
└─────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────┐
│ 2. Revisar Código                      │
│    - Seguir convenciones              │
│    - Sin código duplicado             │
└─────────────────────────────────────────┘
          │
          ▼
┌─────────────────────────────────────────┐
│ 3. Actualizar Documentación            │
│    - README.md                         │
│    - Comentarios XML                   │
└─────────────────────────────────────────┘
```

## 🎨 Estándares de Código

### Naming Conventions
```csharp
// Clases: PascalCase
public class ProductService { }

// Interfaces: IPascalCase
public interface IProductRepository { }

// Métodos: PascalCase
public async Task<Product> GetProductByIdAsync(int id) { }

// Variables: camelCase
var productList = new List<Product>();

// Campos privados: _camelCase
private readonly IProductRepository _repository;

// Constantes: UPPER_CASE
private const int MAX_PAGE_SIZE = 100;
```

### Organización de Archivos
```
Service.Name/
├── Service.Name.Api/              # API Controllers
│   ├── Controllers/
│   ├── Swagger/
│   └── Program.cs
├── Service.Name.Application/       # Lógica de negocio
│   ├── Commands/
│   ├── Queries/
│   └── Validators/
├── Service.Name.Domain/            # Entidades y lógica de dominio
│   ├── Entities/
│   └── Interfaces/
├── Service.Name.Persistence/       # Acceso a datos
│   ├── Configuration/
│   ├── Repositories/
│   └── ApplicationDbContext.cs
└── Service.Name.Common/            # DTOs y utilidades
    └── DTOs/
```

## 🚀 Comandos Útiles

### Desarrollo
```bash
# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run

# Watch mode (recarga automática)
dotnet watch run
```

### Entity Framework
```bash
# Crear migración
dotnet ef migrations add NombreMigracion -p Service.Persistence -s Service.Api

# Aplicar migración
dotnet ef database update -p Service.Persistence -s Service.Api

# Revertir última migración
dotnet ef migrations remove -p Service.Persistence -s Service.Api

# Ver script SQL
dotnet ef migrations script -p Service.Persistence -s Service.Api
```

### Docker
```bash
# Iniciar todos los servicios
docker-compose up -d

# Ver logs
docker-compose logs -f

# Detener servicios
docker-compose down

# Reconstruir imágenes
docker-compose up --build
```

### Git
```bash
# Verificar estado
git status

# Agregar cambios
git add .

# Commit con mensaje descriptivo
git commit -m "feat: Agregar búsqueda avanzada con facetas dinámicas"

# Push
git push origin main
```

## 📊 Optimización de Performance

### Entity Framework Core
```csharp
// ✅ CORRECTO: AsNoTracking para queries de solo lectura
var products = await _context.Products
    .AsNoTracking()
    .Where(p => p.CategoryId == categoryId)
    .ToListAsync();

// ✅ CORRECTO: Proyección para obtener solo campos necesarios
var productDtos = await _context.Products
    .Select(p => new ProductDto
    {
        Id = p.ProductId,
        Name = p.Name,
        Price = p.Price
    })
    .ToListAsync();

// ❌ INCORRECTO: N+1 Problem
var products = await _context.Products.ToListAsync();
foreach (var product in products)
{
    product.Category = await _context.Categories
        .FindAsync(product.CategoryId); // ¡Consulta por cada producto!
}

// ✅ CORRECTO: Include para cargar relaciones
var products = await _context.Products
    .Include(p => p.Category)
    .Include(p => p.Brand)
    .ToListAsync();
```

### Caché con Redis
```csharp
// Patrón de uso de caché
var cacheKey = $"product:{productId}";
var cachedProduct = await _cacheService.GetAsync<ProductDto>(cacheKey);

if (cachedProduct != null)
{
    _logger.LogInformation($"Product {productId} retrieved from cache");
    return cachedProduct;
}

// Si no está en caché, consultar base de datos
var product = await _repository.GetByIdAsync(productId);

// Guardar en caché con TTL
await _cacheService.SetAsync(cacheKey, product, TimeSpan.FromMinutes(30));

return product;
```

## 🔒 Seguridad

### Validación de Inputs
```csharp
// FluentValidation
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name cannot exceed 200 characters");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0");

        RuleFor(x => x.CategoryId)
            .GreaterThan(0).WithMessage("Valid category is required");
    }
}
```

### Autenticación JWT
```csharp
[Authorize]
[ApiController]
[Route("api/v1/[controller]")]
public class ProductController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]  // Endpoint público
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        // ...
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]  // Solo administradores
    public async Task<ActionResult<Product>> CreateProduct(CreateProductRequest request)
    {
        // ...
    }
}
```

## 📝 Logging

### Structured Logging
```csharp
// ✅ CORRECTO: Logging estructurado
_logger.LogInformation(
    "Searching products with query: {Query}, CategoryId: {CategoryId}, Page: {Page}",
    request.Query,
    request.CategoryId,
    request.Page
);

// ❌ INCORRECTO: String interpolation en logs
_logger.LogInformation($"Searching products with query: {request.Query}");
```

### Correlation IDs
```csharp
// Middleware para agregar CorrelationId
app.Use(async (context, next) =>
{
    var correlationId = Guid.NewGuid().ToString();
    context.Items["CorrelationId"] = correlationId;
    context.Response.Headers.Add("X-Correlation-ID", correlationId);

    _logger.LogInformation("Request started: {CorrelationId}", correlationId);
    await next();
    _logger.LogInformation("Request completed: {CorrelationId}", correlationId);
});
```

## 📚 Recursos Adicionales

### Documentación Oficial
- [.NET Documentation](https://docs.microsoft.com/dotnet)
- [Entity Framework Core](https://docs.microsoft.com/ef/core)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core)

### Patrones de Microservicios
- [Microservices.io](https://microservices.io/patterns)
- [Microsoft Architecture Guides](https://docs.microsoft.com/azure/architecture/microservices/)

### Documentación del Proyecto
- `README.md` - Guía principal del proyecto
- `ARCHITECTURE.md` - Arquitectura del sistema
- `DATABASE_SCHEMA.md` - Esquema de base de datos
- `API-ROUTES-ANALYSIS.md` - Análisis de rutas de API

## 🤝 Contribución

Al trabajar en este proyecto:
1. **Consulta Context7** antes de implementar algo nuevo
2. **Mantén la consistencia** con el código existente
3. **Documenta tus cambios** con commits descriptivos
4. **Sigue los estándares** definidos en `.clinerules`
5. **Agrega tests** para funcionalidad crítica
6. **Actualiza la documentación** cuando sea necesario

---

**Última actualización**: Diciembre 2025  
**Versión del Agente**: 1.0.0  
**Tecnologías**: .NET 9, EF Core, SQL Server, Redis, Docker

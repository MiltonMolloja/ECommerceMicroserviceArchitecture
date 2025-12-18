# Plan de Testing - ECommerce Microservices

## 📊 Estado Actual de Tests

### ✅ Proyectos de Test Existentes
- **Catalog.Tests** - 1 test file (ProductInStockUpdateStockEventHandlerTest.cs)
- **Cart.Tests** - Proyecto existe, sin tests
- **Payment.Tests** - Proyecto existe, sin tests
- **Notification.Tests** - Proyecto existe, sin tests

### ✅ Recién Creado
- **Order.Tests** - Proyecto creado, listo para tests

### ❌ Proyectos Faltantes
- **Identity.Tests** - No existe
- **Customer.Tests** - No existe
- **Api.Gateway.Tests** - No existe

---

## 🎯 Tests Prioritarios por Servicio

### 1. Order.Tests (ALTA PRIORIDAD)

#### Unit Tests
- ✅ **OrderCreateEventHandler**
  - Crear orden con items válidos
  - Validar cálculo de totales
  - Validar stock disponible
  - Publicar OrderCreatedEvent
  - Manejar errores de stock insuficiente

- ✅ **UpdateOrderStatusEventHandler**
  - Actualizar estado de orden
  - Validar transiciones de estado válidas
  - Publicar OrderStatusChangedEvent

- ✅ **PaymentCompletedConsumer**
  - Actualizar orden a "Paid"
  - Validar que la orden existe
  - Manejar orden ya pagada

- ✅ **PaymentFailedConsumer**
  - Actualizar orden a "PaymentFailed"
  - Liberar stock reservado

#### Integration Tests
- Flujo completo: Crear orden → Procesar pago → Actualizar estado
- Integración con RabbitMQ (OrderCreatedEvent)

---

### 2. Identity.Tests (ALTA PRIORIDAD)

#### Unit Tests
- ✅ **UserCreateEventHandler**
  - Registrar usuario nuevo
  - Validar email único
  - Hash de password correcto
  - Publicar CustomerRegisteredEvent

- ✅ **UserLoginEventHandler**
  - Login exitoso con credenciales válidas
  - Login fallido con credenciales inválidas
  - Generar JWT token
  - Generar refresh token

- ✅ **RefreshTokenService**
  - Generar refresh token
  - Validar refresh token
  - Revocar refresh token
  - Manejar tokens expirados

#### Integration Tests
- Flujo completo: Registro → Login → Refresh Token

---

### 3. Payment.Tests (MEDIA PRIORIDAD)

#### Unit Tests
- ✅ **ProcessPaymentEventHandler**
  - Procesar pago exitoso
  - Procesar pago fallido
  - Validar monto
  - Publicar PaymentCompletedEvent
  - Publicar PaymentFailedEvent

- ✅ **MercadoPago Integration**
  - Crear preferencia de pago
  - Procesar webhook
  - Validar firma de webhook

#### Integration Tests
- Flujo completo con MercadoPago sandbox

---

### 4. Catalog.Tests (MEDIA PRIORIDAD)

#### Unit Tests Existentes
- ✅ ProductInStockUpdateStockEventHandler (4 tests)

#### Tests Adicionales Necesarios
- ✅ **ProductQueryService**
  - Buscar productos por categoría
  - Buscar productos por nombre
  - Filtrar por precio
  - Filtrar por atributos
  - Paginación

- ✅ **OrderCreatedConsumer**
  - Reservar stock al crear orden
  - Publicar StockUpdatedEvent

- ✅ **OrderCancelledConsumer**
  - Liberar stock al cancelar orden

---

### 5. Cart.Tests (MEDIA PRIORIDAD)

#### Unit Tests
- ✅ **ShoppingCart Domain**
  - Agregar item al carrito
  - Remover item del carrito
  - Actualizar cantidad
  - Aplicar cupón de descuento
  - Calcular totales
  - Limpiar carrito

- ✅ **OrderCreatedConsumer**
  - Limpiar carrito al crear orden

- ✅ **CartAbandonmentService**
  - Detectar carritos abandonados
  - Publicar CartAbandonedEvent

---

### 6. Customer.Tests (BAJA PRIORIDAD)

#### Unit Tests
- ✅ **ClientCreateEventHandler**
  - Crear cliente
  - Validar datos requeridos

- ✅ **ClientUpdateEventHandler**
  - Actualizar información del cliente

---

### 7. Notification.Tests (BAJA PRIORIDAD)

#### Unit Tests
- ✅ **PaymentCompletedConsumer**
  - Enviar email de confirmación
  - Validar template de email

- ✅ **OrderShippedConsumer**
  - Enviar email de envío
  - Incluir tracking number

- ✅ **CustomerRegisteredConsumer**
  - Enviar email de bienvenida

- ✅ **EmailTemplateService**
  - Renderizar templates
  - Reemplazar variables

---

### 8. Integration Tests (BAJA PRIORIDAD)

#### RabbitMQ Integration
- ✅ Publicar y consumir OrderCreatedEvent
- ✅ Publicar y consumir PaymentCompletedEvent
- ✅ Dead Letter Queue funcionando
- ✅ Retry policy funcionando

#### Database Integration
- ✅ Migrations aplicadas correctamente
- ✅ Transacciones funcionando
- ✅ Concurrencia optimista

---

## 📝 Estructura de Tests Recomendada

### Patrón AAA (Arrange-Act-Assert)
```csharp
[TestMethod]
public async Task Should_CreateOrder_When_ValidData()
{
    // Arrange
    var context = GetInMemoryDbContext();
    var publishEndpoint = new Mock<IPublishEndpoint>();
    var handler = new OrderCreateEventHandler(context, publishEndpoint.Object, GetLogger());
    
    var command = new OrderCreateCommand
    {
        ClientId = 1,
        Items = new List<OrderItemDto>
        {
            new OrderItemDto { ProductId = 1, Quantity = 2, UnitPrice = 100 }
        }
    };
    
    // Act
    var result = await handler.Handle(command, CancellationToken.None);
    
    // Assert
    result.Should().NotBeNull();
    result.OrderId.Should().BeGreaterThan(0);
    result.Total.Should().Be(200);
    
    publishEndpoint.Verify(x => x.Publish(
        It.IsAny<OrderCreatedEvent>(), 
        It.IsAny<CancellationToken>()), 
        Times.Once);
}
```

---

## 🛠️ Herramientas de Testing

### Frameworks
- ✅ **MSTest** - Framework de testing
- ✅ **Moq** - Mocking
- ✅ **FluentAssertions** - Assertions legibles
- ✅ **EF Core InMemory** - Base de datos en memoria

### Adicionales Recomendados
- ❌ **xUnit** - Alternativa a MSTest (más moderno)
- ❌ **NSubstitute** - Alternativa a Moq
- ❌ **AutoFixture** - Generación de datos de prueba
- ❌ **Bogus** - Datos fake realistas
- ❌ **TestContainers** - Contenedores para integration tests
- ❌ **WireMock.Net** - Mock de APIs externas

---

## 📊 Cobertura de Código Objetivo

### Mínimo Aceptable
- **Unit Tests:** 70% coverage
- **Integration Tests:** 50% coverage
- **E2E Tests:** Flujos críticos

### Ideal
- **Unit Tests:** 85%+ coverage
- **Integration Tests:** 70%+ coverage
- **E2E Tests:** Todos los flujos de usuario

---

## 🚀 Plan de Implementación

### Fase 1: Tests Críticos (1-2 días)
1. ✅ Order.Tests - OrderCreateEventHandler
2. ✅ Order.Tests - Consumers (Payment)
3. ✅ Identity.Tests - Login/Register
4. ✅ Payment.Tests - ProcessPayment

### Fase 2: Tests Importantes (2-3 días)
5. ✅ Catalog.Tests - Queries y Stock
6. ✅ Cart.Tests - Domain logic
7. ✅ Notification.Tests - Consumers

### Fase 3: Integration Tests (2-3 días)
8. ✅ RabbitMQ integration tests
9. ✅ Database integration tests
10. ✅ E2E tests de flujos críticos

---

## 📋 Comandos Útiles

```bash
# Ejecutar todos los tests
dotnet test

# Ejecutar tests de un proyecto específico
dotnet test src/Services/Order/Order.Tests

# Ejecutar tests con cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ejecutar solo tests de una categoría
dotnet test --filter "Category=Unit"

# Ejecutar tests en paralelo
dotnet test --parallel

# Ver resultados detallados
dotnet test --logger "console;verbosity=detailed"
```

---

## 🎯 Próximos Pasos

¿Qué quieres hacer?

1. **Generar tests para Order.Tests** (los más críticos)
2. **Crear proyecto Identity.Tests** y sus tests
3. **Completar tests de Catalog.Tests**
4. **Crear todos los proyectos de tests faltantes**
5. **Configurar CI/CD con tests automáticos**

Dime cuál prefieres y empiezo a generar los tests.

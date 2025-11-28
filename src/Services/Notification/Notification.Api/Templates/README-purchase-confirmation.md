# Template de Confirmación de Compra

## Descripción
Este template HTML (`purchase-confirmation.html`) se utiliza para enviar un email de confirmación de compra al cliente cuando un pago ha sido procesado exitosamente.

## Variables del Template

El template acepta las siguientes variables:

### Información del Cliente
- **CustomerName**: Nombre del cliente que realizó la compra

### Información del Pedido
- **OrderNumber**: Número de orden/pedido (ej: "#ORD-2025-112601")
- **Subtotal**: Subtotal de la orden formateado (ej: "$2,699.96")
- **ShippingCost**: Costo de envío formateado (ej: "Gratis" o "$50.00")
- **Tax**: Impuestos formateados (ej: "$143.96")
- **Total**: Total de la orden formateado (ej: "$2,843.92")
- **EstimatedDelivery**: Tiempo estimado de entrega (ej: "3-5 días hábiles")

### Lista de Productos
- **Items**: Array de productos comprados. Cada item debe contener:
  - **ProductName**: Nombre del producto
  - **Quantity**: Cantidad comprada
  - **UnitPrice**: Precio unitario formateado (ej: "$899.99")

### URLs de Navegación
- **TrackOrderUrl**: URL para rastrear el pedido
- **ReceiptUrl**: URL para ver el recibo
- **MyAccountUrl**: URL a "Mi Cuenta"
- **MyOrdersUrl**: URL a "Mis Órdenes"
- **SupportUrl**: URL a soporte
- **ReturnsUrl**: URL a devoluciones

## Ejemplo de Uso

### Desde EmailTemplateServiceV2

```csharp
var (subject, htmlBody, textBody) = await _emailTemplateService.RenderTemplateAsync(
    "purchase-confirmation",
    new
    {
        CustomerName = "Milton",
        OrderNumber = "#ORD-2025-112601",
        Subtotal = "$2,699.96",
        ShippingCost = "Gratis",
        Tax = "$143.96",
        Total = "$2,843.92",
        EstimatedDelivery = "3-5 días hábiles",
        Items = new[]
        {
            new
            {
                ProductName = "ASUS ROG Gaming Laptop",
                Quantity = 1,
                UnitPrice = "$899.99"
            },
            new
            {
                ProductName = "Lenovo IdeaPad Gaming 16",
                Quantity = 1,
                UnitPrice = "$599.99"
            },
            new
            {
                ProductName = "HP Victus 15 Gaming Laptop",
                Quantity = 2,
                UnitPrice = "$1,199.98"
            }
        }
    }
);

// Enviar el email
await _emailService.SendEmailAsync(
    to: "customer@example.com",
    subject: subject,
    htmlBody: htmlBody,
    textBody: textBody
);
```

### Desde NotificationController

Para enviar una notificación de confirmación de compra con email, se debe llamar al endpoint `/api/notification/send` con el siguiente payload:

```json
{
  "userId": 1,
  "type": 1,  // OrderPlaced
  "priority": 2,  // High
  "channels": [1, 2],  // InApp, Email
  "variables": {
    "CustomerName": "Milton",
    "OrderNumber": "#ORD-2025-112601",
    "Subtotal": "$2,699.96",
    "ShippingCost": "Gratis",
    "Tax": "$143.96",
    "Total": "$2,843.92",
    "EstimatedDelivery": "3-5 días hábiles",
    "Items": [
      {
        "ProductName": "ASUS ROG Gaming Laptop",
        "Quantity": 1,
        "UnitPrice": "$899.99"
      },
      {
        "ProductName": "Lenovo IdeaPad Gaming 16",
        "Quantity": 1,
        "UnitPrice": "$599.99"
      },
      {
        "ProductName": "HP Victus 15 Gaming Laptop",
        "Quantity": 2,
        "UnitPrice": "$1,199.98"
      }
    ]
  }
}
```

## Integración con Payment Service

Cuando un pago es procesado exitosamente en el `ProcessPaymentEventHandler`, se debe:

1. Obtener los detalles de la orden desde el Order Service
2. Obtener los detalles del cliente desde el Customer Service
3. Formatear los montos con símbolo de moneda
4. Enviar la notificación con todos los datos necesarios

### Ejemplo de integración:

```csharp
// En ProcessPaymentEventHandler después de payment.MarkAsCompleted()

// 1. Obtener detalles completos de la orden
var orderDetails = await _orderProxy.GetOrderWithDetailsAsync(notification.OrderId);

// 2. Obtener información del cliente
var customer = await _customerProxy.GetCustomerByIdAsync(notification.UserId);

// 3. Preparar los items
var items = orderDetails.Items.Select(item => new
{
    ProductName = item.ProductName,
    Quantity = item.Quantity,
    UnitPrice = FormatCurrency(item.UnitPrice * item.Quantity)
}).ToList();

// 4. Enviar notificación de confirmación de compra
await _notificationProxy.SendOrderConfirmationAsync(
    userId: notification.UserId,
    customerName: customer.Name,
    orderNumber: $"#ORD-{orderDetails.OrderId:D8}",
    items: items,
    subtotal: FormatCurrency(orderDetails.Subtotal),
    shippingCost: orderDetails.ShippingCost > 0 ? FormatCurrency(orderDetails.ShippingCost) : "Gratis",
    tax: FormatCurrency(orderDetails.Tax),
    total: FormatCurrency(orderDetails.Total),
    estimatedDelivery: "3-5 días hábiles"
);
```

## Notas Importantes

1. **Formateo de Moneda**: Todos los valores monetarios deben venir pre-formateados con el símbolo de moneda y separadores de miles.

2. **Lista de Productos**: El template usa `{{#each Items}}` para iterar sobre los productos. Cada producto debe tener las propiedades `ProductName`, `Quantity` y `UnitPrice`.

3. **URLs**: Las URLs se generan automáticamente en `EmailTemplateServiceV2.RenderPurchaseConfirmationTemplate()` usando la configuración `FrontendUrl`.

4. **Responsive**: El template es completamente responsive y se adapta a dispositivos móviles.

5. **Branding**: El template usa los colores de la marca ECommerce:
   - Header: #2c3e50 (azul oscuro)
   - Accent: #f39c12 (naranja)
   - Success: #27ae60 (verde)

## Vista Previa

El email incluye:
- ✅ Icono de confirmación con recibo
- 📋 Número de orden destacado
- 📦 Lista de productos comprados
- 💰 Resumen de costos (subtotal, envío, impuestos, total)
- 🚚 Información de entrega estimada
- 🔘 Botones de acción (Rastrear Pedido, Ver Recibo)
- 🔗 Links de navegación en el footer

## Relacionado

- Template: `purchase-confirmation.html`
- Service: `EmailTemplateServiceV2.cs`
- Notification Type: `OrderPlaced` (enum value: 1)
- Priority: `High` (enum value: 2)
- Channels: `InApp`, `Email`

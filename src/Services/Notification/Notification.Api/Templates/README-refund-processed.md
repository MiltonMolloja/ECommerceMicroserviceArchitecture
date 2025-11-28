# Template: Refund Processed Email

## Descripción
Template de email para notificar a los clientes cuando su reembolso ha sido procesado exitosamente.

## Variables del Template

### Información del Cliente
- `{{CustomerName}}` - Nombre del cliente (ej: "Milton")

### Detalles del Reembolso
- `{{RefundNumber}}` - Número único del reembolso (ej: "#REF-2025-87491")
- `{{RefundReason}}` - Razón por la cual se procesó el reembolso (ej: "Producto Defectuoso", "Insatisfacción del Cliente", "Error en el Pedido")
- `{{ProcessedDate}}` - Fecha y hora en que se procesó el reembolso (ej: "26/11/2025 - 14:32:18")
- `{{RefundMethod}}` - Método por el cual se realizará el reembolso (ej: "Tarjeta Crédito (**** 4532)", "PayPal", "Transferencia Bancaria")
- `{{RefundAmount}}` - Monto total reembolsado (ej: "$899.99", "AR$ 145,000.00")

### Información del Pedido Original
- `{{OrderNumber}}` - Número del pedido original (ej: "#ORD-2025-112601")
- `{{PurchaseDate}}` - Fecha de la compra original (ej: "15/11/2025")

### Artículos Reembolsados (Array)
- `{{Items}}` - Array de productos reembolsados
  - `{{ProductName}}` - Nombre del producto (ej: "ASUS ROG Gaming Laptop")
  - `{{Quantity}}` - Cantidad de unidades (ej: "1", "2")
  - `{{Price}}` - Precio del artículo (ej: "$899.99")

### Cronología del Reembolso
- `{{ReturnReceivedDate}}` - Fecha en que se recibió la devolución (ej: "26/11/2025")
- `{{ProductVerifiedDate}}` - Fecha en que se verificó el producto (ej: "26/11/2025")
- `{{RefundProcessedDate}}` - Fecha en que se procesó el reembolso (ej: "26/11/2025")

### URLs de Acción
- `{{ViewDetailsUrl}}` - URL para ver los detalles completos del reembolso
- `{{SupportUrl}}` - URL para contactar soporte
- `{{MyAccountUrl}}` - URL de Mi Cuenta
- `{{MyReturnsUrl}}` - URL de Mis Devoluciones
- `{{FaqUrl}}` - URL de Preguntas Frecuentes

## Ejemplo de Uso en C#

```csharp
var templateData = new
{
    CustomerName = "Milton Rodriguez",
    RefundNumber = "#REF-2025-87491",
    RefundReason = "Producto Defectuoso",
    ProcessedDate = DateTime.Now.ToString("dd/MM/yyyy - HH:mm:ss"),
    RefundMethod = "Tarjeta de Crédito (**** 4532)",
    RefundAmount = "$899.99",
    OrderNumber = "#ORD-2025-112601",
    PurchaseDate = "15/11/2025",
    Items = new[]
    {
        new
        {
            ProductName = "ASUS ROG Gaming Laptop",
            Quantity = "1",
            Price = "$899.99"
        }
    },
    ReturnReceivedDate = "26/11/2025",
    ProductVerifiedDate = "26/11/2025",
    RefundProcessedDate = "26/11/2025",
    ViewDetailsUrl = "https://ecommerce.com/refunds/REF-2025-87491",
    SupportUrl = "https://ecommerce.com/support",
    MyAccountUrl = "https://ecommerce.com/account",
    MyReturnsUrl = "https://ecommerce.com/returns",
    FaqUrl = "https://ecommerce.com/faq"
};

var html = await _emailTemplateService.RenderTemplateAsync("refund-processed", templateData);
await _emailService.SendAsync(userEmail, "Reembolso Procesado", html);
```

## Datos Mockeados (Para Testing)

```json
{
  "CustomerName": "Milton Rodriguez",
  "RefundNumber": "#REF-2025-87491",
  "RefundReason": "Producto Defectuoso",
  "ProcessedDate": "26/11/2025 - 14:32:18",
  "RefundMethod": "Tarjeta de Crédito (**** 4532)",
  "RefundAmount": "AR$ 899.999,00",
  "OrderNumber": "#ORD-2025-112601",
  "PurchaseDate": "15/11/2025",
  "Items": [
    {
      "ProductName": "ASUS ROG Strix Gaming Laptop 15.6\" FHD",
      "Quantity": "1",
      "Price": "AR$ 899.999,00"
    }
  ],
  "ReturnReceivedDate": "24/11/2025",
  "ProductVerifiedDate": "25/11/2025",
  "RefundProcessedDate": "26/11/2025",
  "ViewDetailsUrl": "https://ecommerce.com/refunds/REF-2025-87491",
  "SupportUrl": "https://ecommerce.com/support",
  "MyAccountUrl": "https://ecommerce.com/account",
  "MyReturnsUrl": "https://ecommerce.com/returns",
  "FaqUrl": "https://ecommerce.com/faq"
}
```

## Razones de Reembolso Comunes

- **Producto Defectuoso** - El producto llegó con defectos de fábrica
- **Producto Dañado** - El producto se dañó durante el envío
- **No Coincide con la Descripción** - El producto no es como se describió
- **Insatisfacción del Cliente** - El cliente no está satisfecho con el producto
- **Talla/Color Incorrecto** - Se envió la talla o color equivocado
- **Error en el Pedido** - Se cometió un error al procesar el pedido
- **Producto No Recibido** - El cliente nunca recibió el producto
- **Duplicado** - Compra duplicada accidentalmente
- **Cambio de Opinión** - El cliente cambió de opinión dentro del período de devolución

## Métodos de Reembolso Soportados

- **Tarjeta de Crédito** - `Tarjeta de Crédito (**** 4532)`
- **Tarjeta de Débito** - `Tarjeta de Débito (**** 7891)`
- **MercadoPago** - `MercadoPago (usuario@email.com)`
- **PayPal** - `PayPal (usuario@email.com)`
- **Transferencia Bancaria** - `Transferencia Bancaria (CBU: **** 5678)`
- **Crédito en Cuenta** - `Crédito en tu Cuenta de ECommerce`

## Notas Importantes

1. **Tiempos de Reembolso**: Informar siempre al cliente que el tiempo de procesamiento del banco puede variar (3-5 días hábiles típicamente, hasta 10 días para transferencias internacionales)

2. **Soporte**: Incluir siempre enlaces claros a soporte para consultas

3. **Transparencia**: La cronología debe mostrar claramente cada paso del proceso

4. **Números de Seguimiento**: Todos los números (#REF, #ORD) deben ser únicos y trazables

## Diseño UI

Este template sigue el diseño estándar de ECommerce:
- Colores principales: #232F3E (oscuro), #FF9900 (naranja), #4CAF50 (verde para reembolsos)
- Iconos emoji simples: 💰
- Responsive design optimizado para móviles
- Consistente con otros templates del sistema

## Personalización

Para personalizar el template:
1. Los colores se definen en las variables CSS al inicio
2. El verde (#4CAF50) se usa para indicar acciones positivas (reembolso procesado)
3. Mantener la estructura de bloques para consistencia visual

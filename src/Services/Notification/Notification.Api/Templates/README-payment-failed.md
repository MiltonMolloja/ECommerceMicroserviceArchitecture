# Template: Payment Failed Email

## Descripción
Template de email para notificar a los clientes cuando su pago ha sido rechazado y su pedido está pendiente.

## Variables del Template

### Información del Cliente
- `{{CustomerName}}` - Nombre del cliente (ej: "Milton Rodriguez")

### Detalles del Intento de Pago
- `{{OrderNumber}}` - Número del pedido (ej: "#ORD-2025-112601")
- `{{AttemptDate}}` - Fecha y hora del intento de pago (ej: "26/11/2025 14:32")
- `{{Amount}}` - Monto del pago intentado (ej: "$2,843.92", "AR$ 285,000.00")
- `{{PaymentMethod}}` - Método de pago utilizado (ej: "Visa terminada en 4242")

### Razón del Fallo
- `{{FailureReason}}` - Descripción detallada del motivo del rechazo

### URLs de Acción
- `{{RetryPaymentUrl}}` - URL para reintentar el pago
- `{{ViewOrderUrl}}` - URL para ver los detalles del pedido
- `{{MyAccountUrl}}` - URL de Mi Cuenta
- `{{MyOrdersUrl}}` - URL de Mis Órdenes
- `{{SupportUrl}}` - URL de Soporte
- `{{HelpCenterUrl}}` - URL del Centro de Ayuda

## Ejemplo de Uso en C#

```csharp
var templateData = new
{
    CustomerName = "Milton Rodriguez",
    OrderNumber = "#ORD-2025-112601",
    AttemptDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
    Amount = "$2,843.92",
    PaymentMethod = "Visa terminada en 4242",
    FailureReason = "Fondos insuficientes. Verifica que tu tarjeta tenga saldo suficiente e intenta de nuevo. Si el problema persiste, contacta con tu banco.",
    RetryPaymentUrl = "https://ecommerce.com/orders/ORD-2025-112601/payment",
    ViewOrderUrl = "https://ecommerce.com/orders/ORD-2025-112601",
    MyAccountUrl = "https://ecommerce.com/account",
    MyOrdersUrl = "https://ecommerce.com/orders",
    SupportUrl = "https://ecommerce.com/support",
    HelpCenterUrl = "https://ecommerce.com/help"
};

var html = await _emailTemplateService.RenderTemplateAsync("payment-failed", templateData);
await _emailService.SendAsync(userEmail, "Pago Rechazado - Acción Requerida", html);
```

## Datos Mockeados (Para Testing)

```json
{
  "CustomerName": "Milton Rodriguez",
  "OrderNumber": "#ORD-2025-112601",
  "AttemptDate": "26/11/2025 14:32",
  "Amount": "AR$ 2.843.920,00",
  "PaymentMethod": "Visa terminada en 4242",
  "FailureReason": "Fondos insuficientes. Verifica que tu tarjeta tenga saldo suficiente e intenta de nuevo. Si el problema persiste, contacta con tu banco.",
  "RetryPaymentUrl": "https://ecommerce.com/orders/ORD-2025-112601/payment",
  "ViewOrderUrl": "https://ecommerce.com/orders/ORD-2025-112601",
  "MyAccountUrl": "https://ecommerce.com/account",
  "MyOrdersUrl": "https://ecommerce.com/orders",
  "SupportUrl": "https://ecommerce.com/support",
  "HelpCenterUrl": "https://ecommerce.com/help"
}
```

## Razones Comunes de Rechazo de Pago

### Problemas de Fondos
- **Fondos Insuficientes**: "Fondos insuficientes. Verifica que tu tarjeta tenga saldo suficiente e intenta de nuevo."
- **Límite de Crédito Excedido**: "Has excedido tu límite de crédito. Contacta con tu banco para aumentar el límite o usa otro método de pago."

### Problemas de Tarjeta
- **Tarjeta Expirada**: "Tu tarjeta ha expirado. Por favor, actualiza la fecha de vencimiento o usa otro método de pago."
- **Tarjeta Rechazada**: "Tu banco rechazó la transacción. Contacta con tu entidad bancaria para más información."
- **CVV Incorrecto**: "El código de seguridad (CVV) es incorrecto. Verifica los datos de tu tarjeta."
- **Número de Tarjeta Inválido**: "El número de tarjeta ingresado es inválido. Por favor verifica los datos."

### Problemas de Seguridad
- **Autenticación 3D Secure Falló**: "La verificación de seguridad falló. Intenta nuevamente o contacta con tu banco."
- **Transacción Sospechosa**: "La transacción fue marcada como sospechosa por nuestro sistema de seguridad. Por favor contacta con soporte."
- **Tarjeta Bloqueada**: "Tu tarjeta está bloqueada para transacciones online. Contacta con tu banco."

### Problemas del Banco
- **Banco no Disponible**: "Tu banco no está disponible en este momento. Por favor intenta más tarde."
- **Procesador de Pagos no Disponible**: "Error temporal del procesador de pagos. Por favor intenta en unos minutos."

### Problemas de Configuración
- **Moneda no Soportada**: "Tu tarjeta no soporta transacciones en esta moneda. Usa otro método de pago."
- **País no Permitido**: "Tu banco no permite transacciones desde tu ubicación actual."

## Mapeo de Códigos de Error (MercadoPago)

```csharp
public static string GetFailureReasonFromCode(string errorCode)
{
    return errorCode switch
    {
        "cc_rejected_insufficient_amount" =>
            "Fondos insuficientes. Verifica que tu tarjeta tenga saldo suficiente e intenta de nuevo.",
        "cc_rejected_bad_filled_card_number" =>
            "El número de tarjeta ingresado es incorrecto. Por favor verifica los datos.",
        "cc_rejected_bad_filled_date" =>
            "La fecha de vencimiento es incorrecta. Verifica los datos de tu tarjeta.",
        "cc_rejected_bad_filled_security_code" =>
            "El código de seguridad (CVV) es incorrecto. Verifica los datos de tu tarjeta.",
        "cc_rejected_call_for_authorize" =>
            "Debes autorizar el pago con tu banco. Contacta con tu entidad bancaria.",
        "cc_rejected_card_disabled" =>
            "Tu tarjeta está deshabilitada. Contacta con tu banco o usa otro método de pago.",
        "cc_rejected_duplicated_payment" =>
            "Ya realizaste un pago por este monto recientemente.",
        "cc_rejected_high_risk" =>
            "La transacción fue rechazada por seguridad. Contacta con soporte.",
        "cc_rejected_max_attempts" =>
            "Alcanzaste el límite de intentos permitidos. Intenta nuevamente en 24 horas.",
        "cc_rejected_other_reason" =>
            "Tu banco rechazó la transacción. Contacta con tu entidad bancaria para más información.",
        _ => "No pudimos procesar el pago. Por favor intenta nuevamente o contacta con soporte."
    };
}
```

## Tiempos de Expiración del Pedido

- **Pedidos Estándar**: 24 horas
- **Pedidos de Alta Demanda**: 12 horas
- **Pedidos con Productos en Oferta**: 6 horas
- **Pedidos Reservados**: 48 horas

## Flujo de Reintento de Pago

1. **Notificación Inmediata**: Email enviado inmediatamente después del rechazo
2. **Recordatorio 1**: 6 horas después si no se completó el pago
3. **Recordatorio 2**: 18 horas después (6 horas antes de expirar)
4. **Cancelación Automática**: Si pasan 24 horas sin pago exitoso

## Métodos de Pago Mostrados

### Tarjetas de Crédito/Débito
- `Visa terminada en 4242`
- `Mastercard terminada en 5555`
- `American Express terminada en 3782`

### Billeteras Digitales
- `MercadoPago (usuario@email.com)`
- `PayPal (usuario@email.com)`

### Otros Métodos
- `Transferencia Bancaria`
- `Pago en Efectivo (Rapipago/Pago Fácil)`

## Integración con Payment Service

```csharp
public class PaymentFailedEventHandler : INotificationHandler<PaymentFailedEvent>
{
    private readonly INotificationProxy _notificationProxy;
    private readonly IOrderProxy _orderProxy;

    public async Task Handle(PaymentFailedEvent notification, CancellationToken cancellationToken)
    {
        // 1. Obtener detalles de la orden
        var order = await _orderProxy.GetOrderByIdAsync(notification.OrderId);

        // 2. Mapear el código de error a un mensaje amigable
        var failureReason = GetFailureReasonFromCode(notification.ErrorCode);

        // 3. Enviar email de pago fallido
        await _notificationProxy.SendPaymentFailedNotificationAsync(new PaymentFailedNotification
        {
            UserId = order.UserId,
            UserEmail = order.UserEmail,
            CustomerName = order.CustomerName,
            OrderNumber = $"#ORD-{order.OrderId:D8}",
            AttemptDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
            Amount = FormatCurrency(order.Total),
            PaymentMethod = GetPaymentMethodDisplay(notification.PaymentMethod, notification.Last4Digits),
            FailureReason = failureReason
        });

        // 4. Programar recordatorio si el pedido no expira pronto
        await _reminderService.SchedulePaymentReminderAsync(order.OrderId, TimeSpan.FromHours(6));
    }
}
```

## Diseño UI

Este template sigue el diseño estándar de ECommerce:
- Colores principales: #232F3E (oscuro), #FF9900 (naranja)
- Color de error: #F44336 (rojo) para el fallo de pago
- Iconos emoji simples: ❌
- Responsive design optimizado para móviles
- Consistente con otros templates del sistema

## Mejores Prácticas

1. **Tono Empático**: Usar lenguaje que no culpe al usuario
2. **Acción Clara**: Botón prominente para reintentar el pago
3. **Información Útil**: Explicar claramente por qué falló y qué hacer
4. **Urgencia Apropiada**: Mencionar la expiración del pedido sin ser alarmista
5. **Soporte Visible**: Ofrecer múltiples opciones de ayuda

## Variaciones Según el Error

### Error Temporal (Sistema)
```
Subject: "Problema Temporal con tu Pago - Intenta Nuevamente"
Tono: Disculpa por el inconveniente, es problema nuestro
```

### Error de Datos
```
Subject: "Verifica los Datos de tu Tarjeta"
Tono: Ayudamos a corregir los datos
```

### Error Bancario
```
Subject: "Tu Banco Rechazó el Pago - Necesitamos tu Ayuda"
Tono: Es problema del banco, ofrecemos alternativas
```

## Personalización

Para personalizar el template:
1. Los colores rojos (#F44336, #FFEBEE) indican error/alerta
2. Naranja (#FF9900) para información importante
3. Azul (#2196F3) para ayuda y soporte
4. Mantener la estructura de bloques para consistencia visual

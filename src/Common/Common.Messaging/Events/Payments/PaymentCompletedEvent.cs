namespace Common.Messaging.Events.Payments;

/// <summary>
/// Evento publicado cuando un pago se completa exitosamente.
/// Consumidores: Order.Service (actualizar estado), Notification.Service (enviar confirmación)
/// </summary>
public record PaymentCompletedEvent : IntegrationEvent
{
    /// <summary>
    /// ID del pago.
    /// </summary>
    public int PaymentId { get; init; }

    /// <summary>
    /// ID de la orden asociada.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// ID del cliente.
    /// </summary>
    public int ClientId { get; init; }

    /// <summary>
    /// Email del cliente para notificaciones.
    /// </summary>
    public string ClientEmail { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del cliente.
    /// </summary>
    public string ClientName { get; init; } = string.Empty;

    /// <summary>
    /// Monto pagado.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Método de pago utilizado.
    /// </summary>
    public string PaymentMethod { get; init; } = string.Empty;

    /// <summary>
    /// ID de transacción del proveedor de pagos (MercadoPago, etc.).
    /// </summary>
    public string? TransactionId { get; init; }

    /// <summary>
    /// Fecha y hora del pago.
    /// </summary>
    public DateTime PaidAt { get; init; }
}

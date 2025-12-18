namespace Common.Messaging.Events.Payments;

/// <summary>
/// Evento publicado cuando se procesa un reembolso.
/// Consumidores: Order.Service (actualizar estado), Notification.Service (notificar reembolso)
/// </summary>
public record RefundProcessedEvent : IntegrationEvent
{
    /// <summary>
    /// ID del pago original.
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
    /// Monto reembolsado.
    /// </summary>
    public decimal RefundAmount { get; init; }

    /// <summary>
    /// Razón del reembolso.
    /// </summary>
    public string Reason { get; init; } = string.Empty;

    /// <summary>
    /// ID de transacción del reembolso.
    /// </summary>
    public string? RefundTransactionId { get; init; }

    /// <summary>
    /// Fecha y hora del reembolso.
    /// </summary>
    public DateTime RefundedAt { get; init; }
}

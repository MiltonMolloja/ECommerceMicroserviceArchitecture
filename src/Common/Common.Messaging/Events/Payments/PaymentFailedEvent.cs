namespace Common.Messaging.Events.Payments;

/// <summary>
/// Evento publicado cuando un pago falla.
/// Consumidores: Order.Service (actualizar estado), Notification.Service (notificar fallo)
/// </summary>
public record PaymentFailedEvent : IntegrationEvent
{
    /// <summary>
    /// ID del pago (si se creó).
    /// </summary>
    public int? PaymentId { get; init; }

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
    /// Monto que se intentó pagar.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Código de error del proveedor de pagos.
    /// </summary>
    public string? ErrorCode { get; init; }

    /// <summary>
    /// Mensaje de error.
    /// </summary>
    public string ErrorMessage { get; init; } = string.Empty;

    /// <summary>
    /// Fecha y hora del intento de pago fallido.
    /// </summary>
    public DateTime FailedAt { get; init; }
}

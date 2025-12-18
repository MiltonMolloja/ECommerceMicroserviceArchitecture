namespace Common.Messaging.Events.Orders;

/// <summary>
/// Evento publicado cuando cambia el estado de una orden.
/// Consumidores: Notification.Service (notificar al cliente)
/// </summary>
public record OrderStatusChangedEvent : IntegrationEvent
{
    /// <summary>
    /// ID de la orden.
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
    /// Estado anterior de la orden.
    /// </summary>
    public string PreviousStatus { get; init; } = string.Empty;

    /// <summary>
    /// Nuevo estado de la orden.
    /// </summary>
    public string NewStatus { get; init; } = string.Empty;

    /// <summary>
    /// Razón del cambio de estado (opcional).
    /// </summary>
    public string? Reason { get; init; }
}

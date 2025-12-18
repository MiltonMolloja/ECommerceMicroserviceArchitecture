namespace Common.Messaging.Events.Orders;

/// <summary>
/// Evento publicado cuando se cancela una orden.
/// Consumidores: Catalog.Api (liberar stock), Payment.Api (procesar reembolso), Notification.Api (email)
/// </summary>
public record OrderCancelledEvent : IntegrationEvent
{
    /// <summary>
    /// ID de la orden cancelada.
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
    /// Total de la orden (para reembolso).
    /// </summary>
    public decimal Total { get; init; }

    /// <summary>
    /// Razón de la cancelación.
    /// </summary>
    public string CancellationReason { get; init; } = string.Empty;

    /// <summary>
    /// ID del pago asociado (si existe, para reembolso).
    /// </summary>
    public int? PaymentId { get; init; }

    /// <summary>
    /// Items de la orden (para liberar stock).
    /// </summary>
    public List<OrderItemInfo> Items { get; init; } = new();

    /// <summary>
    /// Fecha de cancelación.
    /// </summary>
    public DateTime CancelledAt { get; init; } = DateTime.UtcNow;
}

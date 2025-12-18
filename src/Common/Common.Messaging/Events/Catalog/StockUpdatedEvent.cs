namespace Common.Messaging.Events.Catalog;

/// <summary>
/// Evento publicado cuando se actualiza el stock de un producto.
/// Consumidores: Notification.Api (alertar disponibilidad), Cart.Api (validar carritos)
/// </summary>
public record StockUpdatedEvent : IntegrationEvent
{
    /// <summary>
    /// ID del producto.
    /// </summary>
    public int ProductId { get; init; }

    /// <summary>
    /// Nombre del producto.
    /// </summary>
    public string ProductName { get; init; } = string.Empty;

    /// <summary>
    /// Stock anterior.
    /// </summary>
    public int PreviousStock { get; init; }

    /// <summary>
    /// Stock actual.
    /// </summary>
    public int CurrentStock { get; init; }

    /// <summary>
    /// Indica si el producto volvió a estar disponible (estaba en 0 y ahora tiene stock).
    /// </summary>
    public bool IsBackInStock => PreviousStock == 0 && CurrentStock > 0;

    /// <summary>
    /// Indica si el producto se quedó sin stock.
    /// </summary>
    public bool IsOutOfStock => CurrentStock == 0 && PreviousStock > 0;

    /// <summary>
    /// Fecha de actualización.
    /// </summary>
    public DateTime UpdatedAt { get; init; } = DateTime.UtcNow;
}

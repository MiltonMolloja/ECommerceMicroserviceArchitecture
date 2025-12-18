namespace Common.Messaging.Events.Orders;

/// <summary>
/// Evento publicado cuando se crea una nueva orden.
/// Consumidores: Notification.Service (enviar confirmación), Inventory.Service (reservar stock)
/// </summary>
public record OrderCreatedEvent : IntegrationEvent
{
    /// <summary>
    /// ID de la orden creada.
    /// </summary>
    public int OrderId { get; init; }

    /// <summary>
    /// ID del cliente que realizó la orden.
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
    /// Total de la orden.
    /// </summary>
    public decimal Total { get; init; }

    /// <summary>
    /// Items de la orden.
    /// </summary>
    public List<OrderItemInfo> Items { get; init; } = new();
}

/// <summary>
/// Información de un item de la orden.
/// </summary>
public record OrderItemInfo
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

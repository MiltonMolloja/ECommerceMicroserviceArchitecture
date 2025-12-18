namespace Common.Messaging.Events.Orders;

/// <summary>
/// Evento publicado cuando una orden es entregada.
/// Consumidores: Notification.Service (notificar entrega, solicitar review)
/// </summary>
public record OrderDeliveredEvent : IntegrationEvent
{
    /// <summary>
    /// ID de la orden entregada.
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
    /// Fecha y hora de la entrega.
    /// </summary>
    public DateTime DeliveredAt { get; init; }

    /// <summary>
    /// Nombre de quien recibió el paquete.
    /// </summary>
    public string? ReceivedBy { get; init; }

    /// <summary>
    /// Firma digital o confirmación de entrega.
    /// </summary>
    public string? DeliverySignature { get; init; }

    /// <summary>
    /// Notas adicionales de la entrega.
    /// </summary>
    public string? DeliveryNotes { get; init; }

    /// <summary>
    /// Items entregados (para solicitar reviews).
    /// </summary>
    public List<DeliveredItemInfo> Items { get; init; } = new();
}

/// <summary>
/// Información de un item entregado.
/// </summary>
public record DeliveredItemInfo
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public int Quantity { get; init; }
}

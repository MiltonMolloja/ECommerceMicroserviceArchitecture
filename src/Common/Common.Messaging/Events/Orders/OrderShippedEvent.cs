namespace Common.Messaging.Events.Orders;

/// <summary>
/// Evento publicado cuando una orden es enviada.
/// Consumidores: Notification.Service (notificar envío al cliente)
/// </summary>
public record OrderShippedEvent : IntegrationEvent
{
    /// <summary>
    /// ID de la orden enviada.
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
    /// Número de seguimiento del envío.
    /// </summary>
    public string TrackingNumber { get; init; } = string.Empty;

    /// <summary>
    /// Empresa de transporte.
    /// </summary>
    public string Carrier { get; init; } = string.Empty;

    /// <summary>
    /// URL de seguimiento del envío.
    /// </summary>
    public string? TrackingUrl { get; init; }

    /// <summary>
    /// Fecha estimada de entrega.
    /// </summary>
    public DateTime? EstimatedDeliveryDate { get; init; }

    /// <summary>
    /// Fecha y hora del envío.
    /// </summary>
    public DateTime ShippedAt { get; init; }

    /// <summary>
    /// Dirección de envío.
    /// </summary>
    public ShippingAddressInfo ShippingAddress { get; init; } = new();
}

/// <summary>
/// Información de la dirección de envío.
/// </summary>
public record ShippingAddressInfo
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string State { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

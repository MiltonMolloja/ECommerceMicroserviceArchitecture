namespace Common.Messaging.Events.Cart;

/// <summary>
/// Evento publicado cuando un carrito es abandonado (sin actividad por X tiempo).
/// Consumidores: Notification.Api (email de recuperación)
/// </summary>
public record CartAbandonedEvent : IntegrationEvent
{
    /// <summary>
    /// ID del carrito abandonado.
    /// </summary>
    public int CartId { get; init; }

    /// <summary>
    /// ID del cliente.
    /// </summary>
    public int ClientId { get; init; }

    /// <summary>
    /// Email del cliente.
    /// </summary>
    public string ClientEmail { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del cliente.
    /// </summary>
    public string ClientName { get; init; } = string.Empty;

    /// <summary>
    /// Total del carrito.
    /// </summary>
    public decimal Total { get; init; }

    /// <summary>
    /// Cantidad de items en el carrito.
    /// </summary>
    public int ItemCount { get; init; }

    /// <summary>
    /// Items del carrito (para mostrar en el email).
    /// </summary>
    public List<CartItemInfo> Items { get; init; } = new();

    /// <summary>
    /// Última actividad del carrito.
    /// </summary>
    public DateTime LastActivityAt { get; init; }

    /// <summary>
    /// Fecha en que se detectó el abandono.
    /// </summary>
    public DateTime AbandonedAt { get; init; } = DateTime.UtcNow;
}

/// <summary>
/// Información de un item del carrito.
/// </summary>
public record CartItemInfo
{
    public int ProductId { get; init; }
    public string ProductName { get; init; } = string.Empty;
    public string? ProductImageUrl { get; init; }
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
}

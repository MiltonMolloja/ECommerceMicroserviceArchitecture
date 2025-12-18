namespace Common.Messaging.Events.Customers;

/// <summary>
/// Evento publicado cuando se registra un nuevo cliente.
/// Consumidores: Notification.Api (email de bienvenida)
/// </summary>
public record CustomerRegisteredEvent : IntegrationEvent
{
    /// <summary>
    /// ID del cliente registrado.
    /// </summary>
    public int CustomerId { get; init; }

    /// <summary>
    /// Email del cliente.
    /// </summary>
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// Nombre del cliente.
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Apellido del cliente.
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// Nombre completo del cliente.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Fecha de registro.
    /// </summary>
    public DateTime RegisteredAt { get; init; } = DateTime.UtcNow;
}

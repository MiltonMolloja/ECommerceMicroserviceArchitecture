namespace Common.Messaging.Events;

/// <summary>
/// Interfaz base para todos los eventos de integración entre microservicios.
/// </summary>
public interface IIntegrationEvent
{
    /// <summary>
    /// Identificador único del evento.
    /// </summary>
    Guid EventId { get; }

    /// <summary>
    /// Fecha y hora en que se creó el evento (UTC).
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// Correlation ID para trazabilidad entre servicios.
    /// </summary>
    string? CorrelationId { get; }
}

/// <summary>
/// Clase base abstracta para eventos de integración.
/// </summary>
public abstract record IntegrationEvent : IIntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public string? CorrelationId { get; init; }
}

namespace Common.Messaging.DeadLetter;

/// <summary>
/// Interface para manejar mensajes de Dead Letter Queue.
/// </summary>
public interface IDeadLetterHandler
{
    /// <summary>
    /// Procesa un mensaje de la Dead Letter Queue.
    /// </summary>
    /// <param name="message">Mensaje de DLQ.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>True si el mensaje fue procesado exitosamente.</returns>
    Task<bool> HandleAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Intenta reprocesar un mensaje de la Dead Letter Queue.
    /// </summary>
    /// <param name="message">Mensaje de DLQ.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    /// <returns>True si el mensaje fue reprocesado exitosamente.</returns>
    Task<bool> ReprocessAsync(DeadLetterMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Descarta un mensaje de la Dead Letter Queue.
    /// </summary>
    /// <param name="message">Mensaje de DLQ.</param>
    /// <param name="reason">Razón del descarte.</param>
    /// <param name="cancellationToken">Token de cancelación.</param>
    Task DiscardAsync(DeadLetterMessage message, string reason, CancellationToken cancellationToken = default);
}

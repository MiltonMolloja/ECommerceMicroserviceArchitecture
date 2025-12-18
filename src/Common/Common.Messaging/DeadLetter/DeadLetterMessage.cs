namespace Common.Messaging.DeadLetter;

/// <summary>
/// Representa un mensaje que fue enviado a la Dead Letter Queue.
/// </summary>
public class DeadLetterMessage
{
    /// <summary>
    /// Identificador único del mensaje.
    /// </summary>
    public Guid MessageId { get; set; }

    /// <summary>
    /// Tipo del mensaje original.
    /// </summary>
    public string MessageType { get; set; } = string.Empty;

    /// <summary>
    /// Contenido del mensaje serializado en JSON.
    /// </summary>
    public string MessageBody { get; set; } = string.Empty;

    /// <summary>
    /// Nombre de la cola de origen.
    /// </summary>
    public string SourceQueue { get; set; } = string.Empty;

    /// <summary>
    /// Razón por la que el mensaje fue enviado a DLQ.
    /// </summary>
    public string FailureReason { get; set; } = string.Empty;

    /// <summary>
    /// Excepción que causó el fallo (si aplica).
    /// </summary>
    public string? ExceptionDetails { get; set; }

    /// <summary>
    /// Número de intentos de procesamiento.
    /// </summary>
    public int RetryCount { get; set; }

    /// <summary>
    /// Fecha y hora del primer intento.
    /// </summary>
    public DateTime FirstAttemptAt { get; set; }

    /// <summary>
    /// Fecha y hora del último intento.
    /// </summary>
    public DateTime LastAttemptAt { get; set; }

    /// <summary>
    /// Fecha y hora en que fue enviado a DLQ.
    /// </summary>
    public DateTime SentToDeadLetterAt { get; set; }

    /// <summary>
    /// Correlation ID para rastreo.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Headers adicionales del mensaje.
    /// </summary>
    public Dictionary<string, string> Headers { get; set; } = new();

    /// <summary>
    /// Indica si el mensaje ha sido reprocesado.
    /// </summary>
    public bool IsReprocessed { get; set; }

    /// <summary>
    /// Fecha y hora del reprocesamiento (si aplica).
    /// </summary>
    public DateTime? ReprocessedAt { get; set; }
}

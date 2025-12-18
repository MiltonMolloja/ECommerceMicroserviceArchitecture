namespace Common.Messaging;

/// <summary>
/// Configuración de conexión a RabbitMQ.
/// </summary>
public class RabbitMQSettings
{
    /// <summary>
    /// Host de RabbitMQ (default: localhost).
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// Puerto de RabbitMQ (default: 5672).
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// Usuario de RabbitMQ (default: guest).
    /// </summary>
    public string Username { get; set; } = "guest";

    /// <summary>
    /// Contraseña de RabbitMQ (default: guest).
    /// </summary>
    public string Password { get; set; } = "guest";

    /// <summary>
    /// Virtual Host de RabbitMQ (default: /).
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// Habilitar o deshabilitar la mensajería (default: true).
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// Número de reintentos para mensajes fallidos (default: 3).
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Intervalo entre reintentos en segundos (default: 5).
    /// </summary>
    public int RetryIntervalSeconds { get; set; } = 5;

    /// <summary>
    /// Prefetch count para consumidores (default: 16).
    /// </summary>
    public int PrefetchCount { get; set; } = 16;

    /// <summary>
    /// Habilitar Dead Letter Queue (default: true).
    /// </summary>
    public bool EnableDeadLetterQueue { get; set; } = true;

    /// <summary>
    /// Sufijo para las colas de Dead Letter (default: _error).
    /// </summary>
    public string DeadLetterQueueSuffix { get; set; } = "_error";

    /// <summary>
    /// Tiempo de vida de mensajes en DLQ en horas (default: 168 = 7 días).
    /// </summary>
    public int DeadLetterMessageTtlHours { get; set; } = 168;

    /// <summary>
    /// Habilitar redelivery automático desde DLQ (default: false).
    /// </summary>
    public bool EnableDeadLetterRedelivery { get; set; } = false;

    /// <summary>
    /// Intervalo de redelivery desde DLQ en minutos (default: 60).
    /// </summary>
    public int DeadLetterRedeliveryIntervalMinutes { get; set; } = 60;

    /// <summary>
    /// Máximo número de redeliveries desde DLQ (default: 3).
    /// </summary>
    public int MaxDeadLetterRedeliveryCount { get; set; } = 3;

    /// <summary>
    /// Obtiene la URI de conexión a RabbitMQ.
    /// </summary>
    public string ConnectionString => $"amqp://{Username}:{Password}@{Host}:{Port}{VirtualHost}";
}

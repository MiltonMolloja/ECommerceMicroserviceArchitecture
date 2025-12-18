using MassTransit;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Common.Messaging.DeadLetter;

/// <summary>
/// Observador para capturar mensajes que van a Dead Letter Queue.
/// </summary>
public class DeadLetterObserver : IReceiveObserver
{
    private readonly ILogger<DeadLetterObserver> _logger;

    public DeadLetterObserver(ILogger<DeadLetterObserver> logger)
    {
        _logger = logger;
    }

    public Task PreReceive(ReceiveContext context)
    {
        return Task.CompletedTask;
    }

    public Task PostReceive(ReceiveContext context)
    {
        return Task.CompletedTask;
    }

    public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType) where T : class
    {
        _logger.LogDebug(
            "Message {MessageId} consumed successfully by {ConsumerType} in {Duration}ms",
            context.MessageId,
            consumerType,
            duration.TotalMilliseconds);

        return Task.CompletedTask;
    }

    public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception) where T : class
    {
        var deadLetterMessage = new DeadLetterMessage
        {
            MessageId = context.MessageId ?? Guid.NewGuid(),
            MessageType = typeof(T).FullName ?? typeof(T).Name,
            MessageBody = JsonSerializer.Serialize(context.Message),
            SourceQueue = context.ReceiveContext.InputAddress?.AbsolutePath ?? "unknown",
            FailureReason = exception.Message,
            ExceptionDetails = exception.ToString(),
            RetryCount = GetRetryCount(context),
            FirstAttemptAt = GetFirstAttemptTime(context),
            LastAttemptAt = DateTime.UtcNow,
            SentToDeadLetterAt = DateTime.UtcNow,
            CorrelationId = context.CorrelationId?.ToString()
        };

        // Agregar headers
        if (context.Headers != null)
        {
            foreach (var header in context.Headers.GetAll())
            {
                if (header.Value != null)
                {
                    deadLetterMessage.Headers[header.Key] = header.Value.ToString() ?? string.Empty;
                }
            }
        }

        _logger.LogError(
            exception,
            "Message {MessageId} of type {MessageType} failed after {RetryCount} attempts. " +
            "Consumer: {ConsumerType}. Duration: {Duration}ms. Sending to Dead Letter Queue. " +
            "CorrelationId: {CorrelationId}",
            deadLetterMessage.MessageId,
            deadLetterMessage.MessageType,
            deadLetterMessage.RetryCount,
            consumerType,
            duration.TotalMilliseconds,
            deadLetterMessage.CorrelationId);

        return Task.CompletedTask;
    }

    public Task ReceiveFault(ReceiveContext context, Exception exception)
    {
        _logger.LogError(
            exception,
            "Receive fault on {InputAddress}. MessageId: {MessageId}",
            context.InputAddress,
            context.GetMessageId());

        return Task.CompletedTask;
    }

    private static int GetRetryCount<T>(ConsumeContext<T> context) where T : class
    {
        if (context.Headers.TryGetHeader("MT-Redelivery-Count", out var value) && value != null)
        {
            if (int.TryParse(value.ToString(), out var count))
            {
                return count;
            }
        }
        return 0;
    }

    private static DateTime GetFirstAttemptTime<T>(ConsumeContext<T> context) where T : class
    {
        if (context.Headers.TryGetHeader("MT-First-Attempt-Time", out var value) && value != null)
        {
            if (DateTime.TryParse(value.ToString(), out var time))
            {
                return time;
            }
        }
        return context.SentTime ?? DateTime.UtcNow;
    }
}

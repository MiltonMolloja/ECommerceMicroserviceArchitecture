using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Messaging.Events.Payments;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento PaymentFailed para enviar notificaciones.
/// Envía email de notificación de pago fallido al cliente.
/// </summary>
public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(
        IEmailNotificationService emailService,
        ILogger<PaymentFailedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PaymentFailedEvent for notification - OrderId: {OrderId}, ClientEmail: {ClientEmail}",
            message.OrderId, message.ClientEmail);

        try
        {
            // Preparar datos para el template de email
            var emailData = new Dictionary<string, object>
            {
                { "OrderNumber", $"#ORD-{message.OrderId:D8}" },
                { "Amount", message.Amount.ToString("C2", new System.Globalization.CultureInfo("es-AR")) },
                { "ErrorMessage", message.ErrorMessage },
                { "ErrorCode", message.ErrorCode ?? "UNKNOWN" },
                { "FailedAt", message.FailedAt.ToString("dd/MM/yyyy HH:mm") }
            };

            // Enviar email de notificación de pago fallido
            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "PaymentFailed",
                emailData);

            _logger.LogInformation(
                "Payment failed email sent via RabbitMQ event for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending payment failed email for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }
}

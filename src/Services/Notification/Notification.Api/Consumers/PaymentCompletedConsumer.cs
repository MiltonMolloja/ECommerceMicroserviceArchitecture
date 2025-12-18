using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Messaging.Events.Payments;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento PaymentCompleted para enviar notificaciones.
/// Envía email de confirmación de compra al cliente.
/// </summary>
public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        IEmailNotificationService emailService,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PaymentCompletedEvent for notification - OrderId: {OrderId}, ClientEmail: {ClientEmail}",
            message.OrderId, message.ClientEmail);

        try
        {
            // Preparar datos para el template de email
            var emailData = new Dictionary<string, object>
            {
                { "CustomerName", message.ClientName },
                { "OrderNumber", $"#ORD-{message.OrderId:D8}" },
                { "Amount", message.Amount.ToString("C2", new System.Globalization.CultureInfo("es-AR")) },
                { "PaymentMethod", message.PaymentMethod },
                { "TransactionId", message.TransactionId ?? "N/A" },
                { "PaidAt", message.PaidAt.ToString("dd/MM/yyyy HH:mm") }
            };

            // Enviar email de confirmación de compra usando template
            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "OrderConfirmation",
                emailData);

            _logger.LogInformation(
                "Order confirmation email sent via RabbitMQ event for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending order confirmation email for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }
}

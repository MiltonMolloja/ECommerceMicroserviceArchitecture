using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Messaging.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento OrderCancelled para enviar email de cancelación.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        IEmailNotificationService emailService,
        ILogger<OrderCancelledConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OrderCancelledEvent - OrderId: {OrderId}, ClientEmail: {ClientEmail}",
            message.OrderId, message.ClientEmail);

        try
        {
            var emailData = new Dictionary<string, object>
            {
                { "CustomerName", message.ClientName },
                { "OrderNumber", $"#ORD-{message.OrderId:D8}" },
                { "Amount", message.Total.ToString("C2", new System.Globalization.CultureInfo("es-AR")) },
                { "CancellationReason", message.CancellationReason },
                { "CancelledAt", message.CancelledAt.ToString("dd/MM/yyyy HH:mm") }
            };

            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "OrderCancelled",
                emailData);

            _logger.LogInformation(
                "Order cancellation email sent for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending order cancellation email for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
            throw;
        }
    }
}

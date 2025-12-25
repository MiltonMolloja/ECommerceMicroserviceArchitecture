using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Messaging.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento OrderShipped para enviar notificaciones.
/// Envía email de confirmación de envío al cliente con tracking.
/// </summary>
public class OrderShippedConsumer : IConsumer<OrderShippedEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<OrderShippedConsumer> _logger;

    public OrderShippedConsumer(
        IEmailNotificationService emailService,
        ILogger<OrderShippedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderShippedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OrderShippedEvent for notification - OrderId: {OrderId}, TrackingNumber: {TrackingNumber}",
            message.OrderId, message.TrackingNumber);

        try
        {
            // Preparar datos para el template de email
            var emailData = new Dictionary<string, object>
            {
                { "CustomerName", message.ClientName },
                { "OrderNumber", $"#ORD-{message.OrderId:D8}" },
                { "TrackingNumber", message.TrackingNumber },
                { "Carrier", message.Carrier },
                { "TrackingUrl", message.TrackingUrl ?? "#" },
                { "ShippedAt", message.ShippedAt.ToString("dd/MM/yyyy HH:mm") },
                { "EstimatedDelivery", message.EstimatedDeliveryDate?.ToString("dd/MM/yyyy") ?? "A confirmar" },
                { "ShippingAddress", FormatAddress(message.ShippingAddress) }
            };

            // Enviar email de confirmación de envío usando template
            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "order-shipped",
                emailData);

            _logger.LogInformation(
                "Order shipped email sent for OrderId: {OrderId} to {ClientEmail}. Tracking: {TrackingNumber}",
                message.OrderId, message.ClientEmail, message.TrackingNumber);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending order shipped email for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }

    private static string FormatAddress(ShippingAddressInfo address)
    {
        return $"{address.Street}, {address.City}, {address.State} {address.PostalCode}, {address.Country}";
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Messaging.Events.Orders;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento OrderDelivered para enviar notificaciones.
/// Envía email de confirmación de entrega y solicita reviews de productos.
/// </summary>
public class OrderDeliveredConsumer : IConsumer<OrderDeliveredEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<OrderDeliveredConsumer> _logger;

    public OrderDeliveredConsumer(
        IEmailNotificationService emailService,
        ILogger<OrderDeliveredConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OrderDeliveredEvent for notification - OrderId: {OrderId}, DeliveredAt: {DeliveredAt}",
            message.OrderId, message.DeliveredAt);

        try
        {
            // Preparar datos para el template de email de entrega
            var emailData = new Dictionary<string, object>
            {
                { "CustomerName", message.ClientName },
                { "OrderNumber", $"#ORD-{message.OrderId:D8}" },
                { "DeliveredAt", message.DeliveredAt.ToString("dd/MM/yyyy HH:mm") },
                { "ReceivedBy", message.ReceivedBy ?? message.ClientName },
                { "DeliveryNotes", message.DeliveryNotes ?? "" },
                { "Items", message.Items.Select(i => new { i.ProductName, i.Quantity }).ToList() },
                { "ReviewUrl", $"https://ecommerce.com/orders/{message.OrderId}/review" }
            };

            // Enviar email de confirmación de entrega
            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "order-delivered",
                emailData);

            _logger.LogInformation(
                "Order delivered email sent for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);

            // Programar email de solicitud de review (se podría hacer con un delay)
            // Por ahora lo enviamos inmediatamente con el template de review
            var reviewData = new Dictionary<string, object>
            {
                { "CustomerName", message.ClientName },
                { "OrderNumber", $"#ORD-{message.OrderId:D8}" },
                { "Products", message.Items.Select(i => new 
                { 
                    i.ProductId, 
                    i.ProductName,
                    ReviewUrl = $"https://ecommerce.com/products/{i.ProductId}/review?orderId={message.OrderId}"
                }).ToList() }
            };

            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "review-request",
                reviewData);

            _logger.LogInformation(
                "Review request email sent for OrderId: {OrderId} to {ClientEmail}. Products: {ProductCount}",
                message.OrderId, message.ClientEmail, message.Items.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending order delivered email for OrderId: {OrderId} to {ClientEmail}",
                message.OrderId, message.ClientEmail);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }
}

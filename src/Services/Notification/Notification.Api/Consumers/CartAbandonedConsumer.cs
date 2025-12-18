using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Messaging.Events.Cart;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento CartAbandoned para enviar email de recuperación de carrito.
/// </summary>
public class CartAbandonedConsumer : IConsumer<CartAbandonedEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<CartAbandonedConsumer> _logger;

    public CartAbandonedConsumer(
        IEmailNotificationService emailService,
        ILogger<CartAbandonedConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CartAbandonedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing CartAbandonedEvent - CartId: {CartId}, ClientEmail: {ClientEmail}, ItemCount: {ItemCount}",
            message.CartId, message.ClientEmail, message.ItemCount);

        try
        {
            var itemsList = string.Join(", ", message.Items.Select(i => $"{i.ProductName} x{i.Quantity}"));

            var emailData = new Dictionary<string, object>
            {
                { "CustomerName", message.ClientName },
                { "CartTotal", message.Total.ToString("C2", new System.Globalization.CultureInfo("es-AR")) },
                { "ItemCount", message.ItemCount },
                { "ItemsList", itemsList },
                { "LastActivityAt", message.LastActivityAt.ToString("dd/MM/yyyy HH:mm") },
                { "CartUrl", $"https://yourstore.com/cart/{message.CartId}" } // URL para recuperar el carrito
            };

            await _emailService.SendTemplatedEmailAsync(
                message.ClientEmail,
                "CartAbandoned",
                emailData);

            _logger.LogInformation(
                "Cart abandonment email sent for CartId: {CartId} to {ClientEmail}",
                message.CartId, message.ClientEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending cart abandonment email for CartId: {CartId} to {ClientEmail}",
                message.CartId, message.ClientEmail);
            throw;
        }
    }
}

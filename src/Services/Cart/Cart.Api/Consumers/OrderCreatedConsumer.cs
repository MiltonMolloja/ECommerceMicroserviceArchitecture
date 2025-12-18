using System;
using System.Linq;
using System.Threading.Tasks;
using Cart.Persistence.Database;
using Common.Messaging.Events.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cart.Api.Consumers;

/// <summary>
/// Consumidor del evento OrderCreated para limpiar el carrito del cliente.
/// Cuando se crea una orden, se elimina el carrito del cliente.
/// </summary>
public class OrderCreatedConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderCreatedConsumer> _logger;

    public OrderCreatedConsumer(
        ApplicationDbContext context,
        ILogger<OrderCreatedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OrderCreatedEvent - Clearing cart for ClientId: {ClientId}, OrderId: {OrderId}",
            message.ClientId, message.OrderId);

        try
        {
            // Buscar el carrito del cliente
            var cart = await _context.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.ClientId == message.ClientId);

            if (cart == null)
            {
                _logger.LogWarning(
                    "No cart found for ClientId: {ClientId} after order {OrderId} was created",
                    message.ClientId, message.OrderId);
                return;
            }

            // Eliminar items del carrito
            if (cart.Items != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                _logger.LogInformation(
                    "Removed {ItemCount} items from cart for ClientId: {ClientId}",
                    cart.Items.Count, message.ClientId);
            }

            // Eliminar el carrito
            _context.ShoppingCarts.Remove(cart);
            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Cart cleared successfully for ClientId: {ClientId} after OrderId: {OrderId}",
                message.ClientId, message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error clearing cart for ClientId: {ClientId} after OrderId: {OrderId}",
                message.ClientId, message.OrderId);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }
}

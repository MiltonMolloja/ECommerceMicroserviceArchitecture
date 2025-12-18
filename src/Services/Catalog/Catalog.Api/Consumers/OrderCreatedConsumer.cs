using System;
using System.Linq;
using System.Threading.Tasks;
using Catalog.Persistence.Database;
using Common.Messaging.Events.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Consumers;

/// <summary>
/// Consumidor del evento OrderCreated para reservar stock.
/// Nota: El stock ya se descuenta en OrderCreateEventHandler via HTTP.
/// Este consumer es para logging/auditoría y posibles acciones adicionales.
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
            "Processing OrderCreatedEvent - OrderId: {OrderId}, ClientId: {ClientId}, Items: {ItemCount}",
            message.OrderId, message.ClientId, message.Items.Count);

        try
        {
            // Registrar la reserva de stock para auditoría
            foreach (var item in message.Items)
            {
                var stock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock != null)
                {
                    _logger.LogInformation(
                        "Stock reserved for ProductId: {ProductId}, Quantity: {Quantity}, RemainingStock: {RemainingStock}",
                        item.ProductId, item.Quantity, stock.Stock);
                }
                else
                {
                    _logger.LogWarning(
                        "No stock record found for ProductId: {ProductId}",
                        item.ProductId);
                }
            }

            _logger.LogInformation(
                "OrderCreatedEvent processed successfully for OrderId: {OrderId}",
                message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing OrderCreatedEvent for OrderId: {OrderId}",
                message.OrderId);
            throw;
        }
    }
}

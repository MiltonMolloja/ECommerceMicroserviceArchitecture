using System;
using System.Threading.Tasks;
using Catalog.Persistence.Database;
using Common.Messaging.Events.Orders;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Consumers;

/// <summary>
/// Consumidor del evento OrderCancelled para liberar stock reservado.
/// Cuando se cancela una orden, se devuelve el stock de los productos.
/// </summary>
public class OrderCancelledConsumer : IConsumer<OrderCancelledEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<OrderCancelledConsumer> _logger;

    public OrderCancelledConsumer(
        ApplicationDbContext context,
        ILogger<OrderCancelledConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCancelledEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing OrderCancelledEvent - Releasing stock for OrderId: {OrderId}, Reason: {Reason}",
            message.OrderId, message.CancellationReason);

        try
        {
            foreach (var item in message.Items)
            {
                var stock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductId == item.ProductId);

                if (stock != null)
                {
                    var previousStock = stock.Stock;
                    stock.Stock += item.Quantity;

                    _logger.LogInformation(
                        "Stock released for ProductId: {ProductId}, Quantity: {Quantity}, PreviousStock: {PreviousStock}, NewStock: {NewStock}",
                        item.ProductId, item.Quantity, previousStock, stock.Stock);
                }
                else
                {
                    _logger.LogWarning(
                        "No stock record found for ProductId: {ProductId}, cannot release stock",
                        item.ProductId);
                }
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Stock released successfully for OrderId: {OrderId}",
                message.OrderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error releasing stock for OrderId: {OrderId}",
                message.OrderId);
            throw;
        }
    }
}

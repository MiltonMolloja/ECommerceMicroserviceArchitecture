using System;
using System.Threading.Tasks;
using Common.Messaging.Events.Catalog;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento StockUpdated para notificar disponibilidad de productos.
/// Solo procesa cuando un producto vuelve a estar disponible (back in stock).
/// </summary>
public class StockUpdatedConsumer : IConsumer<StockUpdatedEvent>
{
    private readonly ILogger<StockUpdatedConsumer> _logger;

    public StockUpdatedConsumer(ILogger<StockUpdatedConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<StockUpdatedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing StockUpdatedEvent - ProductId: {ProductId}, PreviousStock: {PreviousStock}, CurrentStock: {CurrentStock}",
            message.ProductId, message.PreviousStock, message.CurrentStock);

        try
        {
            if (message.IsBackInStock)
            {
                _logger.LogInformation(
                    "Product {ProductId} ({ProductName}) is back in stock! Current stock: {CurrentStock}",
                    message.ProductId, message.ProductName, message.CurrentStock);

                // TODO: Implementar notificación a usuarios que tienen el producto en wishlist
                // Esto requeriría:
                // 1. Una tabla de wishlists/alertas de stock
                // 2. Consultar usuarios interesados en este producto
                // 3. Enviar emails a cada usuario
                
                await Task.CompletedTask;
            }
            else if (message.IsOutOfStock)
            {
                _logger.LogWarning(
                    "Product {ProductId} ({ProductName}) is now out of stock!",
                    message.ProductId, message.ProductName);
            }
            else
            {
                _logger.LogDebug(
                    "Stock updated for ProductId: {ProductId}, no notification needed",
                    message.ProductId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error processing StockUpdatedEvent for ProductId: {ProductId}",
                message.ProductId);
            throw;
        }
    }
}

using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.EventHandlers.Commands;
using Catalog.Service.EventHandlers.Exceptions;
using Common.Messaging.Events.Catalog;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Catalog.Common.Enums;

namespace Catalog.Service.EventHandlers
{
    public class ProductInStockUpdateStockEventHandler :
        INotificationHandler<ProductInStockUpdateStockCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProductInStockUpdateStockEventHandler> _logger;

        public ProductInStockUpdateStockEventHandler(
            ApplicationDbContext context,
            IPublishEndpoint publishEndpoint,
            ILogger<ProductInStockUpdateStockEventHandler> logger)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task Handle(ProductInStockUpdateStockCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("--- ProductInStockUpdateStockCommand started");

            var products = notification.Items.Select(x => x.ProductId);
            var stocks = await _context.Stocks.Where(x => products.Contains(x.ProductId)).ToListAsync();

            _logger.LogInformation("--- Retrieve products from database");

            foreach (var item in notification.Items) 
            {
                var entry = stocks.SingleOrDefault(x => x.ProductId == item.ProductId);

                if (item.Action == ProductInStockAction.Substract)
                {
                    if (entry == null || item.Stock > entry.Stock)
                    {
                        _logger.LogError($"--- Product {item.ProductId} -doens't have enough stock");
                        throw new ProductInStockUpdateStockCommandException($"Product {item.ProductId} - doens't have enough stock");
                    }

                    var previousStock = entry.Stock;
                    entry.Stock -= item.Stock;

                    _logger.LogInformation($"--- Product {entry.ProductId} - its stock was subtracted and its new stock is {entry.Stock}");

                    // Publicar evento de stock actualizado
                    await PublishStockUpdatedEventAsync(entry.ProductId, previousStock, entry.Stock, cancellationToken);
                }
                else
                {
                    var previousStock = entry?.Stock ?? 0;
                    
                    if (entry == null)
                    {
                        entry = new ProductInStock
                        {
                            ProductId = item.ProductId
                        };

                        _logger.LogInformation($"--- New stock record was created for {entry.ProductId} because didn't exists before");

                        await _context.AddAsync(entry);
                    }

                    _logger.LogInformation($"--- Add stock to product {entry.ProductId}");
                    entry.Stock += item.Stock;

                    // Publicar evento de stock actualizado
                    await PublishStockUpdatedEventAsync(entry.ProductId, previousStock, entry.Stock, cancellationToken);
                }
            }

            await _context.SaveChangesAsync();
        }

        private async Task PublishStockUpdatedEventAsync(int productId, int previousStock, int currentStock, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener nombre del producto
                var product = await _context.Products.FindAsync(productId);
                var productName = product?.NameSpanish ?? product?.NameEnglish ?? $"Product #{productId}";

                var stockUpdatedEvent = new StockUpdatedEvent
                {
                    ProductId = productId,
                    ProductName = productName,
                    PreviousStock = previousStock,
                    CurrentStock = currentStock,
                    UpdatedAt = DateTime.UtcNow
                };

                await _publishEndpoint.Publish(stockUpdatedEvent, cancellationToken);
                _logger.LogInformation("StockUpdatedEvent published for ProductId: {ProductId}, Previous: {Previous}, Current: {Current}", 
                    productId, previousStock, currentStock);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to publish StockUpdatedEvent for ProductId: {ProductId}", productId);
                // No fallar la actualización de stock si falla la publicación del evento
            }
        }
    }
}

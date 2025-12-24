using Common.Messaging.Events.Orders;
using MassTransit;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Domain;
using Order.Persistence.Database;
using Order.Service.EventHandlers.Commands;
using Order.Service.Proxies.Catalog;
using Order.Service.Proxies.Catalog.Commands;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Order.Service.EventHandlers
{
    public class OrderCreateEventHandler :
        IRequestHandler<OrderCreateCommand, int>
    {
        private readonly ApplicationDbContext _context;
        private readonly ICatalogProxy _catalogProxy;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<OrderCreateEventHandler> _logger;

        public OrderCreateEventHandler(
            ApplicationDbContext context,
            ICatalogProxy catalogProxy,
            IPublishEndpoint publishEndpoint,
            ILogger<OrderCreateEventHandler> logger)
        {
            _context = context;
            _catalogProxy = catalogProxy;
            _publishEndpoint = publishEndpoint;
            _logger = logger;
        }

        public async Task<int> Handle(OrderCreateCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("--- New order creation started");
            var entry = new Domain.Order();

            // Use execution strategy to support retry on failure (required for PostgreSQL with NpgsqlRetryingExecutionStrategy)
            var strategy = _context.Database.CreateExecutionStrategy();

            await strategy.ExecuteAsync(async () =>
            {
                using (var trx = await _context.Database.BeginTransactionAsync(cancellationToken)) 
                {
                    // 01. Prepare detail
                    _logger.LogInformation("--- Preparing detail");
                    PrepareDetail(entry, notification);

                    // 02. Prepare header
                    _logger.LogInformation("--- Preparing header");
                    PrepareHeader(entry, notification);

                    // 03. Create order
                    _logger.LogInformation("--- Creating order");
                    await _context.AddAsync(entry, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);

                    _logger.LogInformation($"--- Order {entry.OrderId} was created");

                    // 04. Update Stocks
                    _logger.LogInformation("--- Updating stock");
                    await _catalogProxy.UpdateStockAsync(new ProductInStockUpdateStockCommand
                    {
                        Items = notification.Items.Select(x => new ProductInStockUpdateItem
                        {
                            ProductId = x.ProductId,
                            Stock = x.Quantity,
                            Action = ProductInStockAction.Substract
                        })
                    });

                    await trx.CommitAsync(cancellationToken);
                }
            });

            // Publicar evento OrderCreated para notificar a otros servicios
            try
            {
                var orderCreatedEvent = new OrderCreatedEvent
                {
                    OrderId = entry.OrderId,
                    ClientId = entry.ClientId,
                    ClientEmail = string.Empty, // TODO: Obtener del servicio de Customer
                    ClientName = notification.ShippingRecipientName ?? string.Empty,
                    Total = entry.Total,
                    Items = notification.Items.Select(x => new OrderItemInfo
                    {
                        ProductId = x.ProductId,
                        ProductName = string.Empty, // TODO: Obtener del catálogo si es necesario
                        Quantity = x.Quantity,
                        UnitPrice = x.Price
                    }).ToList()
                };

                await _publishEndpoint.Publish(orderCreatedEvent);
                _logger.LogInformation("OrderCreatedEvent published for OrderId: {OrderId}", entry.OrderId);
            }
            catch (Exception ex)
            {
                // No fallar la creación de orden si falla la publicación del evento
                _logger.LogWarning(ex, "Failed to publish OrderCreatedEvent for OrderId: {OrderId}", entry.OrderId);
            }

            _logger.LogInformation("--- New order creation ended");
            return entry.OrderId;
        }

        private void PrepareDetail(Domain.Order entry, OrderCreateCommand notification) 
        {
            entry.Items = notification.Items.Select(x => new OrderDetail
            {
                ProductId = x.ProductId,
                ProductName = $"Product #{x.ProductId}", // TODO: Get from catalog if needed
                Quantity = x.Quantity,
                UnitPrice = x.Price,
                Total = x.Price * x.Quantity
            }).ToList();
        }

        private void PrepareHeader(Domain.Order entry, OrderCreateCommand notification)
        {
            // Header information
            entry.Status = Common.Enums.OrderStatus.AwaitingPayment;  // Nuevo: Orden creada esperando pago
            entry.PaymentType = notification.PaymentType;
            entry.ClientId = notification.ClientId.Value; // ClientId is guaranteed to have a value by the controller
            entry.CreatedAt = DateTime.UtcNow;
            entry.OrderDate = DateTime.UtcNow; // Required by PostgreSQL schema
            entry.UpdatedAt = DateTime.UtcNow;

            // Shipping Address
            entry.ShippingRecipientName = notification.ShippingRecipientName;
            entry.ShippingPhone = notification.ShippingPhone;
            entry.ShippingAddressLine1 = notification.ShippingAddressLine1;
            entry.ShippingAddressLine2 = notification.ShippingAddressLine2;
            entry.ShippingCity = notification.ShippingCity;
            entry.ShippingState = notification.ShippingState;
            entry.ShippingPostalCode = notification.ShippingPostalCode;
            entry.ShippingCountry = notification.ShippingCountry;
            
            // Legacy ShippingAddress field (for PostgreSQL compatibility)
            entry.ShippingAddress = $"{notification.ShippingAddressLine1}, {notification.ShippingCity}, {notification.ShippingCountry}";

            // Billing Address
            entry.BillingAddressLine1 = notification.BillingSameAsShipping
                ? notification.ShippingAddressLine1
                : notification.BillingAddressLine1;
            entry.BillingCity = notification.BillingSameAsShipping
                ? notification.ShippingCity
                : notification.BillingCity;
            entry.BillingPostalCode = notification.BillingSameAsShipping
                ? notification.ShippingPostalCode
                : notification.BillingPostalCode;
            entry.BillingCountry = notification.BillingSameAsShipping
                ? notification.ShippingCountry
                : notification.BillingCountry;
            entry.BillingSameAsShipping = notification.BillingSameAsShipping;

            // Financial calculations (required by PostgreSQL schema)
            entry.SubTotal = entry.Items.Sum(x => x.Total);
            entry.Tax = 0; // TODO: Calculate tax if needed
            entry.Discount = 0; // TODO: Apply discounts if needed
            entry.Total = entry.SubTotal + entry.Tax - entry.Discount;
        }
    }
}

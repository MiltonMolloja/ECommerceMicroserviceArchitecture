using MediatR;
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
        private readonly ILogger<OrderCreateEventHandler> _logger;

        public OrderCreateEventHandler(
            ApplicationDbContext context,
            ICatalogProxy catalogProxy,
            ILogger<OrderCreateEventHandler> logger)
        {
            _context = context;
            _catalogProxy = catalogProxy;
            _logger = logger;
        }

        public async Task<int> Handle(OrderCreateCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("--- New order creation started");
            var entry = new Domain.Order();

            using (var trx = await _context.Database.BeginTransactionAsync()) 
            {
                // 01. Prepare detail
                _logger.LogInformation("--- Preparing detail");
                PrepareDetail(entry, notification);

                // 02. Prepare header
                _logger.LogInformation("--- Preparing header");
                PrepareHeader(entry, notification);

                // 03. Create order
                _logger.LogInformation("--- Creating order");
                await _context.AddAsync(entry);
                await _context.SaveChangesAsync();

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

                await trx.CommitAsync();
            }

            _logger.LogInformation("--- New order creation ended");
            return entry.OrderId;
        }

        private void PrepareDetail(Domain.Order entry, OrderCreateCommand notification) 
        {
            entry.Items = notification.Items.Select(x => new OrderDetail
            {
                ProductId = x.ProductId,
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

            // Shipping Address
            entry.ShippingRecipientName = notification.ShippingRecipientName;
            entry.ShippingPhone = notification.ShippingPhone;
            entry.ShippingAddressLine1 = notification.ShippingAddressLine1;
            entry.ShippingAddressLine2 = notification.ShippingAddressLine2;
            entry.ShippingCity = notification.ShippingCity;
            entry.ShippingState = notification.ShippingState;
            entry.ShippingPostalCode = notification.ShippingPostalCode;
            entry.ShippingCountry = notification.ShippingCountry;

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

            // Sum
            entry.Total = entry.Items.Sum(x => x.Total);
        }
    }
}

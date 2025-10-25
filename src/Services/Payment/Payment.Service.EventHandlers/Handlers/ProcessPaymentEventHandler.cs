using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Payment.Persistence.Database;
using Payment.Service.EventHandlers.Commands;
using Payment.Service.Gateways;
using Payment.Service.Proxies.Order;
using Payment.Service.Proxies.Notification;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.Service.EventHandlers.Handlers
{
    public class ProcessPaymentEventHandler : INotificationHandler<ProcessPaymentCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly IOrderProxy _orderProxy;
        private readonly INotificationProxy _notificationProxy;
        private readonly ILogger<ProcessPaymentEventHandler> _logger;

        public ProcessPaymentEventHandler(
            ApplicationDbContext context,
            IPaymentGatewayFactory gatewayFactory,
            IOrderProxy orderProxy,
            INotificationProxy notificationProxy,
            ILogger<ProcessPaymentEventHandler> logger)
        {
            _context = context;
            _gatewayFactory = gatewayFactory;
            _orderProxy = orderProxy;
            _notificationProxy = notificationProxy;
            _logger = logger;
        }

        public async Task Handle(ProcessPaymentCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"--- Processing payment for order {notification.OrderId}");

            try
            {
                // 1. Validar que la orden existe
                var order = await _orderProxy.GetOrderByIdAsync(notification.OrderId);
                if (order == null)
                {
                    _logger.LogWarning($"Order {notification.OrderId} not found");
                    throw new InvalidOperationException("Order not found");
                }

                // 2. Crear registro de pago
                var payment = new Domain.Payment
                {
                    OrderId = notification.OrderId,
                    UserId = notification.UserId,
                    Amount = notification.Amount,
                    Currency = notification.Currency,
                    Status = Domain.PaymentStatus.Processing,
                    PaymentMethod = notification.PaymentMethod,
                    TransactionId = string.Empty, // Se actualizará después de procesar
                    PaymentGateway = notification.PaymentMethod == Domain.PaymentMethod.CreditCard ||
                                     notification.PaymentMethod == Domain.PaymentMethod.DebitCard
                                     ? "Stripe" : notification.PaymentMethod.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.Payments.AddAsync(payment, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                // 3. Procesar pago según método
                var gateway = _gatewayFactory.GetGateway(notification.PaymentMethod);
                var result = await gateway.ProcessPaymentAsync(new PaymentRequest
                {
                    Amount = notification.Amount,
                    Currency = notification.Currency,
                    PaymentToken = notification.PaymentToken,
                    Description = $"Order {notification.OrderId}"
                });

                // 4. Actualizar estado del pago
                if (result.Success)
                {
                    payment.MarkAsCompleted(result.TransactionId);
                    payment.PaymentGateway = result.Gateway;

                    // Guardar detalles del pago
                    payment.PaymentDetail = new Domain.PaymentDetail
                    {
                        CardLast4Digits = result.CardLast4,
                        CardBrand = result.CardBrand,
                        BillingAddress = notification.BillingAddress,
                        BillingCity = notification.BillingCity,
                        BillingCountry = notification.BillingCountry,
                        BillingZipCode = notification.BillingZipCode
                    };

                    // Notificar al OrderService
                    await _orderProxy.UpdateOrderPaymentStatusAsync(notification.OrderId, "Paid");

                    // Enviar notificación al usuario
                    await _notificationProxy.SendPaymentConfirmationAsync(notification.UserId, payment.PaymentId);

                    _logger.LogInformation($"Payment {payment.PaymentId} completed successfully");
                }
                else
                {
                    payment.MarkAsFailed(result.ErrorMessage);

                    // Notificar al usuario del fallo
                    await _notificationProxy.SendPaymentFailedAsync(notification.UserId, payment.PaymentId, result.ErrorMessage);

                    _logger.LogWarning($"Payment failed: {result.ErrorMessage}");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing payment for order {notification.OrderId}");
                throw;
            }
        }
    }
}

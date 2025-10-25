using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Payment.Persistence.Database;
using Payment.Service.EventHandlers.Commands;
using Payment.Service.Gateways;
using Payment.Service.Proxies.Notification;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Payment.Service.EventHandlers.Handlers
{
    public class ProcessRefundEventHandler : INotificationHandler<ProcessRefundCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly IPaymentGatewayFactory _gatewayFactory;
        private readonly INotificationProxy _notificationProxy;
        private readonly ILogger<ProcessRefundEventHandler> _logger;

        public ProcessRefundEventHandler(
            ApplicationDbContext context,
            IPaymentGatewayFactory gatewayFactory,
            INotificationProxy notificationProxy,
            ILogger<ProcessRefundEventHandler> logger)
        {
            _context = context;
            _gatewayFactory = gatewayFactory;
            _notificationProxy = notificationProxy;
            _logger = logger;
        }

        public async Task Handle(ProcessRefundCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"--- Processing refund for payment {notification.PaymentId}");

            try
            {
                // 1. Obtener el pago
                var payment = await _context.Payments
                    .Include(p => p.Transactions)
                    .FirstOrDefaultAsync(p => p.PaymentId == notification.PaymentId, cancellationToken);

                if (payment == null)
                {
                    _logger.LogWarning($"Payment {notification.PaymentId} not found");
                    throw new InvalidOperationException("Payment not found");
                }

                // 2. Validar que puede ser reembolsado
                if (!payment.CanBeRefunded)
                {
                    _logger.LogWarning($"Payment {notification.PaymentId} cannot be refunded");
                    throw new InvalidOperationException("Payment cannot be refunded");
                }

                // 3. Procesar reembolso con la pasarela
                var gateway = _gatewayFactory.GetGateway(payment.PaymentMethod);
                var refundAmount = notification.Amount ?? payment.Amount;

                var result = await gateway.ProcessRefundAsync(new RefundRequest
                {
                    TransactionId = payment.TransactionId,
                    Amount = refundAmount,
                    Reason = notification.Reason
                });

                // 4. Actualizar estado del pago
                if (result.Success)
                {
                    payment.MarkAsRefunded(result.RefundId);

                    // Notificar al usuario
                    await _notificationProxy.SendRefundProcessedAsync(payment.UserId, payment.PaymentId);

                    _logger.LogInformation($"Refund processed successfully for payment {notification.PaymentId}");
                }
                else
                {
                    _logger.LogError($"Refund failed for payment {notification.PaymentId}: {result.ErrorMessage}");
                    throw new InvalidOperationException($"Refund failed: {result.ErrorMessage}");
                }

                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for payment {notification.PaymentId}");
                throw;
            }
        }
    }
}

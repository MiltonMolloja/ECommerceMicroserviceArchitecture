using MediatR;
using Microsoft.Extensions.Logging;
using Order.Domain;
using Order.Persistence.Database;
using Order.Service.EventHandlers.Commands;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static Order.Common.Enums;

namespace Order.Service.EventHandlers.Handlers
{
    public class UpdateOrderStatusEventHandler : INotificationHandler<UpdateOrderStatusCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdateOrderStatusEventHandler> _logger;

        public UpdateOrderStatusEventHandler(
            ApplicationDbContext context,
            ILogger<UpdateOrderStatusEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(UpdateOrderStatusCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Updating order {notification.OrderId} status to {notification.NewStatus}");

            var order = await _context.Orders.FindAsync(notification.OrderId);
            if (order == null)
            {
                _logger.LogError($"Order {notification.OrderId} not found");
                throw new Exception($"Order {notification.OrderId} not found");
            }

            // Validar transición de estado
            if (!IsValidStateTransition(order.Status, notification.NewStatus))
            {
                _logger.LogWarning($"Invalid state transition from {order.Status} to {notification.NewStatus} for order {notification.OrderId}");
                throw new Exception($"Cannot transition from {order.Status} to {notification.NewStatus}");
            }

            // Actualizar estado
            order.Status = notification.NewStatus;
            order.UpdatedAt = DateTime.UtcNow;

            // Actualizar timestamps según el nuevo estado
            switch (notification.NewStatus)
            {
                case OrderStatus.Paid:
                    order.PaidAt = DateTime.UtcNow;
                    order.PaymentTransactionId = notification.PaymentTransactionId;
                    order.PaymentGateway = notification.PaymentGateway;
                    break;
                case OrderStatus.Shipped:
                    order.ShippedAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Delivered:
                    order.DeliveredAt = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    order.CancelledAt = DateTime.UtcNow;
                    order.CancellationReason = notification.Reason;
                    break;
            }

            await _context.SaveChangesAsync(cancellationToken);
            _logger.LogInformation($"Order {notification.OrderId} status updated to {notification.NewStatus}");
        }

        private bool IsValidStateTransition(OrderStatus currentStatus, OrderStatus newStatus)
        {
            // Definir transiciones válidas
            var validTransitions = new Dictionary<OrderStatus, List<OrderStatus>>
            {
                // AwaitingPayment puede ir directo a Paid (pagos instantáneos como MercadoPago)
                { OrderStatus.AwaitingPayment, new List<OrderStatus> { OrderStatus.PaymentProcessing, OrderStatus.Paid, OrderStatus.PaymentFailed, OrderStatus.Cancelled } },
                { OrderStatus.PaymentProcessing, new List<OrderStatus> { OrderStatus.Paid, OrderStatus.PaymentFailed, OrderStatus.Cancelled } },
                { OrderStatus.PaymentFailed, new List<OrderStatus> { OrderStatus.PaymentProcessing, OrderStatus.Cancelled } },
                { OrderStatus.Paid, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Refunded, OrderStatus.Cancelled } },
                { OrderStatus.Processing, new List<OrderStatus> { OrderStatus.ReadyToShip, OrderStatus.OnHold, OrderStatus.Cancelled } },
                { OrderStatus.ReadyToShip, new List<OrderStatus> { OrderStatus.Shipped, OrderStatus.OnHold } },
                { OrderStatus.Shipped, new List<OrderStatus> { OrderStatus.InTransit, OrderStatus.Delivered, OrderStatus.ReturnRequested } },
                { OrderStatus.InTransit, new List<OrderStatus> { OrderStatus.OutForDelivery, OrderStatus.Delivered, OrderStatus.ReturnRequested } },
                { OrderStatus.OutForDelivery, new List<OrderStatus> { OrderStatus.Delivered, OrderStatus.ReturnRequested } },
                { OrderStatus.Delivered, new List<OrderStatus> { OrderStatus.ReturnRequested } },
                { OrderStatus.OnHold, new List<OrderStatus> { OrderStatus.Processing, OrderStatus.Cancelled } },
                { OrderStatus.ReturnRequested, new List<OrderStatus> { OrderStatus.Returned, OrderStatus.Cancelled } },
                { OrderStatus.PaymentDisputed, new List<OrderStatus> { OrderStatus.Refunded, OrderStatus.Cancelled } }
            };

            // Si no hay transiciones definidas para el estado actual, permitir el cambio (para flexibilidad)
            if (!validTransitions.ContainsKey(currentStatus))
            {
                _logger.LogWarning($"No transition rules defined for {currentStatus}, allowing transition to {newStatus}");
                return true;
            }

            return validTransitions[currentStatus].Contains(newStatus);
        }
    }
}

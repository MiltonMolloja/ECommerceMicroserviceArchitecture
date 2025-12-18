using System;
using System.Threading.Tasks;
using Common.Messaging.Events.Payments;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Order.Common;
using Order.Persistence.Database;

namespace Order.Api.Consumers;

/// <summary>
/// Consumidor del evento PaymentCompleted.
/// Actualiza el estado de la orden a "Paid" cuando el pago se completa exitosamente.
/// </summary>
public class PaymentCompletedConsumer : IConsumer<PaymentCompletedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        ApplicationDbContext context,
        ILogger<PaymentCompletedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentCompletedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PaymentCompletedEvent - OrderId: {OrderId}, PaymentId: {PaymentId}, Amount: {Amount}",
            message.OrderId, message.PaymentId, message.Amount);

        try
        {
            // Buscar la orden
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == message.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for PaymentCompletedEvent", message.OrderId);
                return;
            }

            // Actualizar estado de la orden
            order.Status = Common.Enums.OrderStatus.Paid;
            order.PaymentTransactionId = message.TransactionId;
            order.PaymentGateway = message.PaymentMethod;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} status updated to Paid via RabbitMQ event. TransactionId: {TransactionId}",
                message.OrderId, message.TransactionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentCompletedEvent for OrderId: {OrderId}", message.OrderId);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }
}

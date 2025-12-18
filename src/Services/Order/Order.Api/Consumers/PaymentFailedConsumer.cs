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
/// Consumidor del evento PaymentFailed.
/// Actualiza el estado de la orden a "PaymentFailed" cuando el pago falla.
/// </summary>
public class PaymentFailedConsumer : IConsumer<PaymentFailedEvent>
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PaymentFailedConsumer> _logger;

    public PaymentFailedConsumer(
        ApplicationDbContext context,
        ILogger<PaymentFailedConsumer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<PaymentFailedEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing PaymentFailedEvent - OrderId: {OrderId}, ErrorCode: {ErrorCode}, ErrorMessage: {ErrorMessage}",
            message.OrderId, message.ErrorCode, message.ErrorMessage);

        try
        {
            // Buscar la orden
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == message.OrderId);

            if (order == null)
            {
                _logger.LogWarning("Order {OrderId} not found for PaymentFailedEvent", message.OrderId);
                return;
            }

            // Actualizar estado de la orden
            order.Status = Common.Enums.OrderStatus.PaymentFailed;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation(
                "Order {OrderId} status updated to PaymentFailed via RabbitMQ event. Error: {ErrorMessage}",
                message.OrderId, message.ErrorMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing PaymentFailedEvent for OrderId: {OrderId}", message.OrderId);
            throw; // Re-throw para que MassTransit maneje el retry
        }
    }
}

using Common.Messaging.Events.Payments;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Order.Api.Consumers;
using Order.Tests.Config;
using static Order.Common.Enums;

namespace Order.Tests;

[TestClass]
public class PaymentCompletedConsumerTest
{
    private Mock<ILogger<PaymentCompletedConsumer>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PaymentCompletedConsumer>>();
    }

    [TestMethod]
    public async Task Should_UpdateOrderStatus_ToPaid_When_PaymentCompleted()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        
        // Create a pending order
        var order = new Domain.Order
        {
            ClientId = 1,
            Status = OrderStatus.AwaitingPayment,
            Total = 100,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var consumer = new PaymentCompletedConsumer(context, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 1,
            OrderId = order.OrderId,
            ClientId = 1,
            Amount = 100,
            PaymentMethod = "CreditCard",
            TransactionId = "TXN123",
            PaidAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.Paid);
        updatedOrder.PaymentTransactionId.Should().Be("TXN123");
        updatedOrder.PaymentGateway.Should().Be("CreditCard");
    }

    [TestMethod]
    public async Task Should_NotFail_When_OrderNotFound()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var consumer = new PaymentCompletedConsumer(context, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 1,
            OrderId = 999, // Non-existent order
            ClientId = 1,
            Amount = 100,
            PaymentMethod = "CreditCard",
            TransactionId = "TXN123",
            PaidAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        Func<Task> act = async () => await consumer.Consume(consumeContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [TestMethod]
    public async Task Should_UpdateOrder_EvenWhenAlreadyPaid()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        
        var order = new Domain.Order
        {
            ClientId = 1,
            Status = OrderStatus.Paid, // Already paid
            Total = 100,
            CreatedAt = DateTime.UtcNow,
            PaymentTransactionId = "OLD_TXN"
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var consumer = new PaymentCompletedConsumer(context, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 2,
            OrderId = order.OrderId,
            ClientId = 1,
            Amount = 100,
            PaymentMethod = "MercadoPago",
            TransactionId = "NEW_TXN",
            PaidAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.OrderId);
        updatedOrder!.Status.Should().Be(OrderStatus.Paid);
        updatedOrder.PaymentTransactionId.Should().Be("NEW_TXN");
    }
}

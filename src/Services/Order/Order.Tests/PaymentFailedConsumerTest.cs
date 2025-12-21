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
public class PaymentFailedConsumerTest
{
    private Mock<ILogger<PaymentFailedConsumer>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<PaymentFailedConsumer>>();
    }

    [TestMethod]
    public async Task Should_UpdateOrderStatus_ToPaymentFailed_When_PaymentFails()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        
        var order = new Domain.Order
        {
            ClientId = 1,
            Status = OrderStatus.AwaitingPayment,
            Total = 100,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var consumer = new PaymentFailedConsumer(context, _loggerMock.Object);

        var paymentEvent = new PaymentFailedEvent
        {
            PaymentId = 1,
            OrderId = order.OrderId,
            ClientId = 1,
            Amount = 100,
            ErrorMessage = "Insufficient funds",
            ErrorCode = "INSUFFICIENT_FUNDS",
            FailedAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentFailedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.OrderId);
        updatedOrder.Should().NotBeNull();
        updatedOrder!.Status.Should().Be(OrderStatus.PaymentFailed);
    }

    [TestMethod]
    public async Task Should_HandleMultiplePaymentFailures()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        
        var order = new Domain.Order
        {
            ClientId = 1,
            Status = OrderStatus.AwaitingPayment,
            Total = 100,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var consumer = new PaymentFailedConsumer(context, _loggerMock.Object);

        var paymentEvent1 = new PaymentFailedEvent
        {
            PaymentId = 1,
            OrderId = order.OrderId,
            ClientId = 1,
            Amount = 100,
            ErrorMessage = "Card declined",
            ErrorCode = "CARD_DECLINED",
            FailedAt = DateTime.UtcNow
        };

        var paymentEvent2 = new PaymentFailedEvent
        {
            PaymentId = 2,
            OrderId = order.OrderId,
            ClientId = 1,
            Amount = 100,
            ErrorMessage = "Expired card",
            ErrorCode = "EXPIRED_CARD",
            FailedAt = DateTime.UtcNow.AddMinutes(5)
        };

        var consumeContextMock1 = new Mock<ConsumeContext<PaymentFailedEvent>>();
        consumeContextMock1.Setup(x => x.Message).Returns(paymentEvent1);

        var consumeContextMock2 = new Mock<ConsumeContext<PaymentFailedEvent>>();
        consumeContextMock2.Setup(x => x.Message).Returns(paymentEvent2);

        // Act
        await consumer.Consume(consumeContextMock1.Object);
        await consumer.Consume(consumeContextMock2.Object);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(order.OrderId);
        updatedOrder!.Status.Should().Be(OrderStatus.PaymentFailed);
    }

    [TestMethod]
    public async Task Should_NotFail_When_OrderNotFound()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var consumer = new PaymentFailedConsumer(context, _loggerMock.Object);

        var paymentEvent = new PaymentFailedEvent
        {
            PaymentId = 1,
            OrderId = 999, // Non-existent order
            ClientId = 1,
            Amount = 100,
            ErrorMessage = "Test failure",
            ErrorCode = "TEST",
            FailedAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentFailedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        Func<Task> act = async () => await consumer.Consume(consumeContextMock.Object);

        // Assert
        await act.Should().NotThrowAsync();
    }
}

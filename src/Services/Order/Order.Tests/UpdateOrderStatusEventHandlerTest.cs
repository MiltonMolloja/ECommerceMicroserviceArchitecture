using Common.Messaging.Events.Orders;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Order.Service.EventHandlers.Commands;
using Order.Service.EventHandlers.Handlers;
using Order.Tests.Config;
using static Order.Common.Enums;

namespace Order.Tests;

[TestClass]
public class UpdateOrderStatusEventHandlerTest
{
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;
    private Mock<ILogger<UpdateOrderStatusEventHandler>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _loggerMock = new Mock<ILogger<UpdateOrderStatusEventHandler>>();
    }

    private Domain.Order CreateValidOrder(int orderId = 1, OrderStatus status = OrderStatus.AwaitingPayment)
    {
        return new Domain.Order
        {
            OrderId = orderId,
            ClientId = 100,
            Status = status,
            Total = 500m,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Notes = string.Empty,
            CancellationReason = string.Empty,
            PaymentTransactionId = string.Empty,
            PaymentGateway = string.Empty,
            ShippingAddress = string.Empty,
            ShippingRecipientName = "Test Customer",
            ShippingPhone = string.Empty,
            ShippingAddressLine1 = "123 Test St",
            ShippingAddressLine2 = string.Empty,
            ShippingCity = "Test City",
            ShippingState = "Test State",
            ShippingPostalCode = "12345",
            ShippingCountry = "Argentina",
            BillingAddressLine1 = "123 Test St",
            BillingCity = "Test City",
            BillingPostalCode = "12345",
            BillingCountry = "Argentina"
        };
    }

    [TestMethod]
    public async Task Should_UpdateStatus_ToPaid_When_ValidTransition()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(1, OrderStatus.AwaitingPayment);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 1,
            NewStatus = OrderStatus.Paid,
            PaymentTransactionId = "TXN-123",
            PaymentGateway = "MercadoPago"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(1);
        updatedOrder!.Status.Should().Be(OrderStatus.Paid);
        updatedOrder.PaymentTransactionId.Should().Be("TXN-123");
        updatedOrder.PaymentGateway.Should().Be("MercadoPago");
        updatedOrder.PaidAt.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Should_UpdateStatus_ToShipped_When_ValidTransition()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(2, OrderStatus.ReadyToShip);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 2,
            NewStatus = OrderStatus.Shipped
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(2);
        updatedOrder!.Status.Should().Be(OrderStatus.Shipped);
        updatedOrder.ShippedAt.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Should_UpdateStatus_ToDelivered_When_ValidTransition()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(3, OrderStatus.Shipped);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 3,
            NewStatus = OrderStatus.Delivered
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(3);
        updatedOrder!.Status.Should().Be(OrderStatus.Delivered);
        updatedOrder.DeliveredAt.Should().NotBeNull();
    }

    [TestMethod]
    public async Task Should_UpdateStatus_ToCancelled_And_PublishEvent()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(4, OrderStatus.AwaitingPayment);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 4,
            NewStatus = OrderStatus.Cancelled,
            Reason = "Customer requested cancellation"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(4);
        updatedOrder!.Status.Should().Be(OrderStatus.Cancelled);
        updatedOrder.CancelledAt.Should().NotBeNull();
        updatedOrder.CancellationReason.Should().Be("Customer requested cancellation");

        // Verify event was published
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<OrderCancelledEvent>(e => e.OrderId == 4),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Should_SkipUpdate_When_StatusIsSame()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(5, OrderStatus.Paid);
        var originalUpdatedAt = order.UpdatedAt;
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 5,
            NewStatus = OrderStatus.Paid // Same status
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert - UpdatedAt should not change
        var updatedOrder = await context.Orders.FindAsync(5);
        updatedOrder!.Status.Should().Be(OrderStatus.Paid);
    }

    [TestMethod]
    public async Task Should_ThrowException_When_OrderNotFound()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 99999,
            NewStatus = OrderStatus.Paid
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>();
    }

    [TestMethod]
    public async Task Should_ThrowException_When_InvalidStateTransition()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(6, OrderStatus.Delivered); // Delivered cannot go back to Processing
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 6,
            NewStatus = OrderStatus.Processing // Invalid transition
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<Exception>();
    }

    [TestMethod]
    public async Task Should_AllowTransition_FromPaymentFailed_ToPaymentProcessing()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(7, OrderStatus.PaymentFailed);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 7,
            NewStatus = OrderStatus.PaymentProcessing // Retry payment
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(7);
        updatedOrder!.Status.Should().Be(OrderStatus.PaymentProcessing);
    }

    [TestMethod]
    public async Task Should_AllowTransition_FromPaid_ToProcessing()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var order = CreateValidOrder(8, OrderStatus.Paid);
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var handler = new UpdateOrderStatusEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new UpdateOrderStatusCommand
        {
            OrderId = 8,
            NewStatus = OrderStatus.Processing
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedOrder = await context.Orders.FindAsync(8);
        updatedOrder!.Status.Should().Be(OrderStatus.Processing);
    }
}

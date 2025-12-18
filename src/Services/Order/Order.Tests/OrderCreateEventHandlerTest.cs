using Common.Messaging.Events.Orders;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Order.Domain;
using Order.Service.EventHandlers;
using Order.Service.EventHandlers.Commands;
using Order.Tests.Config;

namespace Order.Tests;

[TestClass]
public class OrderCreateEventHandlerTest
{
    private Mock<ILogger<OrderCreateEventHandler>> _loggerMock = null!;
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<OrderCreateEventHandler>>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
    }

    [TestMethod]
    public async Task Should_CreateOrder_When_ValidData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            Items = new List<OrderCreateCommand.OrderItemDto>
            {
                new() { ProductId = 1, ProductName = "Product 1", Quantity = 2, UnitPrice = 100 },
                new() { ProductId = 2, ProductName = "Product 2", Quantity = 1, UnitPrice = 50 }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.OrderId.Should().BeGreaterThan(0);
        result.Total.Should().Be(250); // (2 * 100) + (1 * 50)

        var orderInDb = await context.Orders.FindAsync(result.OrderId);
        orderInDb.Should().NotBeNull();
        orderInDb!.ClientId.Should().Be(1);
        orderInDb.Status.Should().Be(OrderStatus.Pending);
        orderInDb.Items.Should().HaveCount(2);

        // Verify OrderCreatedEvent was published
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<OrderCreatedEvent>(e => 
                e.OrderId == result.OrderId && 
                e.ClientId == 1 &&
                e.Total == 250),
            It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [TestMethod]
    public async Task Should_CalculateTotals_Correctly()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            Items = new List<OrderCreateCommand.OrderItemDto>
            {
                new() { ProductId = 1, ProductName = "Product 1", Quantity = 3, UnitPrice = 99.99m },
                new() { ProductId = 2, ProductName = "Product 2", Quantity = 2, UnitPrice = 149.50m }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var expectedTotal = (3 * 99.99m) + (2 * 149.50m); // 299.97 + 299.00 = 598.97
        result.Total.Should().Be(expectedTotal);
    }

    [TestMethod]
    public async Task Should_CreateOrderWithSingleItem()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 5,
            Items = new List<OrderCreateCommand.OrderItemDto>
            {
                new() { ProductId = 10, ProductName = "Single Product", Quantity = 1, UnitPrice = 199.99m }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(199.99m);
        
        var orderInDb = await context.Orders.FindAsync(result.OrderId);
        orderInDb!.Items.Should().HaveCount(1);
        orderInDb.Items.First().Quantity.Should().Be(1);
    }

    [TestMethod]
    public async Task Should_SetOrderStatus_ToPending()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            Items = new List<OrderCreateCommand.OrderItemDto>
            {
                new() { ProductId = 1, ProductName = "Product", Quantity = 1, UnitPrice = 100 }
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var orderInDb = await context.Orders.FindAsync(result.OrderId);
        orderInDb!.Status.Should().Be(OrderStatus.Pending);
    }

    [TestMethod]
    public async Task Should_PublishOrderCreatedEvent_WithCorrectData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(context, _publishEndpointMock.Object, _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 123,
            Items = new List<OrderCreateCommand.OrderItemDto>
            {
                new() { ProductId = 1, ProductName = "Product A", Quantity = 2, UnitPrice = 50 },
                new() { ProductId = 2, ProductName = "Product B", Quantity = 1, UnitPrice = 100 }
            }
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<OrderCreatedEvent>(e =>
                e.ClientId == 123 &&
                e.Total == 200 &&
                e.Items.Count == 2 &&
                e.Items.Any(i => i.ProductId == 1 && i.Quantity == 2) &&
                e.Items.Any(i => i.ProductId == 2 && i.Quantity == 1)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }
}

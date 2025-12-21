using Cart.Api.Consumers;
using Cart.Domain;
using Cart.Tests.Config;
using Common.Messaging.Events.Orders;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Cart.Tests;

/// <summary>
/// Tests para el consumer OrderCreatedConsumer que limpia el carrito cuando se crea una orden.
/// </summary>
[TestClass]
public class OrderCreatedConsumerTest
{
    private Mock<ILogger<OrderCreatedConsumer>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<OrderCreatedConsumer>>();
    }

    [TestMethod]
    public async Task Should_ClearCart_When_OrderCreated()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var clientId = 1;
        var orderId = 100;

        // Create a cart with items
        var cart = new ShoppingCart
        {
            ClientId = clientId,
            SessionId = string.Empty,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    ProductSKU = "SKU-001",
                    ProductImageUrl = "http://example.com/img.jpg",
                    Quantity = 2,
                    UnitPrice = 100m,
                    TaxRate = 21m,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                },
                new CartItem
                {
                    ProductId = 2,
                    ProductName = "Product 2",
                    ProductSKU = "SKU-002",
                    ProductImageUrl = "http://example.com/img2.jpg",
                    Quantity = 1,
                    UnitPrice = 50m,
                    TaxRate = 21m,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var consumer = new OrderCreatedConsumer(context, _loggerMock.Object);

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = orderId,
            ClientId = clientId,
            Total = 250m,
            Items = new List<OrderItemInfo>
            {
                new() { ProductId = 1, Quantity = 2, UnitPrice = 100m },
                new() { ProductId = 2, Quantity = 1, UnitPrice = 50m }
            }
        };

        var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        var cartInDb = context.ShoppingCarts.FirstOrDefault(c => c.ClientId == clientId);
        cartInDb.Should().BeNull("Cart should be removed after order is created");

        var itemsInDb = context.CartItems.Where(i => i.CartId == cart.CartId).ToList();
        itemsInDb.Should().BeEmpty("Cart items should be removed after order is created");
    }

    [TestMethod]
    public async Task Should_NotFail_When_CartNotFound()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var consumer = new OrderCreatedConsumer(context, _loggerMock.Object);

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = 100,
            ClientId = 999, // Non-existent client
            Total = 100m,
            Items = new List<OrderItemInfo>()
        };

        var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);

        // Act & Assert - Should not throw
        await consumer.Consume(consumeContextMock.Object);

        // Verify warning was logged
        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("No cart found")),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Should_ClearCart_WithEmptyItems()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var clientId = 2;

        // Create a cart without items
        var cart = new ShoppingCart
        {
            ClientId = clientId,
            SessionId = string.Empty,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<CartItem>()
        };

        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var consumer = new OrderCreatedConsumer(context, _loggerMock.Object);

        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = 200,
            ClientId = clientId,
            Total = 0m,
            Items = new List<OrderItemInfo>()
        };

        var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        var cartInDb = context.ShoppingCarts.FirstOrDefault(c => c.ClientId == clientId);
        cartInDb.Should().BeNull("Empty cart should also be removed");
    }

    [TestMethod]
    public async Task Should_OnlyRemoveCartForSpecificClient()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var clientId1 = 10;
        var clientId2 = 20;

        // Create carts for two different clients
        var cart1 = new ShoppingCart
        {
            ClientId = clientId1,
            SessionId = string.Empty,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 1,
                    ProductName = "Product 1",
                    ProductSKU = "SKU-001",
                    ProductImageUrl = "http://example.com/img.jpg",
                    Quantity = 1,
                    UnitPrice = 100m,
                    TaxRate = 21m,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        var cart2 = new ShoppingCart
        {
            ClientId = clientId2,
            SessionId = string.Empty,
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<CartItem>
            {
                new CartItem
                {
                    ProductId = 2,
                    ProductName = "Product 2",
                    ProductSKU = "SKU-002",
                    ProductImageUrl = "http://example.com/img2.jpg",
                    Quantity = 1,
                    UnitPrice = 50m,
                    TaxRate = 21m,
                    AddedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                }
            }
        };

        context.ShoppingCarts.Add(cart1);
        context.ShoppingCarts.Add(cart2);
        await context.SaveChangesAsync();

        var consumer = new OrderCreatedConsumer(context, _loggerMock.Object);

        // Create order for client 1 only
        var orderCreatedEvent = new OrderCreatedEvent
        {
            OrderId = 300,
            ClientId = clientId1,
            Total = 100m,
            Items = new List<OrderItemInfo>
            {
                new() { ProductId = 1, Quantity = 1, UnitPrice = 100m }
            }
        };

        var consumeContextMock = new Mock<ConsumeContext<OrderCreatedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(orderCreatedEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        var cart1InDb = context.ShoppingCarts.FirstOrDefault(c => c.ClientId == clientId1);
        cart1InDb.Should().BeNull("Cart for client 1 should be removed");

        var cart2InDb = context.ShoppingCarts.FirstOrDefault(c => c.ClientId == clientId2);
        cart2InDb.Should().NotBeNull("Cart for client 2 should still exist");
        cart2InDb!.Items.Should().HaveCount(1);
    }
}

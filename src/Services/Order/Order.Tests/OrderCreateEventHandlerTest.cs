using Common.Messaging.Events.Orders;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Order.Service.EventHandlers;
using Order.Service.EventHandlers.Commands;
using Order.Service.Proxies.Catalog;
using Order.Service.Proxies.Catalog.Commands;
using Order.Tests.Config;
using static Order.Common.Enums;

namespace Order.Tests;

[TestClass]
public class OrderCreateEventHandlerTest
{
    private Mock<ILogger<OrderCreateEventHandler>> _loggerMock = null!;
    private Mock<IPublishEndpoint> _publishEndpointMock = null!;
    private Mock<ICatalogProxy> _catalogProxyMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<OrderCreateEventHandler>>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _catalogProxyMock = new Mock<ICatalogProxy>();
        
        // Setup catalog proxy to succeed by default
        _catalogProxyMock
            .Setup(x => x.UpdateStockAsync(It.IsAny<ProductInStockUpdateStockCommand>()))
            .Returns(Task.CompletedTask);
    }

    [TestMethod]
    public async Task Should_CreateOrder_When_ValidData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            PaymentType = OrderPayment.MercadoPago,
            ShippingRecipientName = "John Doe",
            ShippingPhone = "+54 11 1234-5678",
            ShippingAddressLine1 = "Av. Corrientes 1234",
            ShippingCity = "Buenos Aires",
            ShippingState = "CABA",
            ShippingPostalCode = "C1043",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 2, Price = 100m },
                new() { ProductId = 2, Quantity = 1, Price = 50m }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().BeGreaterThan(0);

        var orderInDb = await context.Orders.FindAsync(orderId);
        orderInDb.Should().NotBeNull();
        orderInDb!.ClientId.Should().Be(1);
        orderInDb.Status.Should().Be(OrderStatus.AwaitingPayment);
        orderInDb.Total.Should().Be(250m); // (2 * 100) + (1 * 50)
    }

    [TestMethod]
    public async Task Should_CalculateTotals_Correctly()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            PaymentType = OrderPayment.CreditCard,
            ShippingRecipientName = "Test User",
            ShippingAddressLine1 = "Test Address",
            ShippingCity = "Test City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 3, Price = 99.99m },
                new() { ProductId = 2, Quantity = 2, Price = 149.50m }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var orderInDb = await context.Orders.FindAsync(orderId);
        var expectedTotal = (3 * 99.99m) + (2 * 149.50m); // 299.97 + 299.00 = 598.97
        orderInDb!.Total.Should().Be(expectedTotal);
    }

    [TestMethod]
    public async Task Should_CreateOrderWithSingleItem()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 5,
            PaymentType = OrderPayment.DebitCard,
            ShippingRecipientName = "Single Item Buyer",
            ShippingAddressLine1 = "Single Address",
            ShippingCity = "City",
            ShippingPostalCode = "00000",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 10, Quantity = 1, Price = 199.99m }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        orderId.Should().BeGreaterThan(0);
        
        var orderInDb = await context.Orders.FindAsync(orderId);
        orderInDb!.Total.Should().Be(199.99m);
        orderInDb.Items.Should().HaveCount(1);
        orderInDb.Items.First().Quantity.Should().Be(1);
    }

    [TestMethod]
    public async Task Should_SetOrderStatus_ToAwaitingPayment()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            PaymentType = OrderPayment.MercadoPago,
            ShippingRecipientName = "Test",
            ShippingAddressLine1 = "Address",
            ShippingCity = "City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 1, Price = 100m }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var orderInDb = await context.Orders.FindAsync(orderId);
        orderInDb!.Status.Should().Be(OrderStatus.AwaitingPayment);
    }

    [TestMethod]
    public async Task Should_PublishOrderCreatedEvent_WithCorrectData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 123,
            PaymentType = OrderPayment.MercadoPago,
            ShippingRecipientName = "Event Test User",
            ShippingAddressLine1 = "Event Address",
            ShippingCity = "Event City",
            ShippingPostalCode = "99999",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 2, Price = 50m },
                new() { ProductId = 2, Quantity = 1, Price = 100m }
            }
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _publishEndpointMock.Verify(x => x.Publish(
            It.Is<OrderCreatedEvent>(e =>
                e.ClientId == 123 &&
                e.Total == 200m &&
                e.Items.Count == 2 &&
                e.Items.Any(i => i.ProductId == 1 && i.Quantity == 2) &&
                e.Items.Any(i => i.ProductId == 2 && i.Quantity == 1)),
            It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [TestMethod]
    public async Task Should_CallCatalogProxy_ToUpdateStock()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            PaymentType = OrderPayment.MercadoPago,
            ShippingRecipientName = "Stock Test",
            ShippingAddressLine1 = "Address",
            ShippingCity = "City",
            ShippingPostalCode = "12345",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 3, Price = 100m },
                new() { ProductId = 2, Quantity = 2, Price = 50m }
            }
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        _catalogProxyMock.Verify(x => x.UpdateStockAsync(
            It.Is<ProductInStockUpdateStockCommand>(cmd =>
                cmd.Items.Count() == 2 &&
                cmd.Items.Any(i => i.ProductId == 1 && i.Stock == 3) &&
                cmd.Items.Any(i => i.ProductId == 2 && i.Stock == 2))),
            Times.Once);
    }

    [TestMethod]
    public async Task Should_SetShippingAddress_Correctly()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            PaymentType = OrderPayment.MercadoPago,
            ShippingRecipientName = "María García",
            ShippingPhone = "+54 11 9999-8888",
            ShippingAddressLine1 = "Av. Santa Fe 1500",
            ShippingAddressLine2 = "Piso 3, Depto B",
            ShippingCity = "Buenos Aires",
            ShippingState = "CABA",
            ShippingPostalCode = "C1060",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = true,
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 1, Price = 100m }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var orderInDb = await context.Orders.FindAsync(orderId);
        orderInDb!.ShippingRecipientName.Should().Be("María García");
        orderInDb.ShippingPhone.Should().Be("+54 11 9999-8888");
        orderInDb.ShippingAddressLine1.Should().Be("Av. Santa Fe 1500");
        orderInDb.ShippingAddressLine2.Should().Be("Piso 3, Depto B");
        orderInDb.ShippingCity.Should().Be("Buenos Aires");
        orderInDb.ShippingState.Should().Be("CABA");
        orderInDb.ShippingPostalCode.Should().Be("C1060");
        orderInDb.ShippingCountry.Should().Be("Argentina");
    }

    [TestMethod]
    public async Task Should_UseBillingAddress_WhenNotSameAsShipping()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new OrderCreateEventHandler(
            context, 
            _catalogProxyMock.Object, 
            _publishEndpointMock.Object, 
            _loggerMock.Object);

        var command = new OrderCreateCommand
        {
            ClientId = 1,
            PaymentType = OrderPayment.CreditCard,
            ShippingRecipientName = "Shipping Name",
            ShippingAddressLine1 = "Shipping Address",
            ShippingCity = "Shipping City",
            ShippingPostalCode = "11111",
            ShippingCountry = "Argentina",
            BillingSameAsShipping = false,
            BillingAddressLine1 = "Billing Address",
            BillingCity = "Billing City",
            BillingPostalCode = "22222",
            BillingCountry = "Argentina",
            Items = new List<OrderCreateDetail>
            {
                new() { ProductId = 1, Quantity = 1, Price = 100m }
            }
        };

        // Act
        var orderId = await handler.Handle(command, CancellationToken.None);

        // Assert
        var orderInDb = await context.Orders.FindAsync(orderId);
        orderInDb!.BillingSameAsShipping.Should().BeFalse();
        orderInDb.BillingAddressLine1.Should().Be("Billing Address");
        orderInDb.BillingCity.Should().Be("Billing City");
        orderInDb.BillingPostalCode.Should().Be("22222");
    }
}

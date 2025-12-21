using Common.Messaging.Events.Payments;
using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Notification.Api.Consumers;
using Notification.Service.EventHandlers.Services;

namespace Notification.Tests;

/// <summary>
/// Tests para el consumer PaymentCompletedConsumer que envía emails de confirmación.
/// </summary>
[TestClass]
public class PaymentCompletedConsumerTest
{
    private Mock<IEmailNotificationService> _emailServiceMock = null!;
    private Mock<ILogger<PaymentCompletedConsumer>> _loggerMock = null!;

    [TestInitialize]
    public void Setup()
    {
        _emailServiceMock = new Mock<IEmailNotificationService>();
        _loggerMock = new Mock<ILogger<PaymentCompletedConsumer>>();
    }

    [TestMethod]
    public async Task Should_SendConfirmationEmail_WhenPaymentCompleted()
    {
        // Arrange
        _emailServiceMock
            .Setup(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .Returns(Task.CompletedTask);

        var consumer = new PaymentCompletedConsumer(_emailServiceMock.Object, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 1,
            OrderId = 100,
            ClientId = 1,
            ClientEmail = "customer@example.com",
            ClientName = "John Doe",
            Amount = 1500m,
            PaymentMethod = "MercadoPago",
            TransactionId = "TXN-12345",
            PaidAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        _emailServiceMock.Verify(x => x.SendTemplatedEmailAsync(
            "customer@example.com",
            "OrderConfirmation",
            It.Is<Dictionary<string, object>>(d =>
                d.ContainsKey("CustomerName") &&
                d.ContainsKey("OrderNumber") &&
                d.ContainsKey("Amount"))),
            Times.Once);
    }

    [TestMethod]
    public async Task Should_IncludeCorrectData_InEmailTemplate()
    {
        // Arrange
        Dictionary<string, object>? capturedData = null;

        _emailServiceMock
            .Setup(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .Callback<string, string, Dictionary<string, object>>((email, template, data) =>
            {
                capturedData = data;
            })
            .Returns(Task.CompletedTask);

        var consumer = new PaymentCompletedConsumer(_emailServiceMock.Object, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 1,
            OrderId = 12345,
            ClientId = 1,
            ClientEmail = "test@example.com",
            ClientName = "Jane Smith",
            Amount = 2500.50m,
            PaymentMethod = "Visa",
            TransactionId = "TXN-ABCDE",
            PaidAt = new DateTime(2024, 12, 18, 15, 30, 0)
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!["CustomerName"].Should().Be("Jane Smith");
        capturedData["OrderNumber"].Should().Be("#ORD-00012345");
        capturedData["PaymentMethod"].Should().Be("Visa");
        capturedData["TransactionId"].Should().Be("TXN-ABCDE");
    }

    [TestMethod]
    public async Task Should_ThrowException_WhenEmailServiceFails()
    {
        // Arrange
        _emailServiceMock
            .Setup(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .ThrowsAsync(new Exception("SMTP connection failed"));

        var consumer = new PaymentCompletedConsumer(_emailServiceMock.Object, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 1,
            OrderId = 100,
            ClientEmail = "test@example.com",
            ClientName = "Test User",
            Amount = 100m,
            PaymentMethod = "Test",
            PaidAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act & Assert
        var action = async () => await consumer.Consume(consumeContextMock.Object);
        await action.Should().ThrowAsync<Exception>()
            .WithMessage("*SMTP*");
    }

    [TestMethod]
    public async Task Should_HandleNullTransactionId_Gracefully()
    {
        // Arrange
        Dictionary<string, object>? capturedData = null;

        _emailServiceMock
            .Setup(x => x.SendTemplatedEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<Dictionary<string, object>>()))
            .Callback<string, string, Dictionary<string, object>>((email, template, data) =>
            {
                capturedData = data;
            })
            .Returns(Task.CompletedTask);

        var consumer = new PaymentCompletedConsumer(_emailServiceMock.Object, _loggerMock.Object);

        var paymentEvent = new PaymentCompletedEvent
        {
            PaymentId = 1,
            OrderId = 100,
            ClientEmail = "test@example.com",
            ClientName = "Test User",
            Amount = 100m,
            PaymentMethod = "Test",
            TransactionId = null, // Null transaction ID
            PaidAt = DateTime.UtcNow
        };

        var consumeContextMock = new Mock<ConsumeContext<PaymentCompletedEvent>>();
        consumeContextMock.Setup(x => x.Message).Returns(paymentEvent);

        // Act
        await consumer.Consume(consumeContextMock.Object);

        // Assert
        capturedData.Should().NotBeNull();
        capturedData!["TransactionId"].Should().Be("N/A");
    }
}

using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Payment.Service.Gateways;
using Payment.Service.Gateways.Mock;

namespace Payment.Tests;

/// <summary>
/// Tests para el MockPaymentGateway que simula pagos de MercadoPago.
/// </summary>
[TestClass]
public class MockPaymentGatewayTest
{
    private Mock<ILogger<MockPaymentGateway>> _loggerMock = null!;
    private IConfiguration _configuration = null!;

    [TestInitialize]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<MockPaymentGateway>>();
        
        // Configuración sin delay para tests rápidos
        var configValues = new Dictionary<string, string?>
        {
            { "PaymentGateway:MockSettings:SimulateDelay", "false" },
            { "PaymentGateway:MockSettings:DelayMilliseconds", "0" }
        };
        
        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues)
            .Build();
    }

    [TestMethod]
    public async Task Should_ApprovePayment_WhenCardholderNameIsAPRO()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 100m,
            Currency = "ARS",
            PaymentToken = "test_token",
            CardholderName = "APRO",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.TransactionId.Should().StartWith("MOCK_");
        result.Gateway.Should().Contain("MercadoPago");
    }

    [TestMethod]
    public async Task Should_RejectPayment_WhenCardholderNameIsFUND()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 100m,
            Currency = "ARS",
            PaymentToken = "test_token",
            CardholderName = "FUND",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("cc_rejected_insufficient_amount");
        result.ErrorMessage.Should().Contain("insuficiente");
    }

    [TestMethod]
    public async Task Should_RejectPayment_WhenCardholderNameIsSECU()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 100m,
            Currency = "ARS",
            PaymentToken = "test_token",
            CardholderName = "SECU",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("cc_rejected_bad_filled_security_code");
    }

    [TestMethod]
    public async Task Should_RejectPayment_WhenAmountExceedsLimit()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 10000m, // Exceeds 9999 limit
            Currency = "ARS",
            PaymentToken = "test_token",
            CardholderName = "Regular User",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("amount_exceeded");
    }

    [TestMethod]
    public async Task Should_RejectPayment_WhenTokenIsMOCK_FAIL_TOKEN()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 100m,
            Currency = "ARS",
            PaymentToken = "MOCK_FAIL_TOKEN",
            CardholderName = "Regular User",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorCode.Should().Be("invalid_token");
    }

    [TestMethod]
    public async Task Should_ApprovePayment_ByDefault_WhenNoTestName()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 500m,
            Currency = "ARS",
            PaymentToken = "valid_token",
            CardholderName = "John Doe", // Not a test name
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeTrue();
        result.TransactionId.Should().StartWith("MOCK_");
    }

    [TestMethod]
    public async Task Should_ProcessRefund_Successfully()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var refundRequest = new RefundRequest
        {
            TransactionId = "MOCK_12345_ABCD1234",
            Amount = 100m
        };

        // Act
        var result = await gateway.ProcessRefundAsync(refundRequest);

        // Assert
        result.Success.Should().BeTrue();
        result.RefundId.Should().StartWith("REFUND_");
    }

    [TestMethod]
    public async Task Should_FailRefund_WhenTransactionIdIsInvalid()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var refundRequest = new RefundRequest
        {
            TransactionId = "INVALID_TXN_ID", // Not a MOCK_ transaction
            Amount = 100m
        };

        // Act
        var result = await gateway.ProcessRefundAsync(refundRequest);

        // Assert
        result.Success.Should().BeFalse();
        result.ErrorMessage.Should().Contain("Invalid transaction ID");
    }

    [TestMethod]
    public async Task Should_ReturnCardInfo_InPaymentResult()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 100m,
            Currency = "ARS",
            PaymentToken = "visa_token",
            PaymentMethodId = "visa",
            CardholderName = "APRO",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.CardLast4.Should().NotBeNullOrEmpty();
        result.CardLast4.Should().HaveLength(4);
        result.CardBrand.Should().Be("Visa");
    }

    [TestMethod]
    public async Task Should_HandlePendingPayment_WhenCardholderNameIsCONT()
    {
        // Arrange
        var gateway = new MockPaymentGateway(_loggerMock.Object, _configuration);
        var request = new PaymentRequest
        {
            Amount = 100m,
            Currency = "ARS",
            PaymentToken = "test_token",
            CardholderName = "CONT",
            PayerEmail = "test@example.com"
        };

        // Act
        var result = await gateway.ProcessPaymentAsync(request);

        // Assert
        result.Success.Should().BeTrue(); // Pending is treated as success
        result.TransactionId.Should().StartWith("MOCK_");
    }
}

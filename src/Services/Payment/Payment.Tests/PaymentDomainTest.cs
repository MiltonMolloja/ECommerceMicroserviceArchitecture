using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Payment.Domain;

namespace Payment.Tests;

/// <summary>
/// Tests unitarios para la lógica de dominio de Payment.
/// </summary>
[TestClass]
public class PaymentDomainTest
{
    private Domain.Payment CreatePendingPayment()
    {
        return new Domain.Payment
        {
            PaymentId = 1,
            OrderId = 100,
            UserId = 1,
            Amount = 1000m,
            Currency = "ARS",
            Status = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.MercadoPago,
            TransactionId = string.Empty,
            PaymentGateway = "MercadoPago",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Transactions = new List<PaymentTransaction>()
        };
    }

    [TestMethod]
    public void Should_MarkAsCompleted_Successfully()
    {
        // Arrange
        var payment = CreatePendingPayment();
        var transactionId = "TXN-12345";

        // Act
        payment.MarkAsCompleted(transactionId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Completed);
        payment.TransactionId.Should().Be(transactionId);
        payment.PaymentDate.Should().NotBeNull();
        payment.IsCompleted.Should().BeTrue();
        payment.IsPending.Should().BeFalse();
    }

    [TestMethod]
    public void Should_MarkAsFailed_AndAddTransaction()
    {
        // Arrange
        var payment = CreatePendingPayment();
        var errorMessage = "Insufficient funds";

        // Act
        payment.MarkAsFailed(errorMessage);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Failed);
        payment.IsCompleted.Should().BeFalse();
        payment.Transactions.Should().HaveCount(1);
        payment.Transactions.First().TransactionType.Should().Be(TransactionType.Charge);
        payment.Transactions.First().Status.Should().Be(TransactionStatus.Failed);
        payment.Transactions.First().ErrorMessage.Should().Be(errorMessage);
    }

    [TestMethod]
    public void Should_MarkAsRefunded_WhenEligible()
    {
        // Arrange
        var payment = CreatePendingPayment();
        payment.MarkAsCompleted("TXN-12345");
        // Payment was made today, so it's within 30 days
        var refundTransactionId = "REFUND-12345";

        // Act
        payment.MarkAsRefunded(refundTransactionId);

        // Assert
        payment.Status.Should().Be(PaymentStatus.Refunded);
        payment.Transactions.Should().HaveCount(1);
        payment.Transactions.First().TransactionType.Should().Be(TransactionType.Refund);
        payment.Transactions.First().Status.Should().Be(TransactionStatus.Success);
    }

    [TestMethod]
    public void Should_ThrowException_WhenRefundingNonCompletedPayment()
    {
        // Arrange
        var payment = CreatePendingPayment();

        // Act & Assert
        var action = () => payment.MarkAsRefunded("REFUND-12345");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be refunded*");
    }

    [TestMethod]
    public void Should_ThrowException_WhenRefundingAfter30Days()
    {
        // Arrange
        var payment = CreatePendingPayment();
        payment.MarkAsCompleted("TXN-12345");
        // Simulate payment made 31 days ago
        payment.PaymentDate = DateTime.UtcNow.AddDays(-31);

        // Act & Assert
        var action = () => payment.MarkAsRefunded("REFUND-12345");
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*cannot be refunded*");
    }

    [TestMethod]
    public void Should_AddTransaction_Successfully()
    {
        // Arrange
        var payment = CreatePendingPayment();

        // Act
        payment.AddTransaction(
            TransactionType.Charge,
            payment.Amount,
            TransactionStatus.Success,
            null,
            "GATEWAY-TXN-123");

        // Assert
        payment.Transactions.Should().HaveCount(1);
        var transaction = payment.Transactions.First();
        transaction.TransactionType.Should().Be(TransactionType.Charge);
        transaction.Amount.Should().Be(payment.Amount);
        transaction.Status.Should().Be(TransactionStatus.Success);
        transaction.GatewayResponse.Should().Be("GATEWAY-TXN-123");
    }

    [TestMethod]
    public void Should_CalculateCanBeRefunded_Correctly()
    {
        // Arrange
        var payment = CreatePendingPayment();

        // Assert - Pending payment cannot be refunded
        payment.CanBeRefunded.Should().BeFalse();

        // Act - Complete the payment
        payment.MarkAsCompleted("TXN-12345");

        // Assert - Completed payment within 30 days can be refunded
        payment.CanBeRefunded.Should().BeTrue();

        // Act - Simulate payment made 31 days ago
        payment.PaymentDate = DateTime.UtcNow.AddDays(-31);

        // Assert - Payment older than 30 days cannot be refunded
        payment.CanBeRefunded.Should().BeFalse();
    }

    [TestMethod]
    public void Should_UpdateTimestamps_OnStatusChange()
    {
        // Arrange
        var payment = CreatePendingPayment();
        var originalUpdatedAt = payment.UpdatedAt;

        // Wait a bit to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        payment.MarkAsCompleted("TXN-12345");

        // Assert
        payment.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [TestMethod]
    public void Should_HaveCorrectInitialState()
    {
        // Arrange & Act
        var payment = new Domain.Payment
        {
            PaymentId = 1,
            OrderId = 100,
            UserId = 1,
            Amount = 500m,
            Currency = "ARS",
            Status = PaymentStatus.Pending,
            PaymentMethod = PaymentMethod.MercadoPago,
            TransactionId = string.Empty,
            PaymentGateway = "MercadoPago",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        payment.IsPending.Should().BeTrue();
        payment.IsCompleted.Should().BeFalse();
        payment.CanBeRefunded.Should().BeFalse();
        payment.PaymentDate.Should().BeNull();
    }
}

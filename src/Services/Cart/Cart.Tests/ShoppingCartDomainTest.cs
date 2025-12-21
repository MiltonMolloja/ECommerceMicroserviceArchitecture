using Cart.Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cart.Tests;

/// <summary>
/// Tests unitarios para la lógica de dominio de ShoppingCart.
/// </summary>
[TestClass]
public class ShoppingCartDomainTest
{
    private ShoppingCart CreateEmptyCart(int? clientId = 1)
    {
        return new ShoppingCart
        {
            CartId = 1,
            ClientId = clientId,
            SessionId = clientId.HasValue ? string.Empty : Guid.NewGuid().ToString(),
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Items = new List<CartItem>()
        };
    }

    [TestMethod]
    public void Should_AddItem_ToCart()
    {
        // Arrange
        var cart = CreateEmptyCart();

        // Act
        cart.AddItem(productId: 1, quantity: 2, unitPrice: 100m, taxRate: 21m, productName: "Product 1");

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(1);
        cart.Items.First().Quantity.Should().Be(2);
        cart.Items.First().UnitPrice.Should().Be(100m);
        cart.ItemCount.Should().Be(2);
    }

    [TestMethod]
    public void Should_IncrementQuantity_WhenAddingExistingProduct()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 2, unitPrice: 100m, taxRate: 21m, productName: "Product 1");

        // Act
        cart.AddItem(productId: 1, quantity: 3, unitPrice: 100m, taxRate: 21m, productName: "Product 1");

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(5); // 2 + 3
        cart.ItemCount.Should().Be(5);
    }

    [TestMethod]
    public void Should_RemoveItem_FromCart()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 2, unitPrice: 100m, taxRate: 21m, productName: "Product 1");
        cart.AddItem(productId: 2, quantity: 1, unitPrice: 50m, taxRate: 21m, productName: "Product 2");

        // Act
        cart.RemoveItem(productId: 1);

        // Assert
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(2);
    }

    [TestMethod]
    public void Should_ClearCart()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 2, unitPrice: 100m, taxRate: 21m, productName: "Product 1");
        cart.AddItem(productId: 2, quantity: 1, unitPrice: 50m, taxRate: 21m, productName: "Product 2");
        cart.ApplyCoupon("DISCOUNT10", 10);

        // Act
        cart.Clear();

        // Assert
        cart.Items.Should().BeEmpty();
        cart.IsEmpty.Should().BeTrue();
        cart.HasCoupon.Should().BeFalse();
    }

    [TestMethod]
    public void Should_ApplyCoupon()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 1, unitPrice: 100m, taxRate: 0m, productName: "Product 1");

        // Act
        cart.ApplyCoupon("DISCOUNT20", 20);

        // Assert
        cart.HasCoupon.Should().BeTrue();
        cart.CouponCode.Should().Be("DISCOUNT20");
        cart.CouponDiscountPercentage.Should().Be(20);
        cart.CouponDiscount.Should().Be(20m); // 100 * 20%
        cart.SubtotalAfterCoupon.Should().Be(80m); // 100 - 20
    }

    [TestMethod]
    public void Should_RemoveCoupon()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 1, unitPrice: 100m, taxRate: 0m, productName: "Product 1");
        cart.ApplyCoupon("DISCOUNT20", 20);

        // Act
        cart.RemoveCoupon();

        // Assert
        cart.HasCoupon.Should().BeFalse();
        cart.CouponDiscount.Should().Be(0);
        cart.SubtotalAfterCoupon.Should().Be(100m);
    }

    [TestMethod]
    public void Should_CalculateTotals_Correctly()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 2, unitPrice: 100m, taxRate: 21m, productName: "Product 1"); // 200 + 42 tax
        cart.AddItem(productId: 2, quantity: 1, unitPrice: 50m, taxRate: 21m, productName: "Product 2");  // 50 + 10.5 tax

        // Assert
        cart.Subtotal.Should().Be(250m); // 200 + 50
        cart.TaxTotal.Should().Be(52.5m); // 42 + 10.5
        cart.Total.Should().Be(302.5m); // 250 + 52.5
        cart.ItemCount.Should().Be(3); // 2 + 1
        cart.UniqueItemCount.Should().Be(2);
    }

    [TestMethod]
    public void Should_MarkAsAbandoned()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 1, unitPrice: 100m, taxRate: 0m, productName: "Product 1");

        // Act
        cart.MarkAsAbandoned();

        // Assert
        cart.Status.Should().Be(CartStatus.Abandoned);
    }

    [TestMethod]
    public void Should_ConvertToOrder()
    {
        // Arrange
        var cart = CreateEmptyCart();
        cart.AddItem(productId: 1, quantity: 1, unitPrice: 100m, taxRate: 0m, productName: "Product 1");

        // Act
        cart.ConvertToOrder(orderId: 123);

        // Assert
        cart.Status.Should().Be(CartStatus.Converted);
        cart.OrderId.Should().Be(123);
        cart.ConvertedAt.Should().NotBeNull();
    }

    [TestMethod]
    public void Should_AssignToClient()
    {
        // Arrange
        var cart = CreateEmptyCart(clientId: null); // Anonymous cart
        cart.SessionId = "session-123";

        // Act
        cart.AssignToClient(clientId: 42);

        // Assert
        cart.ClientId.Should().Be(42);
        cart.IsAnonymous.Should().BeFalse();
        cart.SessionId.Should().BeNull();
        cart.ExpiresAt.Should().BeNull();
    }

    [TestMethod]
    public void Should_ThrowException_WhenAddingZeroQuantity()
    {
        // Arrange
        var cart = CreateEmptyCart();

        // Act & Assert
        var action = () => cart.AddItem(productId: 1, quantity: 0, unitPrice: 100m, taxRate: 0m, productName: "Product");
        action.Should().Throw<ArgumentException>().WithMessage("*Quantity*");
    }

    [TestMethod]
    public void Should_ThrowException_WhenAddingNegativePrice()
    {
        // Arrange
        var cart = CreateEmptyCart();

        // Act & Assert
        var action = () => cart.AddItem(productId: 1, quantity: 1, unitPrice: -10m, taxRate: 0m, productName: "Product");
        action.Should().Throw<ArgumentException>().WithMessage("*price*");
    }

    [TestMethod]
    public void Should_ThrowException_WhenApplyingInvalidCouponPercentage()
    {
        // Arrange
        var cart = CreateEmptyCart();

        // Act & Assert
        var action = () => cart.ApplyCoupon("INVALID", 150);
        action.Should().Throw<ArgumentException>().WithMessage("*percentage*");
    }

    [TestMethod]
    public void Should_ThrowException_WhenAssigningToAlreadyAssignedCart()
    {
        // Arrange
        var cart = CreateEmptyCart(clientId: 1); // Already assigned

        // Act & Assert
        var action = () => cart.AssignToClient(clientId: 2);
        action.Should().Throw<InvalidOperationException>().WithMessage("*already assigned*");
    }

    [TestMethod]
    public void Should_IdentifyExpiredCart()
    {
        // Arrange
        var cart = CreateEmptyCart(clientId: null);
        cart.ExpiresAt = DateTime.UtcNow.AddDays(-1); // Expired yesterday

        // Assert
        cart.IsExpired.Should().BeTrue();
    }

    [TestMethod]
    public void Should_IdentifyNonExpiredCart()
    {
        // Arrange
        var cart = CreateEmptyCart(clientId: null);
        cart.ExpiresAt = DateTime.UtcNow.AddDays(7); // Expires in 7 days

        // Assert
        cart.IsExpired.Should().BeFalse();
    }
}

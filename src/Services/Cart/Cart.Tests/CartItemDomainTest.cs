using Cart.Domain;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cart.Tests;

/// <summary>
/// Tests unitarios para la lógica de dominio de CartItem.
/// </summary>
[TestClass]
public class CartItemDomainTest
{
    private CartItem CreateCartItem(
        int quantity = 2,
        decimal unitPrice = 100m,
        decimal discountPercentage = 0m,
        decimal taxRate = 21m)
    {
        return new CartItem
        {
            CartItemId = 1,
            CartId = 1,
            ProductId = 1,
            ProductName = "Test Product",
            ProductSKU = "SKU-001",
            ProductImageUrl = "https://example.com/image.jpg",
            Quantity = quantity,
            UnitPrice = unitPrice,
            DiscountPercentage = discountPercentage,
            TaxRate = taxRate,
            AddedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    #region Computed Properties Tests

    [TestMethod]
    public void UnitPriceAfterDiscount_Should_ReturnFullPrice_WhenNoDiscount()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m, discountPercentage: 0m);

        // Assert
        item.UnitPriceAfterDiscount.Should().Be(100m);
    }

    [TestMethod]
    public void UnitPriceAfterDiscount_Should_ApplyDiscount_Correctly()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m, discountPercentage: 20m);

        // Assert
        item.UnitPriceAfterDiscount.Should().Be(80m); // 100 - 20%
    }

    [TestMethod]
    public void UnitPriceAfterDiscount_Should_Return0_When100PercentDiscount()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m, discountPercentage: 100m);

        // Assert
        item.UnitPriceAfterDiscount.Should().Be(0m);
    }

    [TestMethod]
    public void LineTotal_Should_CalculateCorrectly_WithoutDiscount()
    {
        // Arrange
        var item = CreateCartItem(quantity: 3, unitPrice: 50m, discountPercentage: 0m);

        // Assert
        item.LineTotal.Should().Be(150m); // 3 * 50
    }

    [TestMethod]
    public void LineTotal_Should_CalculateCorrectly_WithDiscount()
    {
        // Arrange
        var item = CreateCartItem(quantity: 2, unitPrice: 100m, discountPercentage: 10m);

        // Assert
        item.LineTotal.Should().Be(180m); // 2 * (100 - 10%) = 2 * 90
    }

    [TestMethod]
    public void TaxPerItem_Should_CalculateCorrectly()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m, discountPercentage: 0m, taxRate: 21m);

        // Assert
        item.TaxPerItem.Should().Be(21m); // 100 * 21%
    }

    [TestMethod]
    public void TaxPerItem_Should_ApplyToDiscountedPrice()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m, discountPercentage: 50m, taxRate: 10m);

        // Assert
        item.TaxPerItem.Should().Be(5m); // (100 - 50%) * 10% = 50 * 10%
    }

    [TestMethod]
    public void TaxAmount_Should_CalculateTotalTaxForLine()
    {
        // Arrange
        var item = CreateCartItem(quantity: 3, unitPrice: 100m, discountPercentage: 0m, taxRate: 21m);

        // Assert
        item.TaxAmount.Should().Be(63m); // 21 * 3
    }

    [TestMethod]
    public void LineTotalWithTax_Should_IncludeTax()
    {
        // Arrange
        var item = CreateCartItem(quantity: 2, unitPrice: 100m, discountPercentage: 0m, taxRate: 21m);

        // Assert
        item.LineTotalWithTax.Should().Be(242m); // (100 * 2) + (21 * 2) = 200 + 42
    }

    [TestMethod]
    public void TotalSavings_Should_CalculateDiscountAmount()
    {
        // Arrange
        var item = CreateCartItem(quantity: 2, unitPrice: 100m, discountPercentage: 25m);

        // Assert
        item.TotalSavings.Should().Be(50m); // (100 - 75) * 2 = 25 * 2
    }

    [TestMethod]
    public void TotalSavings_Should_BeZero_WhenNoDiscount()
    {
        // Arrange
        var item = CreateCartItem(quantity: 2, unitPrice: 100m, discountPercentage: 0m);

        // Assert
        item.TotalSavings.Should().Be(0m);
    }

    [TestMethod]
    public void HasDiscount_Should_ReturnTrue_WhenDiscountApplied()
    {
        // Arrange
        var item = CreateCartItem(discountPercentage: 10m);

        // Assert
        item.HasDiscount.Should().BeTrue();
    }

    [TestMethod]
    public void HasDiscount_Should_ReturnFalse_WhenNoDiscount()
    {
        // Arrange
        var item = CreateCartItem(discountPercentage: 0m);

        // Assert
        item.HasDiscount.Should().BeFalse();
    }

    #endregion

    #region Business Methods Tests

    [TestMethod]
    public void UpdateQuantity_Should_UpdateQuantityAndTimestamp()
    {
        // Arrange
        var item = CreateCartItem(quantity: 2);
        var originalUpdatedAt = item.UpdatedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        item.UpdateQuantity(5);

        // Assert
        item.Quantity.Should().Be(5);
        item.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [TestMethod]
    public void UpdateQuantity_Should_ThrowException_WhenQuantityIsZero()
    {
        // Arrange
        var item = CreateCartItem();

        // Act & Assert
        var action = () => item.UpdateQuantity(0);
        action.Should().Throw<ArgumentException>().WithMessage("*Quantity*");
    }

    [TestMethod]
    public void UpdateQuantity_Should_ThrowException_WhenQuantityIsNegative()
    {
        // Arrange
        var item = CreateCartItem();

        // Act & Assert
        var action = () => item.UpdateQuantity(-1);
        action.Should().Throw<ArgumentException>().WithMessage("*Quantity*");
    }

    [TestMethod]
    public void UpdatePrice_Should_UpdatePriceAndTimestamp()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m);
        var originalUpdatedAt = item.UpdatedAt;

        // Small delay to ensure timestamp difference
        Thread.Sleep(10);

        // Act
        item.UpdatePrice(150m);

        // Assert
        item.UnitPrice.Should().Be(150m);
        item.DiscountPercentage.Should().Be(0m);
        item.UpdatedAt.Should().BeAfter(originalUpdatedAt);
    }

    [TestMethod]
    public void UpdatePrice_Should_UpdatePriceAndDiscount()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m, discountPercentage: 0m);

        // Act
        item.UpdatePrice(200m, 15m);

        // Assert
        item.UnitPrice.Should().Be(200m);
        item.DiscountPercentage.Should().Be(15m);
        item.UnitPriceAfterDiscount.Should().Be(170m); // 200 - 15%
    }

    [TestMethod]
    public void UpdatePrice_Should_ThrowException_WhenPriceIsNegative()
    {
        // Arrange
        var item = CreateCartItem();

        // Act & Assert
        var action = () => item.UpdatePrice(-10m);
        action.Should().Throw<ArgumentException>().WithMessage("*Price*");
    }

    [TestMethod]
    public void UpdatePrice_Should_ThrowException_WhenDiscountIsNegative()
    {
        // Arrange
        var item = CreateCartItem();

        // Act & Assert
        var action = () => item.UpdatePrice(100m, -5m);
        action.Should().Throw<ArgumentException>().WithMessage("*Discount*");
    }

    [TestMethod]
    public void UpdatePrice_Should_ThrowException_WhenDiscountExceeds100()
    {
        // Arrange
        var item = CreateCartItem();

        // Act & Assert
        var action = () => item.UpdatePrice(100m, 150m);
        action.Should().Throw<ArgumentException>().WithMessage("*Discount*");
    }

    [TestMethod]
    public void UpdatePrice_Should_AllowZeroPrice()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m);

        // Act
        item.UpdatePrice(0m);

        // Assert
        item.UnitPrice.Should().Be(0m);
    }

    [TestMethod]
    public void UpdatePrice_Should_Allow100PercentDiscount()
    {
        // Arrange
        var item = CreateCartItem(unitPrice: 100m);

        // Act
        item.UpdatePrice(100m, 100m);

        // Assert
        item.DiscountPercentage.Should().Be(100m);
        item.UnitPriceAfterDiscount.Should().Be(0m);
    }

    #endregion

    #region Edge Cases

    [TestMethod]
    public void ComputedProperties_Should_HandleDecimalPrecision()
    {
        // Arrange - Use values that could cause floating point issues
        var item = CreateCartItem(quantity: 3, unitPrice: 33.33m, discountPercentage: 15m, taxRate: 21m);

        // Assert - Verify calculations are precise
        item.UnitPriceAfterDiscount.Should().Be(28.3305m); // 33.33 * 0.85
        item.LineTotal.Should().Be(84.9915m); // 28.3305 * 3
        item.TaxPerItem.Should().Be(5.949405m); // 28.3305 * 0.21
    }

    [TestMethod]
    public void ComputedProperties_Should_HandleLargeQuantities()
    {
        // Arrange
        var item = CreateCartItem(quantity: 10000, unitPrice: 99.99m, taxRate: 21m);

        // Assert
        item.LineTotal.Should().Be(999900m); // 10000 * 99.99
        item.ItemCount_Implicit_Test();
    }

    #endregion
}

// Extension for implicit test
public static class CartItemTestExtensions
{
    public static void ItemCount_Implicit_Test(this CartItem item)
    {
        // This is just to verify the item is valid
        item.Quantity.Should().BeGreaterThan(0);
    }
}

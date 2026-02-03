using Cart.Domain;
using Cart.Service.EventHandlers;
using Cart.Service.EventHandlers.Commands;
using Cart.Tests.Config;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cart.Tests;

[TestClass]
public class CartEventHandlerTest
{
    #region AddItemToCart Tests

    [TestMethod]
    public async Task Should_CreateNewCart_When_NoCartExists_ForClient()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new AddItemToCartCommand
        {
            ClientId = 1,
            SessionId = "session-123",
            ProductId = 100,
            ProductName = "Test Product",
            ProductSKU = "SKU-100",
            ProductImageUrl = "http://example.com/image.jpg",
            Quantity = 2,
            UnitPrice = 99.99m,
            DiscountPercentage = 0,
            TaxRate = 21m
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ClientId == 1);

        cart.Should().NotBeNull();
        cart!.Status.Should().Be(CartStatus.Active);
        cart.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(100);
        cart.Items.First().Quantity.Should().Be(2);
    }

    [TestMethod]
    public async Task Should_CreateNewCart_When_NoCartExists_ForSession()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new AddItemToCartCommand
        {
            ClientId = null,
            SessionId = "anonymous-session-456",
            ProductId = 200,
            ProductName = "Anonymous Product",
            ProductSKU = "SKU-200",
            ProductImageUrl = "http://example.com/anon.jpg",
            Quantity = 1,
            UnitPrice = 50m,
            DiscountPercentage = 10,
            TaxRate = 21m
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.SessionId == "anonymous-session-456");

        cart.Should().NotBeNull();
        cart!.ClientId.Should().BeNull();
        cart.ExpiresAt.Should().NotBeNull(); // Anonymous carts expire
        cart.Items.Should().HaveCount(1);
    }

    [TestMethod]
    public async Task Should_AddItemToExistingCart_When_CartExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        // Create existing cart
        var existingCart = new ShoppingCart
        {
            ClientId = 5,
            SessionId = "existing-session",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ShoppingCarts.Add(existingCart);
        await context.SaveChangesAsync();

        var command = new AddItemToCartCommand
        {
            ClientId = 5,
            SessionId = "existing-session",
            ProductId = 300,
            ProductName = "New Item",
            ProductSKU = "SKU-300",
            ProductImageUrl = "http://example.com/new.jpg",
            Quantity = 3,
            UnitPrice = 25m,
            DiscountPercentage = 0,
            TaxRate = 21m
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ClientId == 5);

        cart!.Items.Should().HaveCount(1);
        cart.Items.First().ProductId.Should().Be(300);
        cart.Items.First().Quantity.Should().Be(3);
    }

    [TestMethod]
    public async Task Should_UpdateQuantity_When_AddingSameProduct()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        // First add
        var command1 = new AddItemToCartCommand
        {
            ClientId = 10,
            SessionId = "session-10",
            ProductId = 500,
            ProductName = "Product 500",
            ProductSKU = "SKU-500",
            ProductImageUrl = "http://example.com/500.jpg",
            Quantity = 2,
            UnitPrice = 100m,
            DiscountPercentage = 0,
            TaxRate = 21m
        };

        await handler.Handle(command1, CancellationToken.None);

        // Second add - same product
        var command2 = new AddItemToCartCommand
        {
            ClientId = 10,
            SessionId = "session-10",
            ProductId = 500,
            ProductName = "Product 500",
            ProductSKU = "SKU-500",
            ProductImageUrl = "http://example.com/500.jpg",
            Quantity = 3,
            UnitPrice = 100m,
            DiscountPercentage = 0,
            TaxRate = 21m
        };

        // Act
        await handler.Handle(command2, CancellationToken.None);

        // Assert
        var cart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.ClientId == 10);

        cart!.Items.Should().HaveCount(1);
        cart.Items.First().Quantity.Should().Be(5); // 2 + 3
    }

    #endregion

    #region UpdateCartItemQuantity Tests

    [TestMethod]
    public async Task Should_UpdateItemQuantity_When_ItemExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        // Create cart with item
        var cart = new ShoppingCart
        {
            ClientId = 20,
            SessionId = "session-20",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        cart.AddItem(600, 2, 50m, 21m, "Product 600");
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new UpdateCartItemQuantityCommand
        {
            CartId = cart.CartId,
            ProductId = 600,
            Quantity = 10
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

        updatedCart!.Items.First().Quantity.Should().Be(10);
    }

    [TestMethod]
    public async Task Should_ThrowException_When_CartNotFound_OnUpdateQuantity()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new UpdateCartItemQuantityCommand
        {
            CartId = 99999,
            ProductId = 1,
            Quantity = 5
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [TestMethod]
    public async Task Should_ThrowException_When_ItemNotFound_OnUpdateQuantity()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = 25,
            SessionId = "session-25",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new UpdateCartItemQuantityCommand
        {
            CartId = cart.CartId,
            ProductId = 99999, // Non-existent product
            Quantity = 5
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region RemoveItemFromCart Tests

    [TestMethod]
    public async Task Should_RemoveItem_When_ItemExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = 30,
            SessionId = "session-30",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        cart.AddItem(700, 2, 50m, 21m, "Product 700");
        cart.AddItem(701, 1, 30m, 21m, "Product 701");
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new RemoveItemFromCartCommand
        {
            CartId = cart.CartId,
            ProductId = 700
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

        updatedCart!.Items.Should().HaveCount(1);
        updatedCart.Items.First().ProductId.Should().Be(701);
    }

    [TestMethod]
    public async Task Should_ThrowException_When_CartNotFound_OnRemoveItem()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new RemoveItemFromCartCommand
        {
            CartId = 99999,
            ProductId = 1
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region ClearCart Tests

    [TestMethod]
    public async Task Should_ClearAllItems_When_ClearCartCalled()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = 40,
            SessionId = "session-40",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        cart.AddItem(800, 2, 50m, 21m, "Product 800");
        cart.AddItem(801, 1, 30m, 21m, "Product 801");
        cart.AddItem(802, 3, 20m, 21m, "Product 802");
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new ClearCartCommand { CartId = cart.CartId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.CartId == cart.CartId);

        updatedCart!.Items.Should().BeEmpty();
    }

    [TestMethod]
    public async Task Should_ThrowException_When_CartNotFound_OnClear()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new ClearCartCommand { CartId = 99999 };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region ApplyCoupon Tests

    [TestMethod]
    public async Task Should_ApplyCoupon_When_CartExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = 50,
            SessionId = "session-50",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new ApplyCouponCommand
        {
            CartId = cart.CartId,
            CouponCode = "DISCOUNT20",
            DiscountPercentage = 20m
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts.FindAsync(cart.CartId);
        updatedCart!.CouponCode.Should().Be("DISCOUNT20");
        updatedCart.CouponDiscountPercentage.Should().Be(20m);
    }

    [TestMethod]
    public async Task Should_ThrowException_When_CartNotFound_OnApplyCoupon()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new ApplyCouponCommand
        {
            CartId = 99999,
            CouponCode = "TEST",
            DiscountPercentage = 10m
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region RemoveCoupon Tests

    [TestMethod]
    public async Task Should_RemoveCoupon_When_CouponApplied()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = 60,
            SessionId = "session-60",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        cart.ApplyCoupon("EXISTING", 15m);
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new RemoveCouponCommand { CartId = cart.CartId };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts.FindAsync(cart.CartId);
        updatedCart!.CouponCode.Should().BeNull();
        updatedCart.CouponDiscountPercentage.Should().Be(0);
    }

    [TestMethod]
    public async Task Should_ThrowException_When_CartNotFound_OnRemoveCoupon()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new RemoveCouponCommand { CartId = 99999 };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region ConvertCartToOrder Tests

    [TestMethod]
    public async Task Should_ConvertCartToOrder_When_CartExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = 70,
            SessionId = "session-70",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        cart.AddItem(900, 2, 100m, 21m, "Product 900");
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new ConvertCartToOrderCommand
        {
            CartId = cart.CartId,
            OrderId = 12345
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts.FindAsync(cart.CartId);
        updatedCart!.Status.Should().Be(CartStatus.Converted);
        updatedCart.OrderId.Should().Be(12345);
    }

    [TestMethod]
    public async Task Should_ThrowException_When_CartNotFound_OnConvert()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new ConvertCartToOrderCommand
        {
            CartId = 99999,
            OrderId = 1
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region AssignCartToClient Tests

    [TestMethod]
    public async Task Should_AssignCartToClient_When_SessionCartExists()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var cart = new ShoppingCart
        {
            ClientId = null,
            SessionId = "anonymous-session-80",
            Status = CartStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        context.ShoppingCarts.Add(cart);
        await context.SaveChangesAsync();

        var command = new AssignCartToClientCommand
        {
            SessionId = "anonymous-session-80",
            ClientId = 100
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedCart = await context.ShoppingCarts.FindAsync(cart.CartId);
        updatedCart!.ClientId.Should().Be(100);
        updatedCart.ExpiresAt.Should().BeNull(); // Client carts don't expire
    }

    [TestMethod]
    public async Task Should_ThrowException_When_SessionCartNotFound_OnAssign()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new CartEventHandler(context);

        var command = new AssignCartToClientCommand
        {
            SessionId = "non-existent-session",
            ClientId = 1
        };

        // Act & Assert
        Func<Task> act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion
}

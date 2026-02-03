using Catalog.Service.EventHandlers;
using Catalog.Service.EventHandlers.Commands;
using Catalog.Tests.Config;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Catalog.Tests;

[TestClass]
public class ProductCreateEventHandlerTest
{
    [TestMethod]
    public async Task Should_CreateProduct_When_ValidData()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ProductCreateEventHandler(context);

        var command = new ProductCreateCommand
        {
            NameSpanish = "Producto de Prueba",
            NameEnglish = "Test Product",
            DescriptionSpanish = "Descripcion en espanol",
            DescriptionEnglish = "Description in English",
            SKU = "TEST-SKU-001",
            Brand = "TestBrand",
            Slug = "test-product",
            Price = 99.99m,
            OriginalPrice = 129.99m,
            DiscountPercentage = 23m,
            TaxRate = 21m,
            Images = "[\"image1.jpg\", \"image2.jpg\"]",
            MetaTitle = "Test Product - Buy Now",
            MetaDescription = "Best test product in the market",
            MetaKeywords = "test, product, sample",
            IsActive = true,
            IsFeatured = false
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var product = await context.Products.FirstOrDefaultAsync(p => p.SKU == "TEST-SKU-001");
        product.Should().NotBeNull();
        product!.NameSpanish.Should().Be("Producto de Prueba");
        product.NameEnglish.Should().Be("Test Product");
        product.Price.Should().Be(99.99m);
        product.OriginalPrice.Should().Be(129.99m);
        product.DiscountPercentage.Should().Be(23m);
        product.IsActive.Should().BeTrue();
        product.IsFeatured.Should().BeFalse();
    }

    [TestMethod]
    public async Task Should_CreateFeaturedProduct_When_IsFeaturedIsTrue()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ProductCreateEventHandler(context);

        var command = new ProductCreateCommand
        {
            NameSpanish = "Producto Destacado",
            NameEnglish = "Featured Product",
            DescriptionSpanish = "Un producto destacado",
            DescriptionEnglish = "A featured product",
            SKU = "FEAT-SKU-001",
            Brand = "FeaturedBrand",
            Slug = "featured-product",
            Price = 199.99m,
            TaxRate = 21m,
            Images = "[\"featured.jpg\"]",
            MetaTitle = "Featured Product",
            MetaDescription = "Our featured product",
            MetaKeywords = "featured",
            IsActive = true,
            IsFeatured = true
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var product = await context.Products.FirstOrDefaultAsync(p => p.SKU == "FEAT-SKU-001");
        product.Should().NotBeNull();
        product!.IsFeatured.Should().BeTrue();
    }

    [TestMethod]
    public async Task Should_CreateProduct_WithoutOriginalPrice()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ProductCreateEventHandler(context);

        var command = new ProductCreateCommand
        {
            NameSpanish = "Producto Sin Descuento",
            NameEnglish = "No Discount Product",
            DescriptionSpanish = "Sin precio original",
            DescriptionEnglish = "No original price",
            SKU = "NODIS-SKU-001",
            Brand = "NormalBrand",
            Slug = "no-discount-product",
            Price = 75.00m,
            OriginalPrice = null,
            DiscountPercentage = 0,
            TaxRate = 21m,
            Images = "[\"normal.jpg\"]",
            MetaTitle = "Normal Product",
            MetaDescription = "A normal product",
            MetaKeywords = "normal",
            IsActive = true,
            IsFeatured = false
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var product = await context.Products.FirstOrDefaultAsync(p => p.SKU == "NODIS-SKU-001");
        product.Should().NotBeNull();
        product!.OriginalPrice.Should().BeNull();
        product.DiscountPercentage.Should().Be(0);
    }

    [TestMethod]
    public async Task Should_CreateMultipleProducts_Successfully()
    {
        // Arrange
        var context = ApplicationDbContextInMemory.Get();
        var handler = new ProductCreateEventHandler(context);

        var command1 = new ProductCreateCommand
        {
            NameSpanish = "Producto 1", NameEnglish = "Product 1",
            DescriptionSpanish = "Desc 1", DescriptionEnglish = "Desc 1",
            SKU = "MULTI-001", Brand = "Brand1", Slug = "product-1",
            Price = 10m, TaxRate = 21m, Images = "[]",
            MetaTitle = "P1", MetaDescription = "P1", MetaKeywords = "p1",
            IsActive = true, IsFeatured = false
        };

        var command2 = new ProductCreateCommand
        {
            NameSpanish = "Producto 2", NameEnglish = "Product 2",
            DescriptionSpanish = "Desc 2", DescriptionEnglish = "Desc 2",
            SKU = "MULTI-002", Brand = "Brand2", Slug = "product-2",
            Price = 20m, TaxRate = 21m, Images = "[]",
            MetaTitle = "P2", MetaDescription = "P2", MetaKeywords = "p2",
            IsActive = true, IsFeatured = true
        };

        // Act
        await handler.Handle(command1, CancellationToken.None);
        await handler.Handle(command2, CancellationToken.None);

        // Assert
        var products = await context.Products.ToListAsync();
        products.Count.Should().Be(2);
        products.Should().Contain(p => p.SKU == "MULTI-001");
        products.Should().Contain(p => p.SKU == "MULTI-002");
    }
}

using Cart.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.Persistence.Database.Configuration
{
    public class CartItemConfiguration
    {
        public CartItemConfiguration(EntityTypeBuilder<CartItem> entityBuilder)
        {
            // Table configuration
            entityBuilder.ToTable("CartItems", "Cart");

            // Primary Key
            entityBuilder.HasKey(x => x.CartItemId);

            // Properties
            entityBuilder.Property(x => x.CartItemId)
                .ValueGeneratedOnAdd();

            entityBuilder.Property(x => x.ProductId)
                .IsRequired();

            entityBuilder.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entityBuilder.Property(x => x.ProductSKU)
                .HasMaxLength(50)
                .IsRequired(false);

            entityBuilder.Property(x => x.ProductImageUrl)
                .HasMaxLength(500)
                .IsRequired(false);

            entityBuilder.Property(x => x.Quantity)
                .IsRequired()
                .HasDefaultValue(1);

            entityBuilder.Property(x => x.UnitPrice)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            entityBuilder.Property(x => x.DiscountPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);

            entityBuilder.Property(x => x.TaxRate)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);

            entityBuilder.Property(x => x.AddedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            entityBuilder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            // Indexes
            entityBuilder.HasIndex(x => x.CartId)
                .HasDatabaseName("IX_CartItems_CartId");

            entityBuilder.HasIndex(x => x.ProductId)
                .HasDatabaseName("IX_CartItems_ProductId");

            entityBuilder.HasIndex(x => new { x.CartId, x.ProductId })
                .HasDatabaseName("IX_CartItems_CartId_ProductId")
                .IsUnique();

            // Relationships
            entityBuilder.HasOne(x => x.Cart)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            entityBuilder.Ignore(x => x.UnitPriceAfterDiscount);
            entityBuilder.Ignore(x => x.LineTotal);
            entityBuilder.Ignore(x => x.TaxPerItem);
            entityBuilder.Ignore(x => x.TaxAmount);
            entityBuilder.Ignore(x => x.LineTotalWithTax);
            entityBuilder.Ignore(x => x.TotalSavings);
            entityBuilder.Ignore(x => x.HasDiscount);
        }
    }
}

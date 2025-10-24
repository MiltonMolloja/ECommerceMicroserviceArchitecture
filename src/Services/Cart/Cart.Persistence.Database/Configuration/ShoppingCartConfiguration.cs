using Cart.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cart.Persistence.Database.Configuration
{
    public class ShoppingCartConfiguration
    {
        public ShoppingCartConfiguration(EntityTypeBuilder<ShoppingCart> entityBuilder)
        {
            // Table configuration
            entityBuilder.ToTable("ShoppingCarts", "Cart");

            // Primary Key
            entityBuilder.HasKey(x => x.CartId);

            // Properties
            entityBuilder.Property(x => x.CartId)
                .ValueGeneratedOnAdd();

            entityBuilder.Property(x => x.ClientId)
                .IsRequired(false);

            entityBuilder.Property(x => x.SessionId)
                .HasMaxLength(100)
                .IsRequired(false);

            entityBuilder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(CartStatus.Active);

            entityBuilder.Property(x => x.CouponCode)
                .HasMaxLength(50)
                .IsRequired(false);

            entityBuilder.Property(x => x.CouponDiscountPercentage)
                .HasColumnType("decimal(5,2)")
                .HasDefaultValue(0);

            entityBuilder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            entityBuilder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()")
                .IsRequired();

            entityBuilder.Property(x => x.ExpiresAt)
                .IsRequired(false);

            entityBuilder.Property(x => x.ConvertedAt)
                .IsRequired(false);

            entityBuilder.Property(x => x.OrderId)
                .IsRequired(false);

            // Indexes
            entityBuilder.HasIndex(x => x.ClientId)
                .HasDatabaseName("IX_ShoppingCarts_ClientId");

            entityBuilder.HasIndex(x => x.SessionId)
                .HasDatabaseName("IX_ShoppingCarts_SessionId");

            entityBuilder.HasIndex(x => x.Status)
                .HasDatabaseName("IX_ShoppingCarts_Status");

            entityBuilder.HasIndex(x => new { x.ClientId, x.Status })
                .HasDatabaseName("IX_ShoppingCarts_ClientId_Status");

            entityBuilder.HasIndex(x => x.ExpiresAt)
                .HasDatabaseName("IX_ShoppingCarts_ExpiresAt")
                .HasFilter("[ExpiresAt] IS NOT NULL");

            // Relationships
            entityBuilder.HasMany(x => x.Items)
                .WithOne(x => x.Cart)
                .HasForeignKey(x => x.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            entityBuilder.Ignore(x => x.Subtotal);
            entityBuilder.Ignore(x => x.CouponDiscount);
            entityBuilder.Ignore(x => x.SubtotalAfterCoupon);
            entityBuilder.Ignore(x => x.TaxTotal);
            entityBuilder.Ignore(x => x.Total);
            entityBuilder.Ignore(x => x.ItemCount);
            entityBuilder.Ignore(x => x.UniqueItemCount);
            entityBuilder.Ignore(x => x.IsEmpty);
            entityBuilder.Ignore(x => x.IsAnonymous);
            entityBuilder.Ignore(x => x.IsExpired);
            entityBuilder.Ignore(x => x.HasCoupon);
        }
    }
}

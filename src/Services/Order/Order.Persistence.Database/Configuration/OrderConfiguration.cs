using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Order.Persistence.Database.Configuration
{
    public class OrderConfiguration
    {
        public OrderConfiguration(EntityTypeBuilder<Domain.Order> entityBuilder)
        {
            entityBuilder.HasKey(x => x.OrderId);
            
            // Table name
            entityBuilder.ToTable("Orders");

            // Explicit column mappings for PostgreSQL compatibility
            entityBuilder.Property(e => e.Status).HasColumnName("Status");
            entityBuilder.Property(e => e.PaymentType).HasColumnName("PaymentType");
            entityBuilder.Property(e => e.SubTotal).HasColumnName("SubTotal");
            entityBuilder.Property(e => e.Tax).HasColumnName("Tax");
            entityBuilder.Property(e => e.Discount).HasColumnName("Discount");

            // Shipping Address
            entityBuilder.Property(e => e.ShippingRecipientName).HasMaxLength(200);
            entityBuilder.Property(e => e.ShippingPhone).HasMaxLength(20);
            entityBuilder.Property(e => e.ShippingAddressLine1).HasMaxLength(200);
            entityBuilder.Property(e => e.ShippingAddressLine2).HasMaxLength(200);
            entityBuilder.Property(e => e.ShippingCity).HasMaxLength(100);
            entityBuilder.Property(e => e.ShippingState).HasMaxLength(100);
            entityBuilder.Property(e => e.ShippingPostalCode).HasMaxLength(20);
            entityBuilder.Property(e => e.ShippingCountry).HasMaxLength(100);

            // Billing Address
            entityBuilder.Property(e => e.BillingAddressLine1).HasMaxLength(200);
            entityBuilder.Property(e => e.BillingCity).HasMaxLength(100);
            entityBuilder.Property(e => e.BillingPostalCode).HasMaxLength(20);
            entityBuilder.Property(e => e.BillingCountry).HasMaxLength(100);
        }
    }
}

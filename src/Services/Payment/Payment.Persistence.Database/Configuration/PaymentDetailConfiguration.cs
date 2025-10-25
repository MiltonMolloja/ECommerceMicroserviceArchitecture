using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Persistence.Database.Configuration
{
    public class PaymentDetailConfiguration : IEntityTypeConfiguration<PaymentDetail>
    {
        public void Configure(EntityTypeBuilder<PaymentDetail> builder)
        {
            builder.ToTable("PaymentDetails");

            builder.HasKey(d => d.PaymentDetailId);

            builder.Property(d => d.PaymentId)
                .IsRequired();

            builder.Property(d => d.CardLast4Digits)
                .HasMaxLength(4);

            builder.Property(d => d.CardBrand)
                .HasMaxLength(20);

            builder.Property(d => d.CardHolderName)
                .HasMaxLength(100);

            builder.Property(d => d.BillingAddress)
                .HasMaxLength(500);

            builder.Property(d => d.BillingCity)
                .HasMaxLength(100);

            builder.Property(d => d.BillingCountry)
                .HasMaxLength(50);

            builder.Property(d => d.BillingZipCode)
                .HasMaxLength(20);

            // Indexes
            builder.HasIndex(d => d.PaymentId);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Persistence.Database.Configuration
{
    public class PaymentTransactionConfiguration : IEntityTypeConfiguration<PaymentTransaction>
    {
        public void Configure(EntityTypeBuilder<PaymentTransaction> builder)
        {
            builder.ToTable("PaymentTransactions");

            builder.HasKey(t => t.TransactionId);

            builder.Property(t => t.PaymentId)
                .IsRequired();

            builder.Property(t => t.TransactionType)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(t => t.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(t => t.GatewayResponse)
                .HasColumnType("NVARCHAR(MAX)");

            builder.Property(t => t.ErrorMessage)
                .HasMaxLength(500);

            builder.Property(t => t.TransactionDate)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(t => t.IPAddress)
                .HasMaxLength(50);

            // Indexes
            builder.HasIndex(t => t.PaymentId);
            builder.HasIndex(t => t.TransactionDate);
        }
    }
}

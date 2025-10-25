using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Payment.Domain;

namespace Payment.Persistence.Database.Configuration
{
    public class PaymentConfiguration : IEntityTypeConfiguration<Domain.Payment>
    {
        public void Configure(EntityTypeBuilder<Domain.Payment> builder)
        {
            builder.ToTable("Payments");

            builder.HasKey(p => p.PaymentId);

            builder.Property(p => p.OrderId)
                .IsRequired();

            builder.Property(p => p.UserId)
                .IsRequired();

            builder.Property(p => p.Amount)
                .HasPrecision(18, 2)
                .IsRequired();

            builder.Property(p => p.Currency)
                .HasMaxLength(3)
                .IsRequired()
                .HasDefaultValue("USD");

            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.PaymentMethod)
                .IsRequired()
                .HasConversion<int>();

            builder.Property(p => p.TransactionId)
                .HasMaxLength(100);

            builder.Property(p => p.PaymentGateway)
                .HasMaxLength(50);

            builder.Property(p => p.PaymentDate)
                .IsRequired(false);

            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            builder.HasIndex(p => p.OrderId);
            builder.HasIndex(p => p.UserId);
            builder.HasIndex(p => p.Status);
            builder.HasIndex(p => p.TransactionId);

            // Relationships
            builder.HasOne(p => p.PaymentDetail)
                .WithOne(d => d.Payment)
                .HasForeignKey<PaymentDetail>(d => d.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(p => p.Transactions)
                .WithOne(t => t.Payment)
                .HasForeignKey(t => t.PaymentId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            builder.Ignore(p => p.IsCompleted);
            builder.Ignore(p => p.IsPending);
            builder.Ignore(p => p.CanBeRefunded);
        }
    }
}

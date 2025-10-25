using Microsoft.EntityFrameworkCore;
using Payment.Domain;
using Payment.Persistence.Database.Configuration;

namespace Payment.Persistence.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        )
            : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Database schema
            builder.HasDefaultSchema("Payment");

            // Apply configurations
            builder.ApplyConfiguration(new PaymentConfiguration());
            builder.ApplyConfiguration(new PaymentDetailConfiguration());
            builder.ApplyConfiguration(new PaymentTransactionConfiguration());
        }

        public DbSet<Domain.Payment> Payments { get; set; }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    }
}

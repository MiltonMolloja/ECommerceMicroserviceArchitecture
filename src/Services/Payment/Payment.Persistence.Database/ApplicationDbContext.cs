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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Database schema
            modelBuilder.HasDefaultSchema("Payment");

            // Apply configurations
            modelBuilder.ApplyConfiguration(new PaymentConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentDetailConfiguration());
            modelBuilder.ApplyConfiguration(new PaymentTransactionConfiguration());
        }

        public DbSet<Domain.Payment> Payments { get; set; }
        public DbSet<PaymentDetail> PaymentDetails { get; set; }
        public DbSet<PaymentTransaction> PaymentTransactions { get; set; }
    }
}

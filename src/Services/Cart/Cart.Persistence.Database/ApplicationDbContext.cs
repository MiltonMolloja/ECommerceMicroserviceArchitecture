using Cart.Domain;
using Cart.Persistence.Database.Configuration;
using Microsoft.EntityFrameworkCore;

namespace Cart.Persistence.Database
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
            modelBuilder.HasDefaultSchema("Cart");

            // Apply configurations
            new ShoppingCartConfiguration(modelBuilder.Entity<ShoppingCart>());
            new CartItemConfiguration(modelBuilder.Entity<CartItem>());
        }

        public DbSet<ShoppingCart> ShoppingCarts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
    }
}

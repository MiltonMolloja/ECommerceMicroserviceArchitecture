using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductInStockConfiguration : IEntityTypeConfiguration<ProductInStock>
    {
        public void Configure(EntityTypeBuilder<ProductInStock> builder)
        {
            builder.ToTable("ProductInStock");
            builder.HasKey(x => x.ProductInStockId);

            builder.Property(x => x.Stock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.MinStock)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.MaxStock)
                .IsRequired()
                .HasDefaultValue(1000);

            // Índices
            builder.HasIndex(x => x.ProductId)
                .IsUnique();

            // Check Constraints
            builder.HasCheckConstraint("CK_Stock_Positive", "[Stock] >= 0");
            builder.HasCheckConstraint("CK_MinStock_Positive", "[MinStock] >= 0");
            builder.HasCheckConstraint("CK_MaxStock_Valid", "[MaxStock] > [MinStock]");

            // Ignorar computed properties
            builder.Ignore(x => x.IsLowStock);
            builder.Ignore(x => x.IsOutOfStock);
            builder.Ignore(x => x.IsOverStock);
        }
    }
}

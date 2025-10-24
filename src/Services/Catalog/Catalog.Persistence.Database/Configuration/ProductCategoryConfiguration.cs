using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductCategoryConfiguration : IEntityTypeConfiguration<ProductCategory>
    {
        public void Configure(EntityTypeBuilder<ProductCategory> builder)
        {
            builder.ToTable("ProductCategories");

            // Composite Primary Key
            builder.HasKey(x => new { x.ProductId, x.CategoryId });

            // Properties
            builder.Property(x => x.IsPrimary)
                .IsRequired()
                .HasDefaultValue(false);

            // Relaciones
            builder.HasOne(x => x.Product)
                .WithMany(p => p.ProductCategories)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.Category)
                .WithMany(c => c.ProductCategories)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para búsquedas
            builder.HasIndex(x => x.CategoryId);
            builder.HasIndex(x => x.IsPrimary);
        }
    }
}

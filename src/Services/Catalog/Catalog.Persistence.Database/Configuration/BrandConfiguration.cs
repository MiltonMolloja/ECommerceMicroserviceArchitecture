using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Database.Configuration
{
    public class BrandConfiguration : IEntityTypeConfiguration<Brand>
    {
        public void Configure(EntityTypeBuilder<Brand> builder)
        {
            builder.ToTable("Brands");

            builder.HasKey(x => x.BrandId);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.Description)
                .HasMaxLength(1000);

            builder.Property(x => x.LogoUrl)
                .HasMaxLength(500);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Índices
            builder.HasIndex(x => x.Name)
                .IsUnique();

            builder.HasIndex(x => x.IsActive);

            // Relación con Products
            builder.HasMany(x => x.Products)
                .WithOne(x => x.BrandNavigation)
                .HasForeignKey(x => x.BrandId)
                .OnDelete(DeleteBehavior.SetNull); // Si se elimina una marca, los productos quedan sin marca
        }
    }
}

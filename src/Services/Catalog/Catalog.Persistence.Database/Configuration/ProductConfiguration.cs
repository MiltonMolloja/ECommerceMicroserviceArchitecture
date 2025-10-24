using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(x => x.ProductId);

            // Multiidioma
            builder.Property(x => x.NameSpanish)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.NameEnglish)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.DescriptionSpanish)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(x => x.DescriptionEnglish)
                .IsRequired()
                .HasMaxLength(1000);

            // Identificación
            builder.Property(x => x.SKU)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.SKU)
                .IsUnique();

            builder.Property(x => x.Brand)
                .HasMaxLength(100);

            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            // Pricing
            builder.Property(x => x.Price)
                .IsRequired()
                .HasPrecision(18, 2);

            builder.Property(x => x.OriginalPrice)
                .HasPrecision(18, 2);

            builder.Property(x => x.DiscountPercentage)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            builder.Property(x => x.TaxRate)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            // Media
            builder.Property(x => x.Images)
                .HasMaxLength(4000); // Múltiples URLs

            // SEO
            builder.Property(x => x.MetaTitle)
                .HasMaxLength(100);

            builder.Property(x => x.MetaDescription)
                .HasMaxLength(300);

            builder.Property(x => x.MetaKeywords)
                .HasMaxLength(500);

            // Flags
            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.IsFeatured)
                .IsRequired()
                .HasDefaultValue(false);

            // Auditoría
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Índices adicionales
            builder.HasIndex(x => x.Brand);
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.IsFeatured);
            builder.HasIndex(x => new { x.IsActive, x.IsFeatured });

            // Query Filter Global (solo mostrar activos por defecto)
            builder.HasQueryFilter(p => p.IsActive);

            // Ignorar propiedades calculadas
            builder.Ignore(x => x.FinalPrice);
            builder.Ignore(x => x.HasDiscount);
            builder.Ignore(x => x.PriceWithTax);
            builder.Ignore(x => x.ImageUrls);
            builder.Ignore(x => x.PrimaryImageUrl);

            // Relación con Stock (1:1)
            builder.HasOne(x => x.Stock)
                .WithOne(s => s.Product)
                .HasForeignKey<ProductInStock>(s => s.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

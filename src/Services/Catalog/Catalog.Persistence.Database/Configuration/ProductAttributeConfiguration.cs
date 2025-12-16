using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductAttributeConfiguration : IEntityTypeConfiguration<ProductAttribute>
    {
        public void Configure(EntityTypeBuilder<ProductAttribute> builder)
        {
            builder.ToTable("ProductAttributes");

            builder.HasKey(x => x.AttributeId);

            builder.Property(x => x.AttributeName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.AttributeNameEnglish)
                .HasMaxLength(100);

            builder.Property(x => x.AttributeType)
                .IsRequired()
                .HasMaxLength(20); // 'Text', 'Number', 'Boolean', 'Select', 'MultiSelect'

            builder.Property(x => x.Unit)
                .HasMaxLength(20); // 'inches', 'GB', 'MP', 'Hz', etc.

            builder.Property(x => x.IsFilterable)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.IsSearchable)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Relación con Category (opcional)
            builder.HasOne(x => x.Category)
                .WithMany()
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Índices
            builder.HasIndex(x => x.AttributeName);
            builder.HasIndex(x => new { x.CategoryId, x.IsFilterable });
        }
    }
}

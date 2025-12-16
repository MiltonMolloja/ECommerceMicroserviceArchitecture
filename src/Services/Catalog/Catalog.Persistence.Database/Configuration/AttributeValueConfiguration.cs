using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Database.Configuration
{
    public class AttributeValueConfiguration : IEntityTypeConfiguration<AttributeValue>
    {
        public void Configure(EntityTypeBuilder<AttributeValue> builder)
        {
            builder.ToTable("AttributeValues");

            builder.HasKey(x => x.ValueId);

            builder.Property(x => x.ValueText)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ValueTextEnglish)
                .HasMaxLength(200);

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Relación con ProductAttribute
            builder.HasOne(x => x.ProductAttribute)
                .WithMany(x => x.AttributeValues)
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(x => x.AttributeId);
            builder.HasIndex(x => new { x.AttributeId, x.DisplayOrder });
        }
    }
}

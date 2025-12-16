using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductAttributeValueConfiguration : IEntityTypeConfiguration<ProductAttributeValue>
    {
        public void Configure(EntityTypeBuilder<ProductAttributeValue> builder)
        {
            builder.ToTable("ProductAttributeValues");

            // Clave primaria compuesta
            builder.HasKey(x => new { x.ProductId, x.AttributeId, x.ValueId });

            // Propiedades opcionales para diferentes tipos de atributos
            builder.Property(x => x.TextValue)
                .HasMaxLength(500);

            builder.Property(x => x.NumericValue)
                .HasColumnType("decimal(18,4)");

            // Relación con Product
            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductAttributeValues)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con ProductAttribute
            builder.HasOne(x => x.ProductAttribute)
                .WithMany(x => x.ProductAttributeValues)
                .HasForeignKey(x => x.AttributeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación con AttributeValue (opcional)
            builder.HasOne(x => x.AttributeValue)
                .WithMany(x => x.ProductAttributeValues)
                .HasForeignKey(x => x.ValueId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para filtrado rápido
            builder.HasIndex(x => new { x.AttributeId, x.ValueId })
                .HasDatabaseName("IX_ProductAttributeValues_Attribute_Value");

            builder.HasIndex(x => new { x.AttributeId, x.NumericValue })
                .HasDatabaseName("IX_ProductAttributeValues_Attribute_Numeric")
                .HasFilter("[NumericValue] IS NOT NULL");
        }
    }
}

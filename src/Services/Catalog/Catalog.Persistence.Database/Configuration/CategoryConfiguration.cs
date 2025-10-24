using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain;

namespace Catalog.Persistence.Database.Configuration
{
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(x => x.CategoryId);

            // Multiidioma
            builder.Property(x => x.NameSpanish)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.NameEnglish)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.DescriptionSpanish)
                .HasMaxLength(500);

            builder.Property(x => x.DescriptionEnglish)
                .HasMaxLength(500);

            // Slug
            builder.Property(x => x.Slug)
                .IsRequired()
                .HasMaxLength(200);

            builder.HasIndex(x => x.Slug)
                .IsUnique();

            // Flags
            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            // Índices
            builder.HasIndex(x => x.IsActive);
            builder.HasIndex(x => x.DisplayOrder);
            builder.HasIndex(x => x.ParentCategoryId);

            // Auto-referencia para jerarquía
            builder.HasOne(x => x.ParentCategory)
                .WithMany(x => x.SubCategories)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict); // No eliminar padre si tiene hijos

            // Query Filter Global
            builder.HasQueryFilter(c => c.IsActive);
        }
    }
}

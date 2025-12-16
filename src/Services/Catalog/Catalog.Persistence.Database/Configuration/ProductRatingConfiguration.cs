using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductRatingConfiguration : IEntityTypeConfiguration<ProductRating>
    {
        public void Configure(EntityTypeBuilder<ProductRating> builder)
        {
            builder.ToTable("ProductRatings");

            builder.HasKey(x => x.ProductId);

            builder.Property(x => x.AverageRating)
                .IsRequired()
                .HasColumnType("decimal(3,2)")
                .HasDefaultValue(0);

            builder.Property(x => x.TotalReviews)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Rating5Star)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Rating4Star)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Rating3Star)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Rating2Star)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Rating1Star)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.LastUpdated)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relación con Product (1:1)
            builder.HasOne(x => x.Product)
                .WithOne(x => x.ProductRating)
                .HasForeignKey<ProductRating>(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para búsqueda por rating
            builder.HasIndex(x => x.AverageRating)
                .HasDatabaseName("IX_ProductRatings_Rating")
                .IsDescending();
        }
    }
}

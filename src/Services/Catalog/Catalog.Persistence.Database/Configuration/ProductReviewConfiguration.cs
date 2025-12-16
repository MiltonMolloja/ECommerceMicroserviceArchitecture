using Catalog.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Catalog.Persistence.Database.Configuration
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductReviews");

            builder.HasKey(x => x.ReviewId);

            builder.Property(x => x.Rating)
                .IsRequired()
                .HasColumnType("decimal(2,1)");

            builder.Property(x => x.Title)
                .HasMaxLength(200);

            builder.Property(x => x.Comment)
                .HasColumnType("nvarchar(max)");

            builder.Property(x => x.IsVerifiedPurchase)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.HelpfulCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.NotHelpfulCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.IsApproved)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETDATE()");

            // Relación con Product
            builder.HasOne(x => x.Product)
                .WithMany(x => x.ProductReviews)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices
            builder.HasIndex(x => new { x.ProductId, x.Rating, x.IsApproved })
                .HasDatabaseName("IX_ProductReviews_Product_Rating");

            builder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_ProductReviews_User");

            builder.HasIndex(x => x.CreatedAt)
                .HasDatabaseName("IX_ProductReviews_CreatedAt");

            // Check constraint para Rating
            builder.HasCheckConstraint("CK_ProductReviews_Rating", "[Rating] >= 1.0 AND [Rating] <= 5.0");
        }
    }
}

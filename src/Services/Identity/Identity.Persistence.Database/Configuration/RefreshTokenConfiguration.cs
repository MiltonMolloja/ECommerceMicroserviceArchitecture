using Identity.Domain;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Database.Configuration
{
    public class RefreshTokenConfiguration
    {
        public RefreshTokenConfiguration(EntityTypeBuilder<RefreshToken> entityBuilder)
        {
            entityBuilder.HasKey(x => x.Id);

            entityBuilder.Property(x => x.Token)
                .IsRequired()
                .HasMaxLength(500);

            entityBuilder.Property(x => x.UserId)
                .IsRequired();

            entityBuilder.Property(x => x.CreatedAt)
                .IsRequired();

            entityBuilder.Property(x => x.ExpiresAt)
                .IsRequired();

            entityBuilder.Property(x => x.CreatedByIp)
                .HasMaxLength(50);

            entityBuilder.Property(x => x.RevokedByIp)
                .HasMaxLength(50);

            entityBuilder.Property(x => x.ReplacedByToken)
                .HasMaxLength(500);

            // Relationship with ApplicationUser
            entityBuilder.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(Microsoft.EntityFrameworkCore.DeleteBehavior.Cascade);

            // Index for faster queries
            entityBuilder.HasIndex(x => x.Token);
            entityBuilder.HasIndex(x => x.UserId);
        }
    }
}

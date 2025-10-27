using Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Database.Configuration
{
    public class UserBackupCodeConfiguration
    {
        public UserBackupCodeConfiguration(EntityTypeBuilder<UserBackupCode> entityBuilder)
        {
            entityBuilder.HasKey(x => x.Id);

            entityBuilder.Property(x => x.UserId).IsRequired();
            entityBuilder.Property(x => x.CodeHash).IsRequired().HasMaxLength(256);
            entityBuilder.Property(x => x.IsUsed).IsRequired();
            entityBuilder.Property(x => x.CreatedAt).IsRequired();

            // Relationship
            entityBuilder.HasOne(x => x.User)
                        .WithMany(u => u.BackupCodes)
                        .HasForeignKey(x => x.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            // Index for faster lookups
            entityBuilder.HasIndex(x => new { x.UserId, x.IsUsed });
        }
    }
}

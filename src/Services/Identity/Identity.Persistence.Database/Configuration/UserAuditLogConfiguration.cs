using Identity.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Identity.Persistence.Database.Configuration
{
    public class UserAuditLogConfiguration
    {
        public UserAuditLogConfiguration(EntityTypeBuilder<UserAuditLog> entityBuilder)
        {
            entityBuilder.HasKey(x => x.Id);

            entityBuilder.Property(x => x.UserId).IsRequired();
            entityBuilder.Property(x => x.Action).IsRequired().HasMaxLength(100);
            entityBuilder.Property(x => x.Timestamp).IsRequired();
            entityBuilder.Property(x => x.IpAddress).HasMaxLength(50);
            entityBuilder.Property(x => x.UserAgent).HasMaxLength(500);
            entityBuilder.Property(x => x.Success).IsRequired();
            entityBuilder.Property(x => x.FailureReason).HasMaxLength(500);

            // Relationship
            entityBuilder.HasOne(x => x.User)
                        .WithMany(u => u.AuditLogs)
                        .HasForeignKey(x => x.UserId)
                        .OnDelete(DeleteBehavior.Cascade);

            // Indexes for faster queries
            entityBuilder.HasIndex(x => x.UserId);
            entityBuilder.HasIndex(x => x.Timestamp);
            entityBuilder.HasIndex(x => new { x.UserId, x.Timestamp });
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Notification.Persistence.Database.Configuration
{
    public class NotificationConfiguration : IEntityTypeConfiguration<Domain.Notification>
    {
        public void Configure(EntityTypeBuilder<Domain.Notification> builder)
        {
            builder.ToTable("Notifications");
            builder.HasKey(x => x.NotificationId);

            // UserId
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.HasIndex(x => x.UserId);

            // Type
            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(x => x.Type);

            // Title
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Message
            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(1000);

            // Data (JSON)
            builder.Property(x => x.Data)
                .HasMaxLength(4000); // Para metadata en JSON

            // IsRead
            builder.Property(x => x.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            builder.HasIndex(x => x.IsRead);

            // ReadAt
            builder.Property(x => x.ReadAt)
                .IsRequired(false);

            // Priority
            builder.Property(x => x.Priority)
                .IsRequired()
                .HasConversion<int>()
                .HasDefaultValue(Domain.NotificationPriority.Normal);

            // ExpiresAt
            builder.Property(x => x.ExpiresAt)
                .IsRequired(false);

            builder.HasIndex(x => x.ExpiresAt);

            // CreatedAt
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.HasIndex(x => x.CreatedAt);

            // Índices compuestos para consultas comunes
            builder.HasIndex(x => new { x.UserId, x.IsRead, x.CreatedAt });
            builder.HasIndex(x => new { x.UserId, x.Type });

            // Ignorar propiedades calculadas
            builder.Ignore(x => x.IsExpired);
            builder.Ignore(x => x.IsActive);
            builder.Ignore(x => x.TimeToExpire);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain;

namespace Notification.Persistence.Database.Configuration
{
    public class NotificationPreferencesConfiguration : IEntityTypeConfiguration<NotificationPreferences>
    {
        public void Configure(EntityTypeBuilder<NotificationPreferences> builder)
        {
            builder.ToTable("NotificationPreferences");
            builder.HasKey(x => x.PreferenceId);

            // UserId (UNIQUE - cada usuario solo tiene un registro de preferencias)
            builder.Property(x => x.UserId)
                .IsRequired();

            builder.HasIndex(x => x.UserId)
                .IsUnique();

            // Canales
            builder.Property(x => x.EmailNotifications)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.PushNotifications)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.SMSNotifications)
                .IsRequired()
                .HasDefaultValue(false);

            // Tipos de notificación
            builder.Property(x => x.OrderUpdates)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.Promotions)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.Newsletter)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.PriceAlerts)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(x => x.StockAlerts)
                .IsRequired()
                .HasDefaultValue(false);

            // CreatedAt
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // UpdatedAt
            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
        }
    }
}

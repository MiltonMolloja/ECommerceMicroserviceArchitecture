using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Notification.Domain;

namespace Notification.Persistence.Database.Configuration
{
    public class NotificationTemplateConfiguration : IEntityTypeConfiguration<NotificationTemplate>
    {
        public void Configure(EntityTypeBuilder<NotificationTemplate> builder)
        {
            builder.ToTable("NotificationTemplates");
            builder.HasKey(x => x.TemplateId);

            // Type
            builder.Property(x => x.Type)
                .IsRequired()
                .HasConversion<int>();

            builder.HasIndex(x => x.Type);

            // TemplateKey
            builder.Property(x => x.TemplateKey)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(x => x.TemplateKey)
                .IsUnique();

            // TitleTemplate
            builder.Property(x => x.TitleTemplate)
                .IsRequired()
                .HasMaxLength(200);

            // MessageTemplate
            builder.Property(x => x.MessageTemplate)
                .IsRequired()
                .HasMaxLength(1000);

            // Channel
            builder.Property(x => x.Channel)
                .IsRequired()
                .HasConversion<int>();

            // IsActive
            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // CreatedAt
            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // UpdatedAt
            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            // Query Filter Global (solo mostrar activos por defecto)
            builder.HasQueryFilter(t => t.IsActive);
        }
    }
}

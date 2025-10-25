using Microsoft.EntityFrameworkCore;
using Notification.Domain;
using Notification.Persistence.Database.Configuration;

namespace Notification.Persistence.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options
        )
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
            optionsBuilder.ConfigureWarnings(warnings =>
                warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Database schema
            builder.HasDefaultSchema("Notification");

            // Apply configurations
            builder.ApplyConfiguration(new NotificationConfiguration());
            builder.ApplyConfiguration(new NotificationTemplateConfiguration());
            builder.ApplyConfiguration(new NotificationPreferencesConfiguration());
        }

        public DbSet<Domain.Notification> Notifications { get; set; }
        public DbSet<NotificationTemplate> NotificationTemplates { get; set; }
        public DbSet<NotificationPreferences> NotificationPreferences { get; set; }
    }
}

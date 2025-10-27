using Identity.Domain;
using Identity.Persistence.Database.Configuration;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Identity.Persistence.Database
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
    {
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<UserBackupCode> UserBackupCodes { get; set; }
        public DbSet<UserAuditLog> UserAuditLogs { get; set; }

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
            builder.HasDefaultSchema("Identity");

            // Model Contraints
            ModelConfig(builder);
        }

        private void ModelConfig(ModelBuilder modelBuilder)
        {
            new ApplicationUserConfiguration(modelBuilder.Entity<ApplicationUser>());
            new ApplicationRoleConfiguration(modelBuilder.Entity<ApplicationRole>());
            new RefreshTokenConfiguration(modelBuilder.Entity<RefreshToken>());
            new UserBackupCodeConfiguration(modelBuilder.Entity<UserBackupCode>());
            new UserAuditLogConfiguration(modelBuilder.Entity<UserAuditLog>());
        }
    }
}

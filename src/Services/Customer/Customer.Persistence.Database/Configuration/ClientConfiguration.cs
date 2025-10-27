using Customer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Order.Persistence.Database.Configuration
{
    public class ClientConfiguration
    {
        public ClientConfiguration(EntityTypeBuilder<Client> entityBuilder)
        {
            // Table configuration
            entityBuilder.ToTable("Clients", "Customer");

            // Primary Key
            entityBuilder.HasKey(x => x.ClientId);

            // Identificación
            entityBuilder.Property(x => x.ClientId)
                .ValueGeneratedOnAdd();

            entityBuilder.Property(x => x.UserId)
                .HasMaxLength(450); // Same as AspNetUsers.Id

            // Información Personal
            // NOTA: FirstName, LastName, Email eliminados - se obtienen de Identity.AspNetUsers

            entityBuilder.Property(x => x.Phone)
                .HasMaxLength(20);

            entityBuilder.Property(x => x.MobilePhone)
                .HasMaxLength(20);

            entityBuilder.Property(x => x.Gender)
                .HasMaxLength(20);

            entityBuilder.Property(x => x.ProfileImageUrl)
                .HasMaxLength(500);

            // Preferencias
            entityBuilder.Property(x => x.PreferredLanguage)
                .HasMaxLength(10)
                .HasDefaultValue("es");

            entityBuilder.Property(x => x.PreferredCurrency)
                .HasMaxLength(3)
                .HasDefaultValue("USD");

            entityBuilder.Property(x => x.NewsletterSubscribed)
                .HasDefaultValue(false);

            entityBuilder.Property(x => x.SmsNotificationsEnabled)
                .HasDefaultValue(true);

            entityBuilder.Property(x => x.EmailNotificationsEnabled)
                .HasDefaultValue(true);

            // Estado
            entityBuilder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entityBuilder.Property(x => x.IsEmailVerified)
                .HasDefaultValue(false);

            entityBuilder.Property(x => x.IsPhoneVerified)
                .HasDefaultValue(false);

            // Auditoría
            entityBuilder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entityBuilder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            // Email index eliminado - el campo ya no existe aquí

            entityBuilder.HasIndex(x => x.UserId)
                .HasDatabaseName("IX_Clients_UserId");

            // Relationships
            entityBuilder.HasMany(x => x.Addresses)
                .WithOne(x => x.Client)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            entityBuilder.Ignore(x => x.Age);

            // Seed Data removed to avoid migration conflicts
            // Will be added via separate SQL script or API
        }
    }
}

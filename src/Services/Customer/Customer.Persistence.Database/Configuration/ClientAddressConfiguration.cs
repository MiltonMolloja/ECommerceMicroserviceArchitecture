using Customer.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;

namespace Order.Persistence.Database.Configuration
{
    public class ClientAddressConfiguration
    {
        public ClientAddressConfiguration(EntityTypeBuilder<ClientAddress> entityBuilder)
        {
            // Table configuration
            entityBuilder.ToTable("ClientAddresses", "Customer");

            // Primary Key
            entityBuilder.HasKey(x => x.AddressId);

            // Properties
            entityBuilder.Property(x => x.AddressId)
                .ValueGeneratedOnAdd();

            entityBuilder.Property(x => x.AddressType)
                .IsRequired()
                .HasMaxLength(20); // "Shipping", "Billing", "Both"

            entityBuilder.Property(x => x.AddressName)
                .HasMaxLength(50);

            entityBuilder.Property(x => x.RecipientName)
                .IsRequired()
                .HasMaxLength(200);

            entityBuilder.Property(x => x.RecipientPhone)
                .HasMaxLength(20);

            entityBuilder.Property(x => x.AddressLine1)
                .IsRequired()
                .HasMaxLength(200);

            entityBuilder.Property(x => x.AddressLine2)
                .HasMaxLength(200);

            entityBuilder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(100);

            entityBuilder.Property(x => x.State)
                .HasMaxLength(100);

            entityBuilder.Property(x => x.PostalCode)
                .IsRequired()
                .HasMaxLength(20);

            entityBuilder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(100);

            entityBuilder.Property(x => x.IsDefaultShipping)
                .HasDefaultValue(false);

            entityBuilder.Property(x => x.IsDefaultBilling)
                .HasDefaultValue(false);

            entityBuilder.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entityBuilder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entityBuilder.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            // Indexes
            entityBuilder.HasIndex(x => new { x.ClientId, x.IsDefaultShipping })
                .HasDatabaseName("IX_ClientAddresses_ClientId_DefaultShipping");

            entityBuilder.HasIndex(x => new { x.ClientId, x.IsDefaultBilling })
                .HasDatabaseName("IX_ClientAddresses_ClientId_DefaultBilling");

            // Relationships
            entityBuilder.HasOne(x => x.Client)
                .WithMany(x => x.Addresses)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Cascade);

            // Ignore computed properties
            entityBuilder.Ignore(x => x.FullAddress);
            entityBuilder.Ignore(x => x.ShortAddress);

            // Seed Data removed to avoid migration conflicts
            // Will be added via separate SQL script or API
        }
    }
}

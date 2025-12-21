using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Catalog.Domain;

namespace Catalog.Persistence.Database.Configuration
{
    /// <summary>
    /// Configuración de Entity Framework Core para la entidad Banner
    /// </summary>
    public class BannerConfiguration : IEntityTypeConfiguration<Banner>
    {
        public void Configure(EntityTypeBuilder<Banner> builder)
        {
            builder.ToTable("Banners");
            builder.HasKey(x => x.BannerId);

            #region Contenido Multiidioma

            builder.Property(x => x.TitleSpanish)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.TitleEnglish)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.SubtitleSpanish)
                .HasMaxLength(500);

            builder.Property(x => x.SubtitleEnglish)
                .HasMaxLength(500);

            #endregion

            #region Media

            builder.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(x => x.ImageUrlMobile)
                .HasMaxLength(500);

            #endregion

            #region Navegación

            builder.Property(x => x.LinkUrl)
                .HasMaxLength(500);

            builder.Property(x => x.ButtonTextSpanish)
                .HasMaxLength(100);

            builder.Property(x => x.ButtonTextEnglish)
                .HasMaxLength(100);

            #endregion

            #region Configuración

            builder.Property(x => x.DisplayOrder)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.Position)
                .IsRequired()
                .HasMaxLength(50)
                .HasDefaultValue("hero");

            #endregion

            #region Vigencia

            builder.Property(x => x.StartDate)
                .IsRequired(false);

            builder.Property(x => x.EndDate)
                .IsRequired(false);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            #endregion

            #region Auditoría

            builder.Property(x => x.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            builder.Property(x => x.UpdatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            #endregion

            #region Índices

            // Índice compuesto para búsquedas frecuentes
            // Note: IncludeProperties removed for cross-database compatibility (SQL Server/PostgreSQL)
            builder.HasIndex(x => new { x.Position, x.IsActive })
                .HasDatabaseName("IX_Banners_Position_Active");

            // Índice para ordenamiento
            builder.HasIndex(x => x.DisplayOrder)
                .HasDatabaseName("IX_Banners_DisplayOrder");

            // Índice para vigencia
            builder.HasIndex(x => new { x.StartDate, x.EndDate })
                .HasDatabaseName("IX_Banners_Dates");

            #endregion

            #region Computed Properties

            // Ignorar propiedades calculadas
            builder.Ignore(x => x.IsCurrentlyActive);

            #endregion
        }
    }
}

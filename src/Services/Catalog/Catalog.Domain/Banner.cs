using System;

namespace Catalog.Domain
{
    /// <summary>
    /// Entidad que representa un banner para el hero section de la página Home.
    /// Soporta multiidioma y vigencia temporal.
    /// </summary>
    public class Banner
    {
        /// <summary>
        /// Identificador único del banner
        /// </summary>
        public int BannerId { get; set; }

        #region Contenido Multiidioma

        /// <summary>
        /// Título del banner en español
        /// </summary>
        public string TitleSpanish { get; set; } = string.Empty;

        /// <summary>
        /// Título del banner en inglés
        /// </summary>
        public string TitleEnglish { get; set; } = string.Empty;

        /// <summary>
        /// Subtítulo del banner en español
        /// </summary>
        public string? SubtitleSpanish { get; set; }

        /// <summary>
        /// Subtítulo del banner en inglés
        /// </summary>
        public string? SubtitleEnglish { get; set; }

        #endregion

        #region Media

        /// <summary>
        /// URL de la imagen principal del banner (desktop)
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL de la imagen para dispositivos móviles (opcional)
        /// </summary>
        public string? ImageUrlMobile { get; set; }

        #endregion

        #region Navegación

        /// <summary>
        /// URL de destino al hacer clic en el banner
        /// </summary>
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Texto del botón de acción en español
        /// </summary>
        public string? ButtonTextSpanish { get; set; }

        /// <summary>
        /// Texto del botón de acción en inglés
        /// </summary>
        public string? ButtonTextEnglish { get; set; }

        #endregion

        #region Configuración

        /// <summary>
        /// Orden de visualización (menor número = mayor prioridad)
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// Posición del banner en la página (hero, sidebar, footer, etc.)
        /// </summary>
        public string Position { get; set; } = "hero";

        #endregion

        #region Vigencia

        /// <summary>
        /// Fecha de inicio de vigencia (null = sin límite de inicio)
        /// </summary>
        public DateTime? StartDate { get; set; }

        /// <summary>
        /// Fecha de fin de vigencia (null = sin límite de fin)
        /// </summary>
        public DateTime? EndDate { get; set; }

        /// <summary>
        /// Indica si el banner está activo
        /// </summary>
        public bool IsActive { get; set; } = true;

        #endregion

        #region Auditoría

        /// <summary>
        /// Fecha de creación del banner
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Fecha de última actualización
        /// </summary>
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        #endregion

        #region Computed Properties

        /// <summary>
        /// Indica si el banner está actualmente vigente
        /// </summary>
        public bool IsCurrentlyActive =>
            IsActive &&
            (!StartDate.HasValue || StartDate.Value <= DateTime.UtcNow) &&
            (!EndDate.HasValue || EndDate.Value >= DateTime.UtcNow);

        #endregion
    }
}

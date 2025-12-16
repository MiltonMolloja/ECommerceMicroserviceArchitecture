namespace Catalog.Service.Queries.DTOs
{
    /// <summary>
    /// DTO para representar un banner en las respuestas de la API
    /// </summary>
    public class BannerDto
    {
        /// <summary>
        /// Identificador único del banner
        /// </summary>
        public int BannerId { get; set; }

        /// <summary>
        /// Título del banner (localizado según idioma de la petición)
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Subtítulo del banner (localizado según idioma de la petición)
        /// </summary>
        public string? Subtitle { get; set; }

        /// <summary>
        /// URL de la imagen principal (desktop)
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL de la imagen para móviles (opcional)
        /// </summary>
        public string? ImageUrlMobile { get; set; }

        /// <summary>
        /// URL de destino al hacer clic
        /// </summary>
        public string? LinkUrl { get; set; }

        /// <summary>
        /// Texto del botón de acción (localizado)
        /// </summary>
        public string? ButtonText { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}

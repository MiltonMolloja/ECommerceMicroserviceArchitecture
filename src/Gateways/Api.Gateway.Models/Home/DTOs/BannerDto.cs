namespace Api.Gateway.Models.Home.DTOs
{
    /// <summary>
    /// DTO para banners del hero section
    /// </summary>
    public class BannerDto
    {
        /// <summary>
        /// Identificador único del banner
        /// </summary>
        public int BannerId { get; set; }

        /// <summary>
        /// Título del banner (localizado según Accept-Language)
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Subtítulo del banner (localizado según Accept-Language)
        /// </summary>
        public string Subtitle { get; set; }

        /// <summary>
        /// URL de la imagen principal (desktop)
        /// </summary>
        public string ImageUrl { get; set; }

        /// <summary>
        /// URL de la imagen para móvil (opcional)
        /// </summary>
        public string ImageUrlMobile { get; set; }

        /// <summary>
        /// URL de destino al hacer clic
        /// </summary>
        public string LinkUrl { get; set; }

        /// <summary>
        /// Texto del botón CTA (localizado según Accept-Language)
        /// </summary>
        public string ButtonText { get; set; }

        /// <summary>
        /// Orden de visualización
        /// </summary>
        public int DisplayOrder { get; set; }
    }
}

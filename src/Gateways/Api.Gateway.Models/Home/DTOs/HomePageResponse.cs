using System;
using System.Collections.Generic;
using Api.Gateway.Models.Catalog.DTOs;

namespace Api.Gateway.Models.Home.DTOs
{
    /// <summary>
    /// Respuesta agregada para la página Home.
    /// Contiene todas las secciones necesarias para renderizar la página.
    /// </summary>
    public class HomePageResponse
    {
        /// <summary>
        /// Banners del hero section
        /// </summary>
        public List<BannerDto> Banners { get; set; } = new();

        /// <summary>
        /// Productos destacados (IsFeatured = true)
        /// </summary>
        public List<ProductDto> FeaturedProducts { get; set; } = new();

        /// <summary>
        /// Ofertas del día (productos con descuento)
        /// </summary>
        public List<ProductDto> Deals { get; set; } = new();

        /// <summary>
        /// Productos más vendidos (ordenados por TotalSold)
        /// </summary>
        public List<ProductDto> BestSellers { get; set; } = new();

        /// <summary>
        /// Novedades (productos recientes)
        /// </summary>
        public List<ProductDto> NewArrivals { get; set; } = new();

        /// <summary>
        /// Productos mejor valorados
        /// </summary>
        public List<ProductDto> TopRated { get; set; } = new();

        /// <summary>
        /// Categorías destacadas
        /// </summary>
        public List<CategoryDto> FeaturedCategories { get; set; } = new();

        /// <summary>
        /// Metadata de la respuesta
        /// </summary>
        public HomeMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// Metadata de la respuesta de Home
    /// </summary>
    public class HomeMetadata
    {
        /// <summary>
        /// Idioma de la respuesta
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Fecha/hora de generación
        /// </summary>
        public DateTime GeneratedAt { get; set; }

        /// <summary>
        /// Duración del cache en segundos
        /// </summary>
        public int CacheDurationSeconds { get; set; }

        /// <summary>
        /// Tiempo de ejecución de la query en ms
        /// </summary>
        public long QueryExecutionTimeMs { get; set; }

        /// <summary>
        /// Indica si la respuesta viene del cache
        /// </summary>
        public bool FromCache { get; set; }
    }
}

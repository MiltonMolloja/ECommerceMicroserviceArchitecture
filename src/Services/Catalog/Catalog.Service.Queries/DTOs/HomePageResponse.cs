using System;
using System.Collections.Generic;

namespace Catalog.Service.Queries.DTOs
{
    /// <summary>
    /// Respuesta agregada para la página Home.
    /// Contiene todas las secciones necesarias para renderizar la página inicial.
    /// </summary>
    public class HomePageResponse
    {
        /// <summary>
        /// Banners para el hero carousel
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
        /// Novedades (productos recientes ordenados por CreatedAt)
        /// </summary>
        public List<ProductDto> NewArrivals { get; set; } = new();

        /// <summary>
        /// Productos mejor valorados (rating >= 4)
        /// </summary>
        public List<ProductDto> TopRated { get; set; } = new();

        /// <summary>
        /// Categorías destacadas para mostrar en Home
        /// </summary>
        public List<CategoryDto> FeaturedCategories { get; set; } = new();

        /// <summary>
        /// Metadata de la respuesta
        /// </summary>
        public HomeMetadata Metadata { get; set; } = new();
    }

    /// <summary>
    /// Metadata sobre la generación de la respuesta Home
    /// </summary>
    public class HomeMetadata
    {
        /// <summary>
        /// Fecha y hora de generación de la respuesta
        /// </summary>
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Idioma de la respuesta (es, en)
        /// </summary>
        public string Language { get; set; } = "es";

        /// <summary>
        /// Duración del cache en segundos
        /// </summary>
        public int CacheDurationSeconds { get; set; } = 300;

        /// <summary>
        /// Tiempo de ejecución de las queries en milisegundos
        /// </summary>
        public long QueryExecutionTimeMs { get; set; }

        /// <summary>
        /// Indica si la respuesta proviene del cache
        /// </summary>
        public bool FromCache { get; set; }
    }
}

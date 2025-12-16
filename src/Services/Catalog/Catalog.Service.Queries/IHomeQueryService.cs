using System.Collections.Generic;
using System.Threading.Tasks;
using Catalog.Service.Queries.DTOs;

namespace Catalog.Service.Queries
{
    /// <summary>
    /// Servicio de consultas para la página Home.
    /// Implementa arquitectura híbrida: endpoint agregador + endpoints individuales.
    /// </summary>
    public interface IHomeQueryService
    {
        #region Endpoint Agregador

        /// <summary>
        /// Obtiene todos los datos necesarios para la página Home en una sola llamada.
        /// Ejecuta todas las queries en paralelo para optimizar performance.
        /// </summary>
        /// <param name="language">Idioma de la respuesta (es, en)</param>
        /// <param name="productsPerSection">Cantidad de productos por sección (default: 8)</param>
        /// <returns>Respuesta agregada con todas las secciones de Home</returns>
        Task<HomePageResponse> GetHomePageDataAsync(string language, int productsPerSection = 8);

        #endregion

        #region Endpoints Individuales

        /// <summary>
        /// Obtiene los banners activos para una posición específica.
        /// Cache: 10 minutos (banners cambian poco)
        /// </summary>
        /// <param name="position">Posición del banner (hero, sidebar, footer)</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de banners activos y vigentes</returns>
        Task<List<BannerDto>> GetBannersAsync(string position, string language);

        /// <summary>
        /// Obtiene productos destacados (IsFeatured = true).
        /// Cache: 5 minutos
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de productos destacados</returns>
        Task<List<ProductDto>> GetFeaturedProductsAsync(int limit, string language);

        /// <summary>
        /// Obtiene ofertas del día (productos con descuento).
        /// Cache: 1 minuto (ofertas cambian frecuentemente)
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de productos en oferta ordenados por descuento</returns>
        Task<List<ProductDto>> GetDealsAsync(int limit, string language);

        /// <summary>
        /// Obtiene productos más vendidos (ordenados por TotalSold).
        /// Cache: 5 minutos
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de productos más vendidos</returns>
        Task<List<ProductDto>> GetBestSellersAsync(int limit, string language);

        /// <summary>
        /// Obtiene novedades (productos recientes).
        /// Cache: 5 minutos
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de productos ordenados por fecha de creación</returns>
        Task<List<ProductDto>> GetNewArrivalsAsync(int limit, string language);

        /// <summary>
        /// Obtiene productos mejor valorados.
        /// Cache: 5 minutos
        /// </summary>
        /// <param name="limit">Cantidad máxima de productos</param>
        /// <param name="minRating">Rating mínimo (default: 4.0)</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de productos con mejor rating</returns>
        Task<List<ProductDto>> GetTopRatedAsync(int limit, decimal minRating, string language);

        /// <summary>
        /// Obtiene categorías destacadas para mostrar en Home.
        /// Cache: 10 minutos
        /// </summary>
        /// <param name="limit">Cantidad máxima de categorías</param>
        /// <param name="language">Idioma de la respuesta</param>
        /// <returns>Lista de categorías destacadas</returns>
        Task<List<CategoryDto>> GetFeaturedCategoriesAsync(int limit, string language);

        #endregion
    }
}

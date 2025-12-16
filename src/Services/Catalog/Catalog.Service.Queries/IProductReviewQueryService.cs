using Catalog.Service.Queries.DTOs;
using System.Threading.Tasks;

namespace Catalog.Service.Queries
{
    public interface IProductReviewQueryService
    {
        /// <summary>
        /// Obtiene las reviews de un producto con paginación y filtros
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="sortBy">Ordenamiento</param>
        /// <param name="verifiedOnly">Filtrar solo compras verificadas</param>
        Task<ProductReviewsResponse> GetProductReviewsAsync(int productId, int page = 1, int pageSize = 10, string sortBy = "helpful", bool? verifiedOnly = null);

        /// <summary>
        /// Obtiene el resumen de ratings de un producto
        /// </summary>
        Task<ProductRatingSummaryDto> GetProductRatingSummaryAsync(int productId);
    }
}

using Catalog.Service.Queries;
using Catalog.Service.Queries.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Catalog.Api.Controllers
{
    [ApiController]
    [Route("v1/products/{productId}/reviews")]
    public class ProductReviewController : ControllerBase
    {
        private readonly IProductReviewQueryService _reviewQueryService;
        private readonly ILogger<ProductReviewController> _logger;

        public ProductReviewController(
            IProductReviewQueryService reviewQueryService,
            ILogger<ProductReviewController> logger)
        {
            _reviewQueryService = reviewQueryService;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene las reviews de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <param name="page">Número de página</param>
        /// <param name="pageSize">Tamaño de página</param>
        /// <param name="sortBy">Ordenamiento: newest, oldest, rating_high, rating_low, helpful</param>
        /// <param name="verifiedOnly">Filtrar solo compras verificadas</param>
        /// <returns>Lista paginada de reviews con información de rating</returns>
        [HttpGet]
        [ProducesResponseType(typeof(ProductReviewsResponse), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductReviewsResponse>> GetReviews(
            int productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string sortBy = "helpful",
            [FromQuery] bool? verifiedOnly = null)
        {
            try
            {
                // Validaciones
                if (page < 1)
                {
                    return BadRequest(new { message = "Page must be greater than 0" });
                }

                if (pageSize < 1 || pageSize > 100)
                {
                    return BadRequest(new { message = "PageSize must be between 1 and 100" });
                }

                var validSortOptions = new[] { "newest", "oldest", "rating_high", "rating_low", "helpful" };
                if (!string.IsNullOrEmpty(sortBy) && !Array.Exists(validSortOptions, s => s.Equals(sortBy, StringComparison.OrdinalIgnoreCase)))
                {
                    return BadRequest(new { message = $"Invalid sortBy value. Valid options: {string.Join(", ", validSortOptions)}" });
                }

                var result = await _reviewQueryService.GetProductReviewsAsync(productId, page, pageSize, sortBy, verifiedOnly);

                var filterInfo = verifiedOnly.HasValue && verifiedOnly.Value ? " (verified only)" : "";
                _logger.LogInformation($"Retrieved {result.Items.Count} reviews for product {productId} (page {page}){filterInfo}");

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving reviews for product {productId}");
                return StatusCode(500, new { message = "Error retrieving product reviews", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtiene el resumen de ratings de un producto
        /// </summary>
        /// <param name="productId">ID del producto</param>
        /// <returns>Resumen de ratings con distribución</returns>
        [HttpGet("summary")]
        [ProducesResponseType(typeof(ProductRatingSummaryDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(500)]
        public async Task<ActionResult<ProductRatingSummaryDto>> GetRatingSummary(int productId)
        {
            try
            {
                var summary = await _reviewQueryService.GetProductRatingSummaryAsync(productId);

                _logger.LogInformation($"Retrieved rating summary for product {productId}: {summary.AverageRating:F1} stars ({summary.TotalReviews} reviews)");

                return Ok(summary);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error retrieving rating summary for product {productId}");
                return StatusCode(500, new { message = "Error retrieving rating summary", error = ex.Message });
            }
        }
    }
}

using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Service.Queries
{
    public class ProductReviewQueryService : IProductReviewQueryService
    {
        private readonly ApplicationDbContext _context;

        public ProductReviewQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProductReviewsResponse> GetProductReviewsAsync(
            int productId,
            int page = 1,
            int pageSize = 10,
            string sortBy = "helpful",
            bool? verifiedOnly = null)
        {
            // Query base de reviews aprobadas
            var query = _context.ProductReviews
                .Where(r => r.ProductId == productId && r.IsApproved);

            // Filtrar por compras verificadas si se solicita
            if (verifiedOnly.HasValue && verifiedOnly.Value)
            {
                query = query.Where(r => r.IsVerifiedPurchase);
            }

            // Aplicar ordenamiento
            query = sortBy.ToLower() switch
            {
                "newest" => query.OrderByDescending(r => r.CreatedAt),
                "oldest" => query.OrderBy(r => r.CreatedAt),
                "rating_high" => query.OrderByDescending(r => r.Rating),
                "rating_low" => query.OrderBy(r => r.Rating),
                "helpful" or _ => query.OrderByDescending(r => r.HelpfulCount)
                                       .ThenByDescending(r => r.CreatedAt)
            };

            // Contar total
            var total = await query.CountAsync();

            // Paginar
            var reviews = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new ProductReviewDto
                {
                    ReviewId = r.ReviewId,
                    ProductId = r.ProductId,
                    UserId = r.UserId,
                    UserName = $"Usuario{r.UserId}", // TODO: Obtener de servicio de usuarios
                    Rating = r.Rating,
                    Title = r.Title,
                    Comment = r.Comment,
                    IsVerifiedPurchase = r.IsVerifiedPurchase,
                    HelpfulCount = r.HelpfulCount,
                    NotHelpfulCount = r.NotHelpfulCount,
                    CreatedAt = r.CreatedAt,
                    UpdatedAt = r.UpdatedAt
                })
                .ToListAsync();

            // Obtener información de rating
            var rating = await _context.ProductRatings
                .Where(pr => pr.ProductId == productId)
                .FirstOrDefaultAsync();

            return new ProductReviewsResponse
            {
                Items = reviews,
                Total = total,
                AverageRating = rating?.AverageRating ?? 0,
                RatingDistribution = rating != null ? new RatingDistributionDto
                {
                    Rating5Star = rating.Rating5Star,
                    Rating4Star = rating.Rating4Star,
                    Rating3Star = rating.Rating3Star,
                    Rating2Star = rating.Rating2Star,
                    Rating1Star = rating.Rating1Star
                } : new RatingDistributionDto()
            };
        }

        public async Task<ProductRatingSummaryDto> GetProductRatingSummaryAsync(int productId)
        {
            var rating = await _context.ProductRatings
                .Where(pr => pr.ProductId == productId)
                .FirstOrDefaultAsync();

            if (rating == null)
            {
                return new ProductRatingSummaryDto
                {
                    ProductId = productId,
                    AverageRating = 0,
                    TotalReviews = 0,
                    Distribution = new RatingDistributionDto(),
                    RecommendationPercentage = 0
                };
            }

            // Calcular porcentaje de recomendación (4-5 estrellas)
            var recommendationPercentage = rating.TotalReviews > 0
                ? (int)Math.Round(
                    ((decimal)(rating.Rating5Star + rating.Rating4Star) / rating.TotalReviews) * 100
                  )
                : 0;

            return new ProductRatingSummaryDto
            {
                ProductId = productId,
                AverageRating = rating.AverageRating,
                TotalReviews = rating.TotalReviews,
                Distribution = new RatingDistributionDto
                {
                    Rating5Star = rating.Rating5Star,
                    Rating4Star = rating.Rating4Star,
                    Rating3Star = rating.Rating3Star,
                    Rating2Star = rating.Rating2Star,
                    Rating1Star = rating.Rating1Star
                },
                RecommendationPercentage = recommendationPercentage
            };
        }
    }
}

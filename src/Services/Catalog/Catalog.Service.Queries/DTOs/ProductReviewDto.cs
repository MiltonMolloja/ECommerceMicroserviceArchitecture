using System;

namespace Catalog.Service.Queries.DTOs
{
    public class ProductReviewDto
    {
        public long ReviewId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public decimal Rating { get; set; }
        public string Title { get; set; }
        public string Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; }
        public int HelpfulCount { get; set; }
        public int NotHelpfulCount { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class ProductReviewsResponse
    {
        public System.Collections.Generic.List<ProductReviewDto> Items { get; set; }
        public int Total { get; set; }
        public decimal AverageRating { get; set; }
        public RatingDistributionDto RatingDistribution { get; set; }
    }

    public class RatingDistributionDto
    {
        public int Rating5Star { get; set; }
        public int Rating4Star { get; set; }
        public int Rating3Star { get; set; }
        public int Rating2Star { get; set; }
        public int Rating1Star { get; set; }
    }

    public class ProductRatingSummaryDto
    {
        public int ProductId { get; set; }
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public RatingDistributionDto Distribution { get; set; }
        public int RecommendationPercentage { get; set; }
    }
}

using System;

namespace Catalog.Domain
{
    public class ProductReview
    {
        public long ReviewId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; } // Referencia a Customer.API
        public decimal Rating { get; set; } // 1.0 - 5.0
        public string Title { get; set; }
        public string Comment { get; set; }
        public bool IsVerifiedPurchase { get; set; } = false;
        public int HelpfulCount { get; set; } = 0;
        public int NotHelpfulCount { get; set; } = 0;
        public bool IsApproved { get; set; } = false;
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navigation property
        public Product Product { get; set; }
    }
}

using System;
using System.Collections.Generic;

namespace Cart.Service.Queries.DTOs
{
    public class CartDto
    {
        public int CartId { get; set; }
        public int? ClientId { get; set; }
        public string SessionId { get; set; }
        public string Status { get; set; }

        // Coupon
        public string CouponCode { get; set; }
        public decimal CouponDiscountPercentage { get; set; }

        // Items
        public List<CartItemDto> Items { get; set; }

        // Totals (computed)
        public decimal Subtotal { get; set; }
        public decimal CouponDiscount { get; set; }
        public decimal SubtotalAfterCoupon { get; set; }
        public decimal TaxTotal { get; set; }
        public decimal Total { get; set; }
        public int ItemCount { get; set; }
        public int UniqueItemCount { get; set; }

        // Flags
        public bool IsEmpty { get; set; }
        public bool IsAnonymous { get; set; }
        public bool HasCoupon { get; set; }

        // Dates
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }

    public class CartItemDto
    {
        public int CartItemId { get; set; }
        public int CartId { get; set; }
        public int ProductId { get; set; }

        // Product snapshot
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public string ProductImageUrl { get; set; }

        // Pricing
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }

        // Computed
        public decimal UnitPriceAfterDiscount { get; set; }
        public decimal LineTotal { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal LineTotalWithTax { get; set; }
        public decimal TotalSavings { get; set; }
        public bool HasDiscount { get; set; }

        // Dates
        public DateTime AddedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

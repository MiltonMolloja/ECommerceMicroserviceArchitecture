using System;
using System.Collections.Generic;
using static Order.Common.Enums;

namespace Order.Domain
{
    public class Order
    {
        public int OrderId { get; set; }
        public OrderStatus Status { get; set; }
        public OrderPayment PaymentType { get; set; }
        public int ClientId { get; set; }
        public ICollection<OrderDetail> Items { get; set; } = new List<OrderDetail>();
        public DateTime CreatedAt { get; set; }
        
        // Financial fields (required by PostgreSQL schema)
        public decimal SubTotal { get; set; }
        public decimal Tax { get; set; }
        public decimal Discount { get; set; }
        public decimal Total { get; set; }
        
        // Legacy fields for PostgreSQL compatibility
        public DateTime OrderDate { get; set; }
        public string ShippingAddress { get; set; }
        public string Notes { get; set; }

        // Nuevos campos para tracking de estados
        public DateTime UpdatedAt { get; set; } // Required by PostgreSQL schema (NOT NULL)
        public DateTime? PaidAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }
        public string CancellationReason { get; set; }

        // Tracking de pago
        public string PaymentTransactionId { get; set; }
        public string PaymentGateway { get; set; }

        // Shipping Address
        public string ShippingRecipientName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }

        // Billing Address
        public string BillingAddressLine1 { get; set; }
        public string BillingCity { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }
        public bool BillingSameAsShipping { get; set; }
    }
}

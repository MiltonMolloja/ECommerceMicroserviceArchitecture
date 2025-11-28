using System;
using System.Collections.Generic;

namespace Api.Gateway.Models.Payment.DTOs
{
    public class PaymentDto
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string Status { get; set; }
        public string PaymentMethod { get; set; }
        public string TransactionId { get; set; }
        public string PaymentGateway { get; set; }
        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public PaymentDetailDto PaymentDetail { get; set; }
    }

    public class PaymentDetailDto
    {
        public string CardLast4Digits { get; set; }
        public string CardBrand { get; set; }
        public string BillingCountry { get; set; }
    }

    public class PaymentTransactionDto
    {
        public int TransactionId { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }
        public string Status { get; set; }
        public DateTime TransactionDate { get; set; }
        public string ErrorMessage { get; set; }
    }
}

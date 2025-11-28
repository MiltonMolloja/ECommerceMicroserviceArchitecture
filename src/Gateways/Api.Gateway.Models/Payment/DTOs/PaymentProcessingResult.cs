namespace Api.Gateway.Models.Payment.DTOs
{
    public class PaymentProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string Error { get; set; }
        public string ErrorCode { get; set; }
    }
}

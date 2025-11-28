namespace Payment.Service.EventHandlers.Commands
{
    public class PaymentProcessingResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? PaymentId { get; set; }
        public string TransactionId { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
    }
}

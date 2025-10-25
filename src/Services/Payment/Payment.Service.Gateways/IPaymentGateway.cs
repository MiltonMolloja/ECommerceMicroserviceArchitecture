using System.Threading.Tasks;

namespace Payment.Service.Gateways
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> ProcessPaymentAsync(PaymentRequest request);
        Task<RefundResult> ProcessRefundAsync(RefundRequest request);
    }

    public class PaymentRequest
    {
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public string PaymentToken { get; set; }
        public string Description { get; set; }
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Gateway { get; set; }
        public string CardLast4 { get; set; }
        public string CardBrand { get; set; }
        public string ErrorMessage { get; set; }
    }

    public class RefundRequest
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string Reason { get; set; }
    }

    public class RefundResult
    {
        public bool Success { get; set; }
        public string RefundId { get; set; }
        public string ErrorMessage { get; set; }
    }
}

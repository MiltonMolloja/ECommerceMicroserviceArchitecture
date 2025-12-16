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
        public string PaymentMethodId { get; set; }  // MercadoPago payment method ID (e.g., "master", "visa")
        public int Installments { get; set; }
        public string Description { get; set; }
        public string PayerEmail { get; set; }       // Payer's email
        public string CardholderName { get; set; }   // Cardholder name (used for test simulations: APRO, CALL, FUND, etc.)
        public string IdentificationType { get; set; } // Identification type (DNI, CUIL, etc.)
        public string IdentificationNumber { get; set; } // Identification number
    }

    public class PaymentResult
    {
        public bool Success { get; set; }
        public string TransactionId { get; set; }
        public string Gateway { get; set; }
        public string CardLast4 { get; set; }
        public string CardBrand { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorCode { get; set; }
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

using System;

namespace Payment.Domain
{
    public class PaymentTransaction
    {
        public int TransactionId { get; set; }
        public int PaymentId { get; set; }

        public TransactionType TransactionType { get; set; }
        public decimal Amount { get; set; }
        public TransactionStatus Status { get; set; }

        public string GatewayResponse { get; set; }
        public string ErrorMessage { get; set; }
        public DateTime TransactionDate { get; set; }
        public string IPAddress { get; set; }

        // Navegación
        public Payment Payment { get; set; }
    }
}

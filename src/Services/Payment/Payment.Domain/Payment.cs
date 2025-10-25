using System;
using System.Collections.Generic;
using System.Linq;

namespace Payment.Domain
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }

        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public PaymentStatus Status { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        public string TransactionId { get; set; }
        public string PaymentGateway { get; set; }

        public DateTime? PaymentDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        // Navegación
        public PaymentDetail PaymentDetail { get; set; }
        public ICollection<PaymentTransaction> Transactions { get; set; } = new List<PaymentTransaction>();

        // Propiedades calculadas
        public bool IsCompleted => Status == PaymentStatus.Completed;
        public bool IsPending => Status == PaymentStatus.Pending;
        public bool CanBeRefunded => Status == PaymentStatus.Completed &&
                                     PaymentDate.HasValue &&
                                     DateTime.UtcNow.Subtract(PaymentDate.Value).TotalDays <= 30;

        // Métodos de negocio
        public void MarkAsCompleted(string transactionId)
        {
            Status = PaymentStatus.Completed;
            TransactionId = transactionId;
            PaymentDate = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void MarkAsFailed(string errorMessage)
        {
            Status = PaymentStatus.Failed;
            UpdatedAt = DateTime.UtcNow;

            AddTransaction(TransactionType.Charge, Amount, TransactionStatus.Failed, errorMessage);
        }

        public void MarkAsRefunded(string transactionId)
        {
            if (!CanBeRefunded)
                throw new InvalidOperationException("Payment cannot be refunded");

            Status = PaymentStatus.Refunded;
            UpdatedAt = DateTime.UtcNow;

            AddTransaction(TransactionType.Refund, Amount, TransactionStatus.Success, null, transactionId);
        }

        public void AddTransaction(TransactionType type, decimal amount, TransactionStatus status,
                                  string errorMessage = null, string gatewayTransactionId = null)
        {
            Transactions.Add(new PaymentTransaction
            {
                PaymentId = PaymentId,
                TransactionType = type,
                Amount = amount,
                Status = status,
                ErrorMessage = errorMessage,
                TransactionDate = DateTime.UtcNow,
                GatewayResponse = gatewayTransactionId
            });
        }
    }
}

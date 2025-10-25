using MediatR;
using Payment.Domain;

namespace Payment.Service.EventHandlers.Commands
{
    public class ProcessPaymentCommand : INotification
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public PaymentMethod PaymentMethod { get; set; }

        // Para tarjeta de crédito (tokenizada)
        public string PaymentToken { get; set; }

        // Dirección de facturación
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingZipCode { get; set; }
    }
}

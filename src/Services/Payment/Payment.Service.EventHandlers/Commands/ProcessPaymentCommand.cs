using MediatR;

namespace Payment.Service.EventHandlers.Commands
{
    public class ProcessPaymentCommand : IRequest<PaymentProcessingResult>
    {
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string UserEmail { get; set; }  // Email del usuario desde el token JWT

        // Información de pago de MercadoPago
        public string PaymentMethodId { get; set; }  // ej: "master", "visa", "amex"
        public string Token { get; set; }             // Token de MercadoPago
        public int Installments { get; set; }         // Número de cuotas
        public string CardholderName { get; set; }    // Nombre del titular (para simulación: APRO, CALL, FUND, etc.)
        public string IdentificationType { get; set; } // Tipo de documento (DNI, CUIL, etc.)
        public string IdentificationNumber { get; set; } // Número de documento

        // Dirección de facturación
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingZipCode { get; set; }
    }
}

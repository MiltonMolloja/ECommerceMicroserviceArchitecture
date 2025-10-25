namespace Payment.Domain
{
    public class PaymentDetail
    {
        public int PaymentDetailId { get; set; }
        public int PaymentId { get; set; }

        // Solo últimos 4 dígitos (PCI DSS compliance)
        public string CardLast4Digits { get; set; }
        public string CardBrand { get; set; }
        public string CardHolderName { get; set; }

        public int? ExpiryMonth { get; set; }
        public int? ExpiryYear { get; set; }

        // Dirección de facturación
        public string BillingAddress { get; set; }
        public string BillingCity { get; set; }
        public string BillingCountry { get; set; }
        public string BillingZipCode { get; set; }

        // Navegación
        public Payment Payment { get; set; }
    }
}

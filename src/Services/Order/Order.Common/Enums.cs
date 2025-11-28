namespace Order.Common
{
    public class Enums
    {
        public enum OrderStatus
        {
            // Estados de pago
            AwaitingPayment = 0,      // Orden creada, esperando pago
            PaymentProcessing = 1,     // Pago en proceso
            PaymentFailed = 2,         // Pago falló
            Paid = 3,                  // Pago completado

            // Estados de fulfillment
            Processing = 4,            // Orden en procesamiento
            ReadyToShip = 5,          // Lista para enviar
            Shipped = 6,               // Enviada
            InTransit = 7,             // En tránsito
            OutForDelivery = 8,        // En reparto
            Delivered = 9,             // Entregada

            // Estados de cancelación/devolución
            Cancelled = 10,            // Cancelada
            Refunded = 11,             // Reembolsada
            PartiallyRefunded = 12,    // Reembolso parcial
            ReturnRequested = 13,      // Devolución solicitada
            Returned = 14,             // Devuelta

            // Estados de problemas
            OnHold = 15,               // En espera (problema)
            PaymentDisputed = 16       // Pago disputado
        }

        public enum OrderPayment
        {
            CreditCard = 0,
            DebitCard = 1,
            MercadoPago = 2,
            PayPal = 3,
            BankTransfer = 4,
            Cash = 5,
            Other = 99
        }
    }
}

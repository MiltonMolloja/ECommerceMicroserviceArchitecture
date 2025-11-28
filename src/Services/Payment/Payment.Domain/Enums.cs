namespace Payment.Domain
{
    public enum PaymentStatus
    {
        Pending = 1,
        Processing = 2,
        Completed = 3,
        Failed = 4,
        Refunded = 5,
        Cancelled = 6
    }

    public enum PaymentMethod
    {
        CreditCard = 1,
        DebitCard = 2,
        PayPal = 3,
        Stripe = 4,
        BankTransfer = 5,
        CashOnDelivery = 6,
        MercadoPago = 7
    }

    public enum TransactionType
    {
        Charge = 1,
        Refund = 2,
        Void = 3,
        Authorization = 4
    }

    public enum TransactionStatus
    {
        Success = 1,
        Failed = 2,
        Pending = 3
    }
}

namespace Notification.Domain
{
    public enum NotificationType
    {
        OrderPlaced = 1,
        OrderShipped = 2,
        OrderDelivered = 3,
        OrderCancelled = 4,
        PaymentCompleted = 5,
        PaymentFailed = 6,
        Promotion = 7,
        LowStock = 8,
        PriceDrop = 9,
        Newsletter = 10,
        WelcomeEmail = 11,
        PasswordReset = 12
    }
}

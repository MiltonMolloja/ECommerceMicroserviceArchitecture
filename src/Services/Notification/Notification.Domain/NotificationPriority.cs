namespace Notification.Domain
{
    public enum NotificationPriority
    {
        High = 1,      // Urgente: Pago fallido, orden cancelada
        Normal = 2,    // Normal: Orden creada, orden enviada
        Low = 3        // Baja: Promociones, newsletter
    }
}

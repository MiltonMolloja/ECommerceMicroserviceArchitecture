using System;

namespace Notification.Domain
{
    public class NotificationPreferences
    {
        #region Properties - Persistidas

        public int PreferenceId { get; set; }
        public int UserId { get; set; }

        // Canales
        public bool EmailNotifications { get; set; }
        public bool PushNotifications { get; set; }
        public bool SMSNotifications { get; set; }

        // Tipos de notificación
        public bool OrderUpdates { get; set; }
        public bool Promotions { get; set; }
        public bool Newsletter { get; set; }
        public bool PriceAlerts { get; set; }
        public bool StockAlerts { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        #endregion

        #region Business Methods

        /// <summary>
        /// Verifica si el usuario permite notificaciones por un canal específico
        /// </summary>
        public bool AllowsChannel(NotificationChannel channel)
        {
            return channel switch
            {
                NotificationChannel.Email => true,
                NotificationChannel.Push => PushNotifications,
                NotificationChannel.SMS => SMSNotifications,
                NotificationChannel.InApp => true, // In-App siempre permitido
                _ => false
            };
        }

        /// <summary>
        /// Verifica si el usuario permite un tipo específico de notificación
        /// </summary>
        public bool AllowsNotificationType(NotificationType type)
        {
            return type switch
            {
                NotificationType.OrderPlaced => OrderUpdates,
                NotificationType.OrderShipped => OrderUpdates,
                NotificationType.OrderDelivered => OrderUpdates,
                NotificationType.OrderCancelled => OrderUpdates,
                NotificationType.PaymentCompleted => OrderUpdates,
                NotificationType.PaymentFailed => OrderUpdates, // Siempre notificar fallos de pago
                NotificationType.Promotion => Promotions,
                NotificationType.LowStock => StockAlerts,
                NotificationType.PriceDrop => PriceAlerts,
                NotificationType.Newsletter => Newsletter,
                NotificationType.WelcomeEmail => true, // Siempre enviar email de bienvenida
                NotificationType.PasswordReset => true, // Siempre enviar reset de contraseña
                _ => true
            };
        }

        /// <summary>
        /// Habilita todos los canales de notificación
        /// </summary>
        public void EnableAllChannels()
        {
            EmailNotifications = true;
            PushNotifications = true;
            SMSNotifications = true;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deshabilita todos los canales de notificación excepto críticos
        /// </summary>
        public void DisableAllChannels()
        {
            EmailNotifications = false;
            PushNotifications = false;
            SMSNotifications = false;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Habilita todos los tipos de notificación
        /// </summary>
        public void EnableAllTypes()
        {
            OrderUpdates = true;
            Promotions = true;
            Newsletter = true;
            PriceAlerts = true;
            StockAlerts = true;
            UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}

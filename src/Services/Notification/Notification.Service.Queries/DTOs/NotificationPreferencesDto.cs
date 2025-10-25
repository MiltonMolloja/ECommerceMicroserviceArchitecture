using System;

namespace Notification.Service.Queries.DTOs
{
    public class NotificationPreferencesDto
    {
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
    }
}

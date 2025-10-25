using MediatR;
using Notification.Domain;
using System;
using System.Collections.Generic;

namespace Notification.Service.EventHandlers.Commands
{
    public class SendNotificationCommand : INotification
    {
        public int UserId { get; set; }
        public NotificationType Type { get; set; }
        public NotificationPriority Priority { get; set; }
        public Dictionary<string, object> Variables { get; set; } // Para template rendering
        public List<NotificationChannel> Channels { get; set; } // Email, Push, SMS, InApp
        public DateTime? ExpiresAt { get; set; }
    }
}

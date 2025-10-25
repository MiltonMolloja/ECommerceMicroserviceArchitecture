using System;

namespace Notification.Service.Queries.DTOs
{
    public class NotificationDto
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Data { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }
        public string Priority { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Propiedades calculadas
        public bool IsExpired { get; set; }
        public bool IsActive { get; set; }
    }
}

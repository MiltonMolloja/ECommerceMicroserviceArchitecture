using System;

namespace Notification.Domain
{
    public class Notification
    {
        #region Properties - Persistidas

        public int NotificationId { get; set; }
        public int UserId { get; set; }

        public NotificationType Type { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public string Data { get; set; } // JSON metadata (orderId, productId, etc.)

        public bool IsRead { get; set; }
        public DateTime? ReadAt { get; set; }

        public NotificationPriority Priority { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; }

        #endregion

        #region Computed Properties - NO persistidas

        /// <summary>
        /// Indica si la notificación ha expirado
        /// </summary>
        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt < DateTime.UtcNow;

        /// <summary>
        /// Indica si la notificación está activa (no expirada)
        /// </summary>
        public bool IsActive => !IsExpired;

        /// <summary>
        /// Tiempo hasta que expire la notificación
        /// </summary>
        public TimeSpan? TimeToExpire => ExpiresAt.HasValue
            ? ExpiresAt.Value - DateTime.UtcNow
            : null;

        #endregion

        #region Business Methods

        /// <summary>
        /// Marca la notificación como leída
        /// </summary>
        public void MarkAsRead()
        {
            if (!IsRead)
            {
                IsRead = true;
                ReadAt = DateTime.UtcNow;
            }
        }

        /// <summary>
        /// Marca la notificación como no leída
        /// </summary>
        public void MarkAsUnread()
        {
            IsRead = false;
            ReadAt = null;
        }

        /// <summary>
        /// Expira la notificación inmediatamente
        /// </summary>
        public void Expire()
        {
            ExpiresAt = DateTime.UtcNow;
        }

        #endregion
    }
}

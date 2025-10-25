using Notification.Service.Queries.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notification.Service.Queries
{
    public interface INotificationQueryService
    {
        Task<NotificationDto> GetNotificationByIdAsync(int notificationId);
        Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 10);
        Task<List<NotificationDto>> GetUnreadNotificationsAsync(int userId);
        Task<int> GetUnreadCountAsync(int userId);
        Task<NotificationPreferencesDto> GetPreferencesAsync(int userId);
    }
}

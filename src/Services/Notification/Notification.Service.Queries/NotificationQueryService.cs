using Microsoft.EntityFrameworkCore;
using Notification.Persistence.Database;
using Notification.Service.Queries.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Notification.Service.Queries
{
    public class NotificationQueryService : INotificationQueryService
    {
        private readonly ApplicationDbContext _context;

        public NotificationQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<NotificationDto> GetNotificationByIdAsync(int notificationId)
        {
            return await _context.Notifications
                .Where(n => n.NotificationId == notificationId)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    Data = n.Data,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    Priority = n.Priority.ToString(),
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt,
                    IsExpired = n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow,
                    IsActive = !n.ExpiresAt.HasValue || n.ExpiresAt >= DateTime.UtcNow
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<NotificationDto>> GetUserNotificationsAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    Data = n.Data,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    Priority = n.Priority.ToString(),
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt,
                    IsExpired = n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow,
                    IsActive = !n.ExpiresAt.HasValue || n.ExpiresAt >= DateTime.UtcNow
                })
                .ToListAsync();
        }

        public async Task<List<NotificationDto>> GetUnreadNotificationsAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .OrderByDescending(n => n.CreatedAt)
                .Select(n => new NotificationDto
                {
                    NotificationId = n.NotificationId,
                    UserId = n.UserId,
                    Type = n.Type.ToString(),
                    Title = n.Title,
                    Message = n.Message,
                    Data = n.Data,
                    IsRead = n.IsRead,
                    ReadAt = n.ReadAt,
                    Priority = n.Priority.ToString(),
                    ExpiresAt = n.ExpiresAt,
                    CreatedAt = n.CreatedAt,
                    IsExpired = n.ExpiresAt.HasValue && n.ExpiresAt < DateTime.UtcNow,
                    IsActive = !n.ExpiresAt.HasValue || n.ExpiresAt >= DateTime.UtcNow
                })
                .ToListAsync();
        }

        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        public async Task<NotificationPreferencesDto> GetPreferencesAsync(int userId)
        {
            var preferences = await _context.NotificationPreferences
                .Where(p => p.UserId == userId)
                .Select(p => new NotificationPreferencesDto
                {
                    PreferenceId = p.PreferenceId,
                    UserId = p.UserId,
                    EmailNotifications = p.EmailNotifications,
                    PushNotifications = p.PushNotifications,
                    SMSNotifications = p.SMSNotifications,
                    OrderUpdates = p.OrderUpdates,
                    Promotions = p.Promotions,
                    Newsletter = p.Newsletter,
                    PriceAlerts = p.PriceAlerts,
                    StockAlerts = p.StockAlerts,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .FirstOrDefaultAsync();

            // Si no existen preferencias, retornar defaults
            if (preferences == null)
            {
                preferences = new NotificationPreferencesDto
                {
                    UserId = userId,
                    EmailNotifications = true,
                    PushNotifications = true,
                    SMSNotifications = false,
                    OrderUpdates = true,
                    Promotions = true,
                    Newsletter = false,
                    PriceAlerts = true,
                    StockAlerts = false,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }

            return preferences;
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Persistence.Database;
using Notification.Service.EventHandlers.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Service.EventHandlers.Handlers
{
    public class MarkAsReadEventHandler : INotificationHandler<MarkAsReadCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<MarkAsReadEventHandler> _logger;

        public MarkAsReadEventHandler(
            ApplicationDbContext context,
            ILogger<MarkAsReadEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(MarkAsReadCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Marking notification {notification.NotificationId} as read for user {notification.UserId}");

            try
            {
                var notificationEntity = await _context.Notifications
                    .FirstOrDefaultAsync(
                        n => n.NotificationId == notification.NotificationId && n.UserId == notification.UserId,
                        cancellationToken);

                if (notificationEntity == null)
                {
                    _logger.LogWarning($"Notification {notification.NotificationId} not found for user {notification.UserId}");
                    return;
                }

                notificationEntity.MarkAsRead();
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Notification {notification.NotificationId} marked as read");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {notification.NotificationId} as read");
                throw;
            }
        }
    }
}

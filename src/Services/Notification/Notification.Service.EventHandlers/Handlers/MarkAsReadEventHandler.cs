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
            _logger.LogInformation("Marking notification {NotificationId} as read for user {UserId}", notification.NotificationId, notification.UserId);

            try
            {
                var notificationEntity = await _context.Notifications
                    .FirstOrDefaultAsync(
                        n => n.NotificationId == notification.NotificationId && n.UserId == notification.UserId,
                        cancellationToken);

                if (notificationEntity == null)
                {
                    _logger.LogWarning("Notification {NotificationId} not found for user {UserId}", notification.NotificationId, notification.UserId);
                    return;
                }

                notificationEntity.MarkAsRead();
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Notification {NotificationId} marked as read", notification.NotificationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking notification {NotificationId} as read", notification.NotificationId);
                throw;
            }
        }
    }
}

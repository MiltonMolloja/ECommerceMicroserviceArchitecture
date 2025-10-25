using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Domain;
using Notification.Persistence.Database;
using Notification.Service.EventHandlers.Commands;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Service.EventHandlers.Handlers
{
    public class UpdatePreferencesEventHandler : INotificationHandler<UpdatePreferencesCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<UpdatePreferencesEventHandler> _logger;

        public UpdatePreferencesEventHandler(
            ApplicationDbContext context,
            ILogger<UpdatePreferencesEventHandler> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Handle(UpdatePreferencesCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Updating notification preferences for user {notification.UserId}");

            try
            {
                var preferences = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == notification.UserId, cancellationToken);

                if (preferences == null)
                {
                    // Crear preferencias si no existen
                    preferences = new NotificationPreferences
                    {
                        UserId = notification.UserId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _context.NotificationPreferences.AddAsync(preferences, cancellationToken);
                }

                // Actualizar preferencias
                preferences.EmailNotifications = notification.EmailNotifications;
                preferences.PushNotifications = notification.PushNotifications;
                preferences.SMSNotifications = notification.SMSNotifications;
                preferences.OrderUpdates = notification.OrderUpdates;
                preferences.Promotions = notification.Promotions;
                preferences.Newsletter = notification.Newsletter;
                preferences.PriceAlerts = notification.PriceAlerts;
                preferences.StockAlerts = notification.StockAlerts;
                preferences.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation($"Notification preferences updated for user {notification.UserId}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating notification preferences for user {notification.UserId}");
                throw;
            }
        }
    }
}

using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Notification.Domain;
using Notification.Persistence.Database;
using Notification.Service.EventHandlers.Commands;
using Notification.Service.EventHandlers.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Notification.Service.EventHandlers.Handlers
{
    public class SendNotificationEventHandler : INotificationHandler<SendNotificationCommand>
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailNotificationService _emailService;
        private readonly ILogger<SendNotificationEventHandler> _logger;

        public SendNotificationEventHandler(
            ApplicationDbContext context,
            IEmailNotificationService emailService,
            ILogger<SendNotificationEventHandler> logger)
        {
            _context = context;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task Handle(SendNotificationCommand notification, CancellationToken cancellationToken)
        {
            _logger.LogInformation("--- Sending notification type {Type} to user {UserId}", notification.Type, notification.UserId);

            try
            {
                // 1. Obtener preferencias del usuario
                var preferences = await _context.NotificationPreferences
                    .FirstOrDefaultAsync(p => p.UserId == notification.UserId, cancellationToken);

                // Si no existen preferencias, crear defaults
                if (preferences == null)
                {
                    _logger.LogInformation("Creating default preferences for user {UserId}", notification.UserId);
                    preferences = new NotificationPreferences
                    {
                        UserId = notification.UserId,
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
                    await _context.NotificationPreferences.AddAsync(preferences, cancellationToken);
                    await _context.SaveChangesAsync(cancellationToken);
                }

                // 2. Validar que el usuario permite este tipo de notificación
                if (!preferences.AllowsNotificationType(notification.Type))
                {
                    _logger.LogInformation("User {UserId} has disabled notifications of type {Type}", notification.UserId, notification.Type);
                    return; // No enviar notificación
                }

                // 3. Filtrar canales según preferencias del usuario
                var allowedChannels = notification.Channels
                    .Where(channel => preferences.AllowsChannel(channel))
                    .ToList();

                if (!allowedChannels.Any())
                {
                    _logger.LogInformation("User {UserId} has disabled all requested channels", notification.UserId);
                    return; // No enviar notificación
                }

                // 4. Obtener template según tipo de notificación
                var template = await _context.NotificationTemplates
                    .FirstOrDefaultAsync(
                        t => t.Type == notification.Type && t.Channel == NotificationChannel.InApp,
                        cancellationToken);

                string title, message;

                if (template != null)
                {
                    // Renderizar con template
                    title = template.RenderTitle(notification.Variables);
                    message = template.RenderMessage(notification.Variables);
                }
                else
                {
                    // Fallback si no hay template
                    _logger.LogWarning("No template found for notification type {Type}", notification.Type);
                    title = $"Notification: {notification.Type}";
                    message = JsonSerializer.Serialize(notification.Variables);
                }

                // 5. Crear notificación In-App (siempre)
                var inAppNotification = new Domain.Notification
                {
                    UserId = notification.UserId,
                    Type = notification.Type,
                    Title = title,
                    Message = message,
                    Data = JsonSerializer.Serialize(notification.Variables),
                    IsRead = false,
                    Priority = notification.Priority,
                    ExpiresAt = notification.ExpiresAt,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Notifications.AddAsync(inAppNotification, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("In-App notification {NotificationId} created successfully", inAppNotification.NotificationId);

                // 6. Enviar por otros canales (Email, Push, SMS)
                foreach (var channel in allowedChannels.Where(c => c != NotificationChannel.InApp))
                {
                    _logger.LogInformation("Sending notification via {Channel}", channel);

                    if (channel == NotificationChannel.Email)
                    {
                        await SendEmailNotificationAsync(notification, template);
                    }
                    else
                    {
                        _logger.LogInformation("TODO: Send notification via {Channel} (Provider not implemented yet)", channel);
                        // TODO: Implementar Push y SMS
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification to user {UserId}", notification.UserId);
                throw;
            }
        }

        private async Task SendEmailNotificationAsync(
            SendNotificationCommand notification,
            NotificationTemplate template)
        {
            try
            {
                // Obtener el email del usuario desde las variables
                var userEmail = GetVariableValue(notification.Variables, "Email")
                    ?? GetVariableValue(notification.Variables, "UserEmail")
                    ?? GetVariableValue(notification.Variables, "email");

                if (string.IsNullOrEmpty(userEmail))
                {
                    _logger.LogWarning("Cannot send email to user {UserId}: Email not provided in variables", notification.UserId);
                    return;
                }

                // Determinar el template a usar basado en el tipo de notificación
                string templateName = DetermineEmailTemplate(notification.Type);

                _logger.LogInformation("Sending email to {UserEmail} using template {TemplateName}", userEmail, templateName);

                // Enviar el email con todas las variables
                await _emailService.SendTemplatedEmailAsync(userEmail, templateName, notification.Variables);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending email notification to user {UserId}", notification.UserId);
                // No lanzar excepción para no interrumpir el flujo
            }
        }

        private string GetVariableValue(Dictionary<string, object> variables, string key)
        {
            if (variables == null) return null;

            // Try exact match first
            if (variables.ContainsKey(key))
                return variables[key]?.ToString();

            // Try case-insensitive search
            var foundKey = variables.Keys.FirstOrDefault(k => k.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (foundKey != null)
                return variables[foundKey]?.ToString();

            return null;
        }

        private string DetermineEmailTemplate(NotificationType type)
        {
            return type switch
            {
                NotificationType.OrderPlaced => "purchase-confirmation",
                NotificationType.OrderShipped => "order-shipped",
                NotificationType.OrderDelivered => "order-delivered",
                NotificationType.OrderCancelled => "order-cancelled",
                NotificationType.PaymentCompleted => "payment-completed",
                NotificationType.PaymentFailed => "payment-failed",
                NotificationType.PaymentRefunded => "refund-processed",
                NotificationType.WelcomeEmail => "welcome",
                NotificationType.PasswordReset => "password-reset",
                _ => "default"
            };
        }
    }
}

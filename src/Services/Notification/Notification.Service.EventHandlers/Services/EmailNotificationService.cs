using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Notification.Service.EventHandlers.Services
{
    public class EmailNotificationService : IEmailNotificationService
    {
        private readonly HttpClient _httpClient;
        private readonly string _notificationApiUrl;
        private readonly ILogger<EmailNotificationService> _logger;

        public EmailNotificationService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<EmailNotificationService> logger)
        {
            _httpClient = httpClient;
            _notificationApiUrl = configuration["NotificationApiUrl"] ?? "http://localhost:45000";
            _logger = logger;
        }

        public async Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, object> data)
        {
            try
            {
                _logger.LogInformation("Sending templated email to {To} using template {TemplateName}", to, templateName);

                var request = new
                {
                    To = to,
                    Template = templateName,
                    Data = data
                };

                var response = await _httpClient.PostAsJsonAsync(
                    $"{_notificationApiUrl}/api/v1/notifications/email",
                    request);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogWarning("Failed to send email to {To}. Status: {StatusCode}, Error: {ErrorContent}", to, response.StatusCode, errorContent);
                }
                else
                {
                    _logger.LogInformation("Email sent successfully to {To}", to);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending templated email to {To}", to);
                // No lanzar excepción, solo loguear
            }
        }
    }
}

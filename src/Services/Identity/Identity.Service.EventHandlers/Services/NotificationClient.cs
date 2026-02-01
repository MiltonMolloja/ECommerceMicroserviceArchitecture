using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers.Services
{
    public class NotificationClient : INotificationClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<NotificationClient> _logger;
        private readonly string _notificationApiUrl;

        public NotificationClient(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<NotificationClient> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _notificationApiUrl = configuration.GetValue<string>("Email:NotificationApiUrl");
        }

        public async Task SendEmailAsync(string to, string template, object data)
        {
            try
            {
                var request = new
                {
                    To = to,
                    Template = template,
                    Data = data
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_notificationApiUrl}/api/v1/notifications/email", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error sending email to {To}. Status: {StatusCode}. Error: {ErrorContent}", to, response.StatusCode, errorContent);
                }
                else
                {
                    _logger.LogInformation("Email sent successfully to {To} using template {Template}", to, template);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "Exception sending email to {To}", to);
                // Don't throw - email failures shouldn't break the main flow
            }
        }

        public async Task SendSmsAsync(string phoneNumber, string message)
        {
            try
            {
                var request = new
                {
                    PhoneNumber = phoneNumber,
                    Message = message
                };

                var json = JsonSerializer.Serialize(request);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync($"{_notificationApiUrl}/api/v1/notifications/sms", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Error sending SMS to {PhoneNumber}. Status: {StatusCode}. Error: {ErrorContent}", phoneNumber, response.StatusCode, errorContent);
                }
                else
                {
                    _logger.LogInformation("SMS sent successfully to {PhoneNumber}", phoneNumber);
                }
            }
            catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or HttpRequestException or TaskCanceledException)
            {
                _logger.LogError(ex, "Exception sending SMS to {PhoneNumber}", phoneNumber);
                // Don't throw - SMS failures shouldn't break the main flow
            }
        }
    }
}

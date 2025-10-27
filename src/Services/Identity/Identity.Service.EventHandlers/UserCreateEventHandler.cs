using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class UserCreateEventHandler :
        IRequestHandler<UserCreateCommand, IdentityResult>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserCreateEventHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserCreateEventHandler(
            UserManager<ApplicationUser> userManager,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserCreateEventHandler> logger,
            IHttpClientFactory httpClientFactory)
        {
            _userManager = userManager;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        public async Task<IdentityResult> Handle(UserCreateCommand notification, CancellationToken cancellationToken)
        {
            var entry = new ApplicationUser
            {
                FirstName = notification.FirstName,
                LastName = notification.LastName,
                Email = notification.Email,
                UserName = notification.Email
            };

            var result = await _userManager.CreateAsync(entry, notification.Password);

            if (result.Succeeded)
            {
                try
                {
                    // Generate email confirmation token
                    var confirmationToken = await _userManager.GenerateEmailConfirmationTokenAsync(entry);

                    // Send email confirmation
                    await _notificationClient.SendEmailAsync(
                        entry.Email,
                        "email-confirmation",
                        new
                        {
                            FirstName = entry.FirstName,
                            ConfirmationToken = confirmationToken,
                            UserId = entry.Id,
                            ExpirationHours = 24
                        });

                    // Audit log
                    var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                    var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                    await _auditService.LogActionAsync(
                        entry.Id,
                        "UserRegistration",
                        true,
                        ipAddress,
                        userAgent);

                    _logger.LogInformation($"User created successfully: {entry.Email}. Confirmation email sent.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error sending confirmation email to {entry.Email}");
                    // Don't fail the registration if email sending fails
                }

                // Create client profile in Customer.Api
                try
                {
                    var httpClient = _httpClientFactory.CreateClient("CustomerApi");
                    // NOTA: No enviamos FirstName, LastName, Email a Customer
                    // Se obtienen directamente de Identity via UserId
                    var clientRequest = new
                    {
                        userId = entry.Id,
                        phone = (string)null,
                        preferredLanguage = "es"
                    };

                    var response = await httpClient.PostAsJsonAsync("/v1/clients", clientRequest, cancellationToken);

                    if (response.IsSuccessStatusCode)
                    {
                        _logger.LogInformation($"Client profile created successfully for user: {entry.Email}");
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                        _logger.LogWarning($"Failed to create client profile for user {entry.Email}. Status: {response.StatusCode}, Error: {errorContent}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error creating client profile for user {entry.Email}");
                    // Don't fail the registration if client creation fails
                }
            }

            return result;
        }
    }
}

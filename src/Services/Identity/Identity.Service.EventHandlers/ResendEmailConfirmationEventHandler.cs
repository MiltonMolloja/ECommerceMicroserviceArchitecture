using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class ResendEmailConfirmationEventHandler : IRequestHandler<ResendEmailConfirmationCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ResendEmailConfirmationEventHandler> _logger;

        public ResendEmailConfirmationEventHandler(
            UserManager<ApplicationUser> userManager,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ResendEmailConfirmationEventHandler> logger)
        {
            _userManager = userManager;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(ResendEmailConfirmationCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                // Don't reveal if user exists or not
                if (user == null)
                {
                    _logger.LogWarning($"Resend email confirmation for non-existent email: {request.Email}");
                    return true; // Return true to not reveal user existence
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation($"Email already confirmed for {user.Email}");
                    return true;
                }

                // Generate email confirmation token
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                // Send confirmation email
                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "email-confirmation",
                    new
                    {
                        FirstName = user.FirstName,
                        ConfirmationToken = token,
                        UserId = user.Id,
                        ExpirationHours = 24
                    });

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "ResendEmailConfirmation",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Email confirmation resent to {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resending email confirmation to {request.Email}");
                throw;
            }
        }
    }
}

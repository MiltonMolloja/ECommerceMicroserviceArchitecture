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
    public class Verify2FAEventHandler : IRequestHandler<Verify2FACommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<Verify2FAEventHandler> _logger;

        public Verify2FAEventHandler(
            UserManager<ApplicationUser> userManager,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Verify2FAEventHandler> logger)
        {
            _userManager = userManager;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(Verify2FACommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"2FA verify attempt for non-existent user: {request.UserId}");
                    return false;
                }

                // Verify the code
                var isValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    request.Code);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                if (!isValid)
                {
                    _logger.LogWarning($"Invalid 2FA code for user {user.Email}");

                    await _auditService.LogActionAsync(
                        user.Id,
                        "Verify2FA",
                        false,
                        ipAddress,
                        userAgent,
                        "Invalid verification code");

                    return false;
                }

                // Enable 2FA for the user
                await _userManager.SetTwoFactorEnabledAsync(user, true);

                // Send notification email
                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "2fa-enabled",
                    new
                    {
                        FirstName = user.FirstName,
                        EnabledTime = DateTime.UtcNow,
                        IpAddress = ipAddress
                    });

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "Verify2FA",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"2FA successfully enabled for {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error verifying 2FA for user {request.UserId}");
                throw;
            }
        }
    }
}

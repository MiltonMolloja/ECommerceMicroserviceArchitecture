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
    public class Disable2FAEventHandler : IRequestHandler<Disable2FACommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITwoFactorService _twoFactorService;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<Disable2FAEventHandler> _logger;

        public Disable2FAEventHandler(
            UserManager<ApplicationUser> userManager,
            ITwoFactorService twoFactorService,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Disable2FAEventHandler> logger)
        {
            _userManager = userManager;
            _twoFactorService = twoFactorService;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(Disable2FACommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"2FA disable attempt for non-existent user: {request.UserId}");
                    return false;
                }

                // Verify password
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    _logger.LogWarning($"Invalid password during 2FA disable for {user.Email}");
                    return false;
                }

                // Verify 2FA code or backup code
                var codeValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    request.Code);

                if (!codeValid)
                {
                    // Try backup code
                    codeValid = await _twoFactorService.ValidateBackupCodeAsync(user.Id, request.Code);
                }

                if (!codeValid)
                {
                    _logger.LogWarning($"Invalid 2FA code during disable for {user.Email}");
                    return false;
                }

                // Disable 2FA
                await _userManager.SetTwoFactorEnabledAsync(user, false);

                // Reset authenticator key
                await _userManager.ResetAuthenticatorKeyAsync(user);

                // Invalidate all backup codes
                await _twoFactorService.InvalidateBackupCodesAsync(user.Id);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                // Send notification email
                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "2fa-disabled",
                    new
                    {
                        FirstName = user.FirstName,
                        DisabledTime = DateTime.UtcNow,
                        IpAddress = ipAddress
                    });

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "Disable2FA",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"2FA successfully disabled for {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error disabling 2FA for user {request.UserId}");
                throw;
            }
        }
    }
}

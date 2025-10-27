using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class Enable2FAEventHandler : IRequestHandler<Enable2FACommand, Enable2FAResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<Enable2FAEventHandler> _logger;

        public Enable2FAEventHandler(
            UserManager<ApplicationUser> userManager,
            ITwoFactorService twoFactorService,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Enable2FAEventHandler> logger)
        {
            _userManager = userManager;
            _twoFactorService = twoFactorService;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<Enable2FAResponse> Handle(Enable2FACommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"2FA enable attempt for non-existent user: {request.UserId}");
                    return new Enable2FAResponse { Succeeded = false };
                }

                // Reset authenticator key
                await _userManager.ResetAuthenticatorKeyAsync(user);

                // Get the new authenticator key
                var secret = await _userManager.GetAuthenticatorKeyAsync(user);

                // Generate QR code URI
                var qrCodeUri = _twoFactorService.GenerateQRCodeUri(user.Email, secret);

                // Generate backup codes
                var backupCodes = await _twoFactorService.GenerateBackupCodesAsync(user.Id, 10);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "Enable2FA",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"2FA setup initiated for {user.Email}. Secret generated.");

                return new Enable2FAResponse
                {
                    Succeeded = true,
                    Secret = secret,
                    QrCodeUri = qrCodeUri,
                    BackupCodes = backupCodes.ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error enabling 2FA for user {request.UserId}");
                throw;
            }
        }
    }
}

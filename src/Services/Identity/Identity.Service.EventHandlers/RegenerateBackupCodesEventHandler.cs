using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class RegenerateBackupCodesEventHandler : IRequestHandler<RegenerateBackupCodesCommand, RegenerateBackupCodesResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RegenerateBackupCodesEventHandler> _logger;

        public RegenerateBackupCodesEventHandler(
            UserManager<ApplicationUser> userManager,
            ITwoFactorService twoFactorService,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RegenerateBackupCodesEventHandler> logger)
        {
            _userManager = userManager;
            _twoFactorService = twoFactorService;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<RegenerateBackupCodesResponse> Handle(RegenerateBackupCodesCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"Backup codes regeneration attempt for non-existent user: {request.UserId}");
                    return new RegenerateBackupCodesResponse { Succeeded = false };
                }

                // Verify password
                var passwordValid = await _userManager.CheckPasswordAsync(user, request.Password);
                if (!passwordValid)
                {
                    _logger.LogWarning($"Invalid password during backup codes regeneration for {user.Email}");
                    return new RegenerateBackupCodesResponse { Succeeded = false };
                }

                // Verify 2FA code (accept both authenticator TOTP and backup codes)
                var codeValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    request.Code);

                // If authenticator code fails, try backup code (but don't mark as used)
                if (!codeValid)
                {
                    var backupCodeHash = HashCode(request.Code);
                    var backupCode = await _twoFactorService.ValidateBackupCodeExistsAsync(user.Id, backupCodeHash);
                    codeValid = backupCode != null;
                }

                if (!codeValid)
                {
                    _logger.LogWarning($"Invalid 2FA code during backup codes regeneration for {user.Email}");
                    return new RegenerateBackupCodesResponse { Succeeded = false };
                }

                // Generate new backup codes
                var backupCodes = await _twoFactorService.GenerateBackupCodesAsync(user.Id, 10);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "RegenerateBackupCodes",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Backup codes regenerated for {user.Email}");

                return new RegenerateBackupCodesResponse
                {
                    Succeeded = true,
                    BackupCodes = backupCodes.ToArray()
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error regenerating backup codes for user {request.UserId}");
                throw;
            }
        }

        private string HashCode(string code)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(code);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

using Identity.Domain;
using Identity.Persistence.Database;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class ResetPasswordEventHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ResetPasswordEventHandler> _logger;

        public ResetPasswordEventHandler(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ResetPasswordEventHandler> logger)
        {
            _userManager = userManager;
            _context = context;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);
                if (user == null)
                {
                    _logger.LogWarning($"Reset password attempt for non-existent email: {request.Email}");
                    return false;
                }

                // Reset password using token
                var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning($"Password reset failed for {user.Email}: {errors}");

                    await _auditService.LogActionAsync(
                        user.Id,
                        "ResetPassword",
                        false,
                        ipAddress,
                        userAgent,
                        errors);

                    return false;
                }

                // Invalidate all refresh tokens (logout from all sessions)
                var refreshTokens = await _context.RefreshTokens
                    .Where(rt => rt.UserId == user.Id && rt.IsActive)
                    .ToListAsync(cancellationToken);

                foreach (var token in refreshTokens)
                {
                    token.IsRevoked = true;
                    token.RevokedAt = DateTime.UtcNow;
                    token.RevokedByIp = ipAddress;
                }

                await _context.SaveChangesAsync(cancellationToken);

                // Send confirmation email
                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "password-reset-confirmation",
                    new
                    {
                        FirstName = user.FirstName,
                        ResetTime = DateTime.UtcNow,
                        IpAddress = ipAddress
                    });

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "ResetPassword",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Password reset successfully for {user.Email}. All sessions invalidated.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error resetting password for {request.Email}");
                throw;
            }
        }
    }
}

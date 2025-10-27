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
    public class ChangePasswordEventHandler : IRequestHandler<ChangePasswordCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ChangePasswordEventHandler> _logger;

        public ChangePasswordEventHandler(
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ChangePasswordEventHandler> logger)
        {
            _userManager = userManager;
            _context = context;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"User not found: {request.UserId}");
                    return false;
                }

                // Change password
                var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning($"Password change failed for user {user.Email}: {errors}");

                    await _auditService.LogActionAsync(
                        user.Id,
                        "ChangePassword",
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

                // Send notification email
                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "password-changed",
                    new
                    {
                        FirstName = user.FirstName,
                        ChangeTime = DateTime.UtcNow,
                        IpAddress = ipAddress
                    });

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "ChangePassword",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Password changed successfully for user {user.Email}. All sessions invalidated.");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user {request.UserId}");
                throw;
            }
        }
    }
}

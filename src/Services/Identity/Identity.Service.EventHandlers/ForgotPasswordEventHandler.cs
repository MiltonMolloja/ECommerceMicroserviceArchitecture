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
    public class ForgotPasswordEventHandler : IRequestHandler<ForgotPasswordCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationClient _notificationClient;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ForgotPasswordEventHandler> _logger;

        public ForgotPasswordEventHandler(
            UserManager<ApplicationUser> userManager,
            INotificationClient notificationClient,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ForgotPasswordEventHandler> logger)
        {
            _userManager = userManager;
            _notificationClient = notificationClient;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(request.Email);

                // Don't reveal if user exists or not (security best practice)
                if (user == null)
                {
                    _logger.LogWarning($"Forgot password request for non-existent email: {request.Email}");
                    // Still return true to not reveal user existence
                    return true;
                }

                // Generate password reset token
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                // Send password reset email
                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "password-reset",
                    new
                    {
                        FirstName = user.FirstName,
                        ResetToken = token,
                        Email = user.Email,
                        RequestTime = DateTime.UtcNow,
                        IpAddress = ipAddress,
                        ExpirationMinutes = 60 // Token expires in 1 hour
                    });

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "ForgotPassword",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Password reset email sent to {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing forgot password request for {request.Email}");
                throw;
            }
        }
    }
}

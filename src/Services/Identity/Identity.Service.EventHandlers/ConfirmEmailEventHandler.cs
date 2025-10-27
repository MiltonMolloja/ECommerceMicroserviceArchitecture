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
    public class ConfirmEmailEventHandler : IRequestHandler<ConfirmEmailCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ConfirmEmailEventHandler> _logger;

        public ConfirmEmailEventHandler(
            UserManager<ApplicationUser> userManager,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<ConfirmEmailEventHandler> logger)
        {
            _userManager = userManager;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(ConfirmEmailCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"Email confirmation attempt for non-existent user: {request.UserId}");
                    return false;
                }

                if (user.EmailConfirmed)
                {
                    _logger.LogInformation($"Email already confirmed for user {user.Email}");
                    return true;
                }

                var result = await _userManager.ConfirmEmailAsync(user, request.Token);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    _logger.LogWarning($"Email confirmation failed for {user.Email}: {errors}");

                    await _auditService.LogActionAsync(
                        user.Id,
                        "ConfirmEmail",
                        false,
                        ipAddress,
                        userAgent,
                        errors);

                    return false;
                }

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "ConfirmEmail",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Email confirmed successfully for {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error confirming email for user {request.UserId}");
                throw;
            }
        }
    }
}

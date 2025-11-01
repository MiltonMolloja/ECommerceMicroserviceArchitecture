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
    public class UpdateUserProfileEventHandler : IRequestHandler<UpdateUserProfileCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UpdateUserProfileEventHandler> _logger;

        public UpdateUserProfileEventHandler(
            UserManager<ApplicationUser> userManager,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UpdateUserProfileEventHandler> logger)
        {
            _userManager = userManager;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"Update profile attempt for non-existent user: {request.UserId}");
                    return false;
                }

                // Update user profile
                user.FirstName = request.FirstName;
                user.LastName = request.LastName;

                var result = await _userManager.UpdateAsync(user);

                if (!result.Succeeded)
                {
                    _logger.LogWarning($"Failed to update profile for user {user.Email}");
                    return false;
                }

                // Audit log
                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                await _auditService.LogActionAsync(
                    user.Id,
                    "UpdateProfile",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Profile updated successfully for user {user.Email}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating profile for user {request.UserId}");
                return false;
            }
        }
    }
}

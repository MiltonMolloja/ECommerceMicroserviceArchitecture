using Identity.Persistence.Database;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class RevokeAllSessionsEventHandler : IRequestHandler<RevokeAllSessionsCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RevokeAllSessionsEventHandler> _logger;

        public RevokeAllSessionsEventHandler(
            ApplicationDbContext context,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RevokeAllSessionsEventHandler> logger)
        {
            _context = context;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(RevokeAllSessionsCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var sessions = await _context.RefreshTokens
                    .Where(rt => rt.UserId == request.UserId && !rt.IsRevoked && rt.ExpiresAt > DateTime.UtcNow)
                    .ToListAsync(cancellationToken);

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                foreach (var session in sessions)
                {
                    // Skip current session if specified
                    if (!string.IsNullOrEmpty(request.CurrentRefreshToken) && session.Token == request.CurrentRefreshToken)
                    {
                        continue;
                    }

                    session.IsRevoked = true;
                    session.RevokedAt = DateTime.UtcNow;
                    session.RevokedByIp = ipAddress;
                }

                await _context.SaveChangesAsync(cancellationToken);

                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                await _auditService.LogActionAsync(
                    request.UserId,
                    "RevokeAllSessions",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"All sessions revoked for user {request.UserId} (excluding current)");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking all sessions for user {request.UserId}");
                throw;
            }
        }
    }
}

using Identity.Persistence.Database;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class RevokeSessionEventHandler : IRequestHandler<RevokeSessionCommand, bool>
    {
        private readonly ApplicationDbContext _context;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<RevokeSessionEventHandler> _logger;

        public RevokeSessionEventHandler(
            ApplicationDbContext context,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<RevokeSessionEventHandler> logger)
        {
            _context = context;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<bool> Handle(RevokeSessionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var session = await _context.RefreshTokens
                    .FirstOrDefaultAsync(rt => rt.Id == request.SessionId && rt.UserId == request.UserId, cancellationToken);

                if (session == null)
                {
                    _logger.LogWarning($"Session {request.SessionId} not found for user {request.UserId}");
                    return false;
                }

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

                session.IsRevoked = true;
                session.RevokedAt = DateTime.UtcNow;
                session.RevokedByIp = ipAddress;

                await _context.SaveChangesAsync(cancellationToken);

                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                await _auditService.LogActionAsync(
                    request.UserId,
                    "RevokeSession",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Session {request.SessionId} revoked for user {request.UserId}");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error revoking session {request.SessionId} for user {request.UserId}");
                throw;
            }
        }
    }
}

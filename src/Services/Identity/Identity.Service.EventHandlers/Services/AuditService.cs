using Identity.Domain;
using Identity.Persistence.Database;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers.Services
{
    public class AuditService : IAuditService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AuditService> _logger;

        public AuditService(
            ApplicationDbContext context,
            ILogger<AuditService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task LogActionAsync(
            string userId,
            string action,
            bool success,
            string ipAddress = null,
            string userAgent = null,
            string failureReason = null)
        {
            try
            {
                var auditLog = new UserAuditLog
                {
                    UserId = userId,
                    Action = action,
                    Success = success,
                    IpAddress = ipAddress,
                    UserAgent = userAgent,
                    FailureReason = failureReason,
                    Timestamp = DateTime.UtcNow
                };

                _context.UserAuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Audit log created: User {userId} - Action {action} - Success {success}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error creating audit log for user {userId}, action {action}");
                // Don't throw - audit failures shouldn't break the main flow
            }
        }
    }
}

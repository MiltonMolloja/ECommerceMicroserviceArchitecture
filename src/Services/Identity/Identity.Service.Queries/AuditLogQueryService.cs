using Identity.Persistence.Database;
using Identity.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Service.Queries
{
    public class AuditLogQueryService : IAuditLogQueryService
    {
        private readonly ApplicationDbContext _context;

        public AuditLogQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AuditLogDto>> GetUserAuditLogsAsync(string userId, int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;

            var logs = await _context.UserAuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Skip(skip)
                .Take(pageSize)
                .Select(log => new AuditLogDto
                {
                    Id = log.Id,
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    IpAddress = log.IpAddress,
                    UserAgent = log.UserAgent,
                    Success = log.Success,
                    FailureReason = log.FailureReason
                })
                .ToListAsync();

            return logs;
        }
    }
}

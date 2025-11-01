using Identity.Persistence.Database;
using Identity.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Service.Queries
{
    public class AccountActivityQueryService : IAccountActivityQueryService
    {
        private readonly ApplicationDbContext _context;

        public AccountActivityQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AccountActivityDto>> GetRecentActivityAsync(string userId, int limit = 20)
        {
            var activities = await _context.UserAuditLogs
                .Where(log => log.UserId == userId)
                .OrderByDescending(log => log.Timestamp)
                .Take(limit)
                .Select(log => new AccountActivityDto
                {
                    Action = log.Action,
                    Timestamp = log.Timestamp,
                    IpAddress = log.IpAddress ?? "Unknown",
                    Success = log.Success
                })
                .ToListAsync();

            return activities;
        }
    }
}

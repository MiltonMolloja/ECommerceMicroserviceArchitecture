using Identity.Persistence.Database;
using Identity.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Identity.Service.Queries
{
    public class SessionQueryService : ISessionQueryService
    {
        private readonly ApplicationDbContext _context;

        public SessionQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SessionDto>> GetActiveSessionsAsync(string userId, string currentRefreshToken = null)
        {
            var sessions = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .OrderByDescending(rt => rt.CreatedAt)
                .Select(rt => new SessionDto
                {
                    Id = rt.Id,
                    DeviceInfo = rt.CreatedByIp ?? "Unknown",
                    IpAddress = rt.CreatedByIp,
                    CreatedAt = rt.CreatedAt,
                    ExpiresAt = rt.ExpiresAt,
                    IsCurrent = !string.IsNullOrEmpty(currentRefreshToken) && rt.Token == currentRefreshToken
                })
                .ToListAsync();

            return sessions;
        }
    }
}

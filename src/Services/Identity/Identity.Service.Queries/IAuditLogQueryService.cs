using Identity.Service.Queries.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Service.Queries
{
    public interface IAuditLogQueryService
    {
        Task<List<AuditLogDto>> GetUserAuditLogsAsync(string userId, int page = 1, int pageSize = 50);
    }
}

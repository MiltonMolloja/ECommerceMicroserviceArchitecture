using Identity.Service.Queries.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Service.Queries
{
    public interface IAccountActivityQueryService
    {
        Task<List<AccountActivityDto>> GetRecentActivityAsync(string userId, int limit = 20);
    }
}

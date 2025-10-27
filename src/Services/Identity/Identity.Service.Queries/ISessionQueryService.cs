using Identity.Service.Queries.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Service.Queries
{
    public interface ISessionQueryService
    {
        Task<List<SessionDto>> GetActiveSessionsAsync(string userId, string currentRefreshToken = null);
    }
}

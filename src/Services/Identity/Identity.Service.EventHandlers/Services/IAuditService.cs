using System.Threading.Tasks;

namespace Identity.Service.EventHandlers.Services
{
    public interface IAuditService
    {
        Task LogActionAsync(string userId, string action, bool success, string ipAddress = null, string userAgent = null, string failureReason = null);
    }
}

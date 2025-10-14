using Identity.Domain;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers.Services
{
    public interface IRefreshTokenService
    {
        Task<RefreshToken> GenerateRefreshTokenAsync(string userId, string ipAddress);
        Task<RefreshToken> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress, string replacedByToken = null);
        Task RevokeAllUserTokensAsync(string userId, string ipAddress);
        Task CleanupExpiredTokensAsync();
    }
}

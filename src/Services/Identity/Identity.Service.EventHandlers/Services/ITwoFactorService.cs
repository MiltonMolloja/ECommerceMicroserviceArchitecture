using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers.Services
{
    public interface ITwoFactorService
    {
        Task<List<string>> GenerateBackupCodesAsync(string userId, int count = 10);
        Task<bool> ValidateBackupCodeAsync(string userId, string code);
        Task<object> ValidateBackupCodeExistsAsync(string userId, string codeHash);
        Task InvalidateBackupCodesAsync(string userId);
        string GenerateQRCodeUri(string email, string secret);
    }
}

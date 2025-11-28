using Identity.Domain;
using Identity.Persistence.Database;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace Identity.Service.EventHandlers.Services
{
    public class TwoFactorService : ITwoFactorService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TwoFactorService> _logger;

        public TwoFactorService(
            ApplicationDbContext context,
            ILogger<TwoFactorService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<string>> GenerateBackupCodesAsync(string userId, int count = 10)
        {
            try
            {
                // Invalidate previous codes
                await InvalidateBackupCodesAsync(userId);

                var codes = new List<string>();
                var backupCodes = new List<UserBackupCode>();

                for (int i = 0; i < count; i++)
                {
                    var code = GenerateRandomCode();
                    codes.Add(code);

                    var backupCode = new UserBackupCode
                    {
                        UserId = userId,
                        CodeHash = HashCode(code),
                        IsUsed = false,
                        CreatedAt = DateTime.UtcNow
                    };

                    backupCodes.Add(backupCode);
                }

                _context.UserBackupCodes.AddRange(backupCodes);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Generated {count} backup codes for user {userId}");

                return codes;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error generating backup codes for user {userId}");
                throw;
            }
        }

        public async Task<bool> ValidateBackupCodeAsync(string userId, string code)
        {
            try
            {
                var codeHash = HashCode(code);

                var backupCode = await _context.UserBackupCodes
                    .FirstOrDefaultAsync(bc => bc.UserId == userId
                                            && bc.CodeHash == codeHash
                                            && !bc.IsUsed);

                if (backupCode == null)
                {
                    _logger.LogWarning($"Invalid backup code attempt for user {userId}");
                    return false;
                }

                // Mark as used
                backupCode.IsUsed = true;
                backupCode.UsedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Backup code validated and marked as used for user {userId}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error validating backup code for user {userId}");
                return false;
            }
        }

        public async Task<object> ValidateBackupCodeExistsAsync(string userId, string codeHash)
        {
            try
            {
                var backupCode = await _context.UserBackupCodes
                    .FirstOrDefaultAsync(bc => bc.UserId == userId
                                            && bc.CodeHash == codeHash
                                            && !bc.IsUsed);

                return backupCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking backup code existence for user {userId}");
                return null;
            }
        }

        public async Task InvalidateBackupCodesAsync(string userId)
        {
            try
            {
                var existingCodes = await _context.UserBackupCodes
                    .Where(bc => bc.UserId == userId)
                    .ToListAsync();

                if (existingCodes.Any())
                {
                    _context.UserBackupCodes.RemoveRange(existingCodes);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation($"Invalidated {existingCodes.Count} backup codes for user {userId}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error invalidating backup codes for user {userId}");
                throw;
            }
        }

        public string GenerateQRCodeUri(string email, string secret)
        {
            var encodedEmail = HttpUtility.UrlEncode(email);
            var encodedSecret = HttpUtility.UrlEncode(secret);
            return $"otpauth://totp/ECommerceApp:{encodedEmail}?secret={encodedSecret}&issuer=ECommerceApp&digits=6";
        }

        private string GenerateRandomCode()
        {
            // Generate a 10-character alphanumeric code
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            var random = RandomNumberGenerator.Create();
            var code = new char[10];

            for (int i = 0; i < code.Length; i++)
            {
                var randomBytes = new byte[4];
                random.GetBytes(randomBytes);
                var randomNumber = BitConverter.ToUInt32(randomBytes, 0);
                code[i] = chars[(int)(randomNumber % chars.Length)];
            }

            return new string(code);
        }

        private string HashCode(string code)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(code);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}

using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace Identity.Domain
{
    public class ApplicationUser : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public ICollection<ApplicationUserRole> UserRoles { get; set; }
        public ICollection<RefreshToken> RefreshTokens { get; set; }
        public ICollection<UserBackupCode> BackupCodes { get; set; }
        public ICollection<UserAuditLog> AuditLogs { get; set; }
    }
}

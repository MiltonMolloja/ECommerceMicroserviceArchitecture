using System;

namespace Identity.Domain
{
    public class RefreshToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime? RevokedAt { get; set; }
        public string ReplacedByToken { get; set; }
        public string CreatedByIp { get; set; }
        public string RevokedByIp { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; }

        // Helper properties
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}

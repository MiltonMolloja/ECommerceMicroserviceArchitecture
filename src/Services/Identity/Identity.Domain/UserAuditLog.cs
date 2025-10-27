using System;

namespace Identity.Domain
{
    public class UserAuditLog
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; }
    }
}

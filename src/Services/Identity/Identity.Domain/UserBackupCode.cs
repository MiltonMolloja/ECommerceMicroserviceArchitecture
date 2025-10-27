using System;

namespace Identity.Domain
{
    public class UserBackupCode
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public string CodeHash { get; set; }
        public bool IsUsed { get; set; }
        public DateTime? UsedAt { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation property
        public ApplicationUser User { get; set; }
    }
}

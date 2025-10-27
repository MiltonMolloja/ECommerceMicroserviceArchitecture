using System;

namespace Identity.Service.Queries.DTOs
{
    public class AuditLogDto
    {
        public int Id { get; set; }
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public string UserAgent { get; set; }
        public bool Success { get; set; }
        public string FailureReason { get; set; }
    }
}

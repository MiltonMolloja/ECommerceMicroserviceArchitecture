using System;

namespace Identity.Service.Queries.DTOs
{
    public class AccountActivityDto
    {
        public string Action { get; set; }
        public DateTime Timestamp { get; set; }
        public string IpAddress { get; set; }
        public bool Success { get; set; }
    }
}

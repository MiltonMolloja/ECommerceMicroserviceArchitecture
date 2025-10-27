using System;

namespace Identity.Service.Queries.DTOs
{
    public class SessionDto
    {
        public int Id { get; set; }
        public string DeviceInfo { get; set; }
        public string IpAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsCurrent { get; set; }
    }
}

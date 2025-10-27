using System;

namespace Identity.Service.EventHandlers.Responses
{
    public class IdentityAccess
    {
        public bool Succeeded { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool Requires2FA { get; set; }
        public string UserId { get; set; }
    }
}

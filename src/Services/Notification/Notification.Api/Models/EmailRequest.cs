using System.Collections.Generic;

namespace Notification.Api.Models
{
    public class EmailRequest
    {
        public string To { get; set; }
        public string Template { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}

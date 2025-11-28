using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notification.Service.EventHandlers.Services
{
    public interface IEmailNotificationService
    {
        Task SendTemplatedEmailAsync(string to, string templateName, Dictionary<string, object> data);
    }
}

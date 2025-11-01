using Notification.Api.Models;
using System.Threading.Tasks;

namespace Notification.Api.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(EmailMessage message);
        Task SendTemplatedEmailAsync(string to, string templateName, object data);
    }
}

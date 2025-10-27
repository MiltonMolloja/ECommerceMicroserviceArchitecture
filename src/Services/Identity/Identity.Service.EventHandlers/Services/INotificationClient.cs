using System.Threading.Tasks;

namespace Identity.Service.EventHandlers.Services
{
    public interface INotificationClient
    {
        Task SendEmailAsync(string to, string template, object data);
        Task SendSmsAsync(string phoneNumber, string message);
    }
}

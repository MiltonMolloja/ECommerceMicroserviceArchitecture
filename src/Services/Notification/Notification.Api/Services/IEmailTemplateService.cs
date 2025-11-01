using System.Threading.Tasks;

namespace Notification.Api.Services
{
    public interface IEmailTemplateService
    {
        Task<(string subject, string htmlBody, string textBody)> RenderTemplateAsync(string templateName, object data);
    }
}

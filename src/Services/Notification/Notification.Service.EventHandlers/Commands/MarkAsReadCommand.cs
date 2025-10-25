using MediatR;

namespace Notification.Service.EventHandlers.Commands
{
    public class MarkAsReadCommand : INotification
    {
        public int NotificationId { get; set; }
        public int UserId { get; set; }
    }
}

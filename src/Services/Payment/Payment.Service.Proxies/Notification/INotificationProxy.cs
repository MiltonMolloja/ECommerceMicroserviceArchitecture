using System.Threading.Tasks;

namespace Payment.Service.Proxies.Notification
{
    public interface INotificationProxy
    {
        Task SendPaymentConfirmationAsync(int userId, int paymentId);
        Task SendPaymentFailedAsync(int userId, int paymentId, string reason);
        Task SendRefundProcessedAsync(int userId, int paymentId);
    }
}

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payment.Service.Proxies.Notification
{
    public interface INotificationProxy
    {
        Task SendPaymentConfirmationAsync(int userId, int paymentId);
        Task SendPaymentFailedAsync(PaymentFailedNotification notification);
        Task SendRefundProcessedAsync(int userId, int paymentId);
        Task SendOrderPlacedNotificationAsync(OrderPlacedNotification notification);
    }

    public class OrderPlacedNotification
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public List<OrderItemNotification> Items { get; set; }
        public string Subtotal { get; set; }
        public string ShippingCost { get; set; }
        public string Tax { get; set; }
        public string Total { get; set; }
        public string EstimatedDelivery { get; set; }
    }

    public class OrderItemNotification
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; }
    }

    public class PaymentFailedNotification
    {
        public int UserId { get; set; }
        public int PaymentId { get; set; }
        public string Reason { get; set; }
        public string Email { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public string AttemptDate { get; set; }
        public string Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string FailureReason { get; set; }
    }
}

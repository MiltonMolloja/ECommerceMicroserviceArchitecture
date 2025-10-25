using System.Threading.Tasks;

namespace Payment.Service.Proxies.Order
{
    public interface IOrderProxy
    {
        Task<OrderDto> GetOrderByIdAsync(int orderId);
        Task UpdateOrderPaymentStatusAsync(int orderId, string status);
    }

    public class OrderDto
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; }
        public int Status { get; set; }
        public int PaymentType { get; set; }
        public int ClientId { get; set; }
        public decimal Total { get; set; }
    }
}

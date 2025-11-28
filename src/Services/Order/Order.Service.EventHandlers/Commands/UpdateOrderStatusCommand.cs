using MediatR;
using static Order.Common.Enums;

namespace Order.Service.EventHandlers.Commands
{
    public class UpdateOrderStatusCommand : INotification
    {
        public int OrderId { get; set; }
        public OrderStatus NewStatus { get; set; }
        public string Reason { get; set; }  // Opcional, para cancelaciones
        public string PaymentTransactionId { get; set; }  // Opcional, para pagos
        public string PaymentGateway { get; set; }  // Opcional, para pagos
    }
}

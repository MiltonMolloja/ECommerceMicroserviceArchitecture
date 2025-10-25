using MediatR;

namespace Payment.Service.EventHandlers.Commands
{
    public class ProcessRefundCommand : INotification
    {
        public int PaymentId { get; set; }
        public decimal? Amount { get; set; } // Null = reembolso total
        public string Reason { get; set; }
        public int RequestedBy { get; set; } // Admin UserId
    }
}

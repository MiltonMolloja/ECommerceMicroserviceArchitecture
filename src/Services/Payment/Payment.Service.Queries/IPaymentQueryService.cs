using Payment.Service.Queries.DTOs;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Payment.Service.Queries
{
    public interface IPaymentQueryService
    {
        Task<PaymentDto> GetPaymentByIdAsync(int paymentId);
        Task<PaymentDto> GetPaymentByOrderIdAsync(int orderId);
        Task<List<PaymentDto>> GetUserPaymentHistoryAsync(int userId, int page = 1, int pageSize = 10);
        Task<List<PaymentTransactionDto>> GetPaymentTransactionsAsync(int paymentId);
    }
}

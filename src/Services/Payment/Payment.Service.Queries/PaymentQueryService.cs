using Microsoft.EntityFrameworkCore;
using Payment.Persistence.Database;
using Payment.Service.Queries.DTOs;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.Service.Queries
{
    public class PaymentQueryService : IPaymentQueryService
    {
        private readonly ApplicationDbContext _context;

        public PaymentQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<PaymentDto> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.PaymentDetail)
                .Include(p => p.Transactions)
                .Where(p => p.PaymentId == paymentId)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    UserId = p.UserId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status.ToString(),
                    PaymentMethod = p.PaymentMethod.ToString(),
                    TransactionId = p.TransactionId,
                    PaymentGateway = p.PaymentGateway,
                    PaymentDate = p.PaymentDate,
                    CreatedAt = p.CreatedAt,
                    PaymentDetail = p.PaymentDetail != null ? new PaymentDetailDto
                    {
                        CardLast4Digits = p.PaymentDetail.CardLast4Digits,
                        CardBrand = p.PaymentDetail.CardBrand,
                        BillingCountry = p.PaymentDetail.BillingCountry
                    } : null
                })
                .FirstOrDefaultAsync();
        }

        public async Task<PaymentDto> GetPaymentByOrderIdAsync(int orderId)
        {
            return await _context.Payments
                .Include(p => p.PaymentDetail)
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Status = p.Status.ToString(),
                    PaymentDate = p.PaymentDate
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<PaymentDto>> GetUserPaymentHistoryAsync(int userId, int page = 1, int pageSize = 10)
        {
            return await _context.Payments
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(p => new PaymentDto
                {
                    PaymentId = p.PaymentId,
                    OrderId = p.OrderId,
                    Amount = p.Amount,
                    Currency = p.Currency,
                    Status = p.Status.ToString(),
                    PaymentMethod = p.PaymentMethod.ToString(),
                    PaymentDate = p.PaymentDate,
                    CreatedAt = p.CreatedAt
                })
                .ToListAsync();
        }

        public async Task<List<PaymentTransactionDto>> GetPaymentTransactionsAsync(int paymentId)
        {
            return await _context.PaymentTransactions
                .Where(t => t.PaymentId == paymentId)
                .OrderByDescending(t => t.TransactionDate)
                .Select(t => new PaymentTransactionDto
                {
                    TransactionId = t.TransactionId,
                    TransactionType = t.TransactionType.ToString(),
                    Amount = t.Amount,
                    Status = t.Status.ToString(),
                    TransactionDate = t.TransactionDate,
                    ErrorMessage = t.ErrorMessage
                })
                .ToListAsync();
        }
    }
}

using Cart.Persistence.Database;
using Cart.Service.Queries.DTOs;
using Microsoft.EntityFrameworkCore;
using Service.Common.Mapping;
using System.Linq;
using System.Threading.Tasks;

namespace Cart.Service.Queries
{
    public interface ICartQueryService
    {
        Task<CartDto> GetByClientIdAsync(int clientId);
        Task<CartDto> GetBySessionIdAsync(string sessionId);
        Task<CartDto> GetByIdAsync(int cartId);
    }

    public class CartQueryService : ICartQueryService
    {
        private readonly ApplicationDbContext _context;

        public CartQueryService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CartDto> GetByClientIdAsync(int clientId)
        {
            var cart = await _context.ShoppingCarts
                .Include(x => x.Items)
                .Where(x => x.ClientId == clientId && x.Status == Domain.CartStatus.Active)
                .FirstOrDefaultAsync();

            return MapToDto(cart);
        }

        public async Task<CartDto> GetBySessionIdAsync(string sessionId)
        {
            var cart = await _context.ShoppingCarts
                .Include(x => x.Items)
                .Where(x => x.SessionId == sessionId && x.Status == Domain.CartStatus.Active)
                .FirstOrDefaultAsync();

            return MapToDto(cart);
        }

        public async Task<CartDto> GetByIdAsync(int cartId)
        {
            var cart = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartId == cartId);

            return MapToDto(cart);
        }

        private CartDto MapToDto(Domain.ShoppingCart cart)
        {
            if (cart == null) return null;

            var dto = new CartDto
            {
                CartId = cart.CartId,
                ClientId = cart.ClientId,
                SessionId = cart.SessionId,
                Status = cart.Status.ToString(),
                CouponCode = cart.CouponCode,
                CouponDiscountPercentage = cart.CouponDiscountPercentage,
                Subtotal = cart.Subtotal,
                CouponDiscount = cart.CouponDiscount,
                SubtotalAfterCoupon = cart.SubtotalAfterCoupon,
                TaxTotal = cart.TaxTotal,
                Total = cart.Total,
                ItemCount = cart.ItemCount,
                UniqueItemCount = cart.UniqueItemCount,
                IsEmpty = cart.IsEmpty,
                IsAnonymous = cart.IsAnonymous,
                HasCoupon = cart.HasCoupon,
                CreatedAt = cart.CreatedAt,
                UpdatedAt = cart.UpdatedAt,
                ExpiresAt = cart.ExpiresAt,
                Items = cart.Items?.Select(i => new CartItemDto
                {
                    CartItemId = i.CartItemId,
                    CartId = i.CartId,
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    ProductSKU = i.ProductSKU,
                    ProductImageUrl = i.ProductImageUrl,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    DiscountPercentage = i.DiscountPercentage,
                    TaxRate = i.TaxRate,
                    UnitPriceAfterDiscount = i.UnitPriceAfterDiscount,
                    LineTotal = i.LineTotal,
                    TaxAmount = i.TaxAmount,
                    LineTotalWithTax = i.LineTotalWithTax,
                    TotalSavings = i.TotalSavings,
                    HasDiscount = i.HasDiscount,
                    AddedAt = i.AddedAt,
                    UpdatedAt = i.UpdatedAt
                }).ToList() ?? new System.Collections.Generic.List<CartItemDto>()
            };

            return dto;
        }
    }
}

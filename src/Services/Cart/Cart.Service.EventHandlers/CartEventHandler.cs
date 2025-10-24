using Cart.Domain;
using Cart.Persistence.Database;
using Cart.Service.EventHandlers.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Cart.Service.EventHandlers
{
    public class CartEventHandler :
        INotificationHandler<AddItemToCartCommand>,
        INotificationHandler<UpdateCartItemQuantityCommand>,
        INotificationHandler<RemoveItemFromCartCommand>,
        INotificationHandler<ClearCartCommand>,
        INotificationHandler<ApplyCouponCommand>,
        INotificationHandler<RemoveCouponCommand>,
        INotificationHandler<ConvertCartToOrderCommand>,
        INotificationHandler<AssignCartToClientCommand>
    {
        private readonly ApplicationDbContext _context;

        public CartEventHandler(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(AddItemToCartCommand command, CancellationToken cancellationToken)
        {
            // Buscar o crear carrito
            ShoppingCart cart;

            if (command.ClientId.HasValue)
            {
                cart = await _context.ShoppingCarts
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.ClientId == command.ClientId && x.Status == CartStatus.Active, cancellationToken);
            }
            else
            {
                cart = await _context.ShoppingCarts
                    .Include(x => x.Items)
                    .FirstOrDefaultAsync(x => x.SessionId == command.SessionId && x.Status == CartStatus.Active, cancellationToken);
            }

            if (cart == null)
            {
                // Crear nuevo carrito
                cart = new ShoppingCart
                {
                    ClientId = command.ClientId,
                    SessionId = command.SessionId,
                    Status = CartStatus.Active,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow,
                    ExpiresAt = command.ClientId.HasValue ? (DateTime?)null : DateTime.UtcNow.AddDays(7)
                };

                _context.ShoppingCarts.Add(cart);
                await _context.SaveChangesAsync(cancellationToken);
            }

            // Agregar item usando método de negocio
            cart.AddItem(command.ProductId, command.Quantity, command.UnitPrice, command.TaxRate, command.ProductName);

            // Actualizar snapshot del producto en el item agregado
            var addedItem = cart.Items.First(i => i.ProductId == command.ProductId);
            addedItem.ProductSKU = command.ProductSKU;
            addedItem.ProductImageUrl = command.ProductImageUrl;
            addedItem.DiscountPercentage = command.DiscountPercentage;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(UpdateCartItemQuantityCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartId == command.CartId, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            var item = cart.Items.FirstOrDefault(i => i.ProductId == command.ProductId);
            if (item == null)
                throw new InvalidOperationException("Item not found in cart");

            item.UpdateQuantity(command.Quantity);
            cart.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(RemoveItemFromCartCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartId == command.CartId, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            cart.RemoveItem(command.ProductId);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ClearCartCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .Include(x => x.Items)
                .FirstOrDefaultAsync(x => x.CartId == command.CartId, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            cart.Clear();
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ApplyCouponCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(x => x.CartId == command.CartId, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            cart.ApplyCoupon(command.CouponCode, command.DiscountPercentage);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(RemoveCouponCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(x => x.CartId == command.CartId, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            cart.RemoveCoupon();
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(ConvertCartToOrderCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(x => x.CartId == command.CartId, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            cart.ConvertToOrder(command.OrderId);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task Handle(AssignCartToClientCommand command, CancellationToken cancellationToken)
        {
            var cart = await _context.ShoppingCarts
                .FirstOrDefaultAsync(x => x.SessionId == command.SessionId && x.Status == CartStatus.Active, cancellationToken);

            if (cart == null)
                throw new InvalidOperationException("Cart not found");

            cart.AssignToClient(command.ClientId);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

using MediatR;
using System;

namespace Cart.Service.EventHandlers.Commands
{
    public class AddItemToCartCommand : INotification
    {
        public int? ClientId { get; set; }
        public string SessionId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductSKU { get; set; }
        public string ProductImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal TaxRate { get; set; }
    }

    public class UpdateCartItemQuantityCommand : INotification
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class RemoveItemFromCartCommand : INotification
    {
        public int CartId { get; set; }
        public int ProductId { get; set; }
    }

    public class ClearCartCommand : INotification
    {
        public int CartId { get; set; }
    }

    public class ApplyCouponCommand : INotification
    {
        public int CartId { get; set; }
        public string CouponCode { get; set; }
        public decimal DiscountPercentage { get; set; }
    }

    public class RemoveCouponCommand : INotification
    {
        public int CartId { get; set; }
    }

    public class ConvertCartToOrderCommand : INotification
    {
        public int CartId { get; set; }
        public int OrderId { get; set; }
    }

    public class AssignCartToClientCommand : INotification
    {
        public string SessionId { get; set; }
        public int ClientId { get; set; }
    }
}

using MediatR;
using System.Collections.Generic;
using static Order.Common.Enums;

namespace Order.Service.EventHandlers.Commands
{
    public class OrderCreateCommand : IRequest<int>
    {
        public OrderPayment PaymentType { get; set; }
        public int? ClientId { get; set; }
        public IEnumerable<OrderCreateDetail> Items { get; set; } = new List<OrderCreateDetail>();

        // Shipping Address (REQUERIDA)
        public string ShippingRecipientName { get; set; }
        public string ShippingPhone { get; set; }
        public string ShippingAddressLine1 { get; set; }
        public string ShippingAddressLine2 { get; set; }
        public string ShippingCity { get; set; }
        public string ShippingState { get; set; }
        public string ShippingPostalCode { get; set; }
        public string ShippingCountry { get; set; }

        // Billing Address (OPCIONAL - usa shipping si no se provee)
        public string BillingAddressLine1 { get; set; }
        public string BillingCity { get; set; }
        public string BillingPostalCode { get; set; }
        public string BillingCountry { get; set; }
        public bool BillingSameAsShipping { get; set; } = true;
    }

    public class OrderCreateDetail
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}

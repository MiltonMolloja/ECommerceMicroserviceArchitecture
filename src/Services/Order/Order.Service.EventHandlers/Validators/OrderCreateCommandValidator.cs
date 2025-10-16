using System.Linq;
using FluentValidation;
using Order.Service.EventHandlers.Commands;

namespace Order.Service.EventHandlers.Validators
{
    public class OrderCreateCommandValidator : AbstractValidator<OrderCreateCommand>
    {
        public OrderCreateCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("Client ID must be greater than zero");

            RuleFor(x => x.PaymentType)
                .IsInEnum().WithMessage("Invalid payment type");

            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items list cannot be null")
                .NotEmpty().WithMessage("Order must contain at least one item")
                .Must(items => items != null && items.Count() <= 100)
                .WithMessage("Order cannot contain more than 100 items");

            RuleForEach(x => x.Items).SetValidator(new OrderCreateDetailValidator());
        }
    }

    public class OrderCreateDetailValidator : AbstractValidator<OrderCreateDetail>
    {
        public OrderCreateDetailValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than zero");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero")
                .LessThan(10000).WithMessage("Quantity must be less than 10,000");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThan(1000000).WithMessage("Price must be less than 1,000,000")
                .PrecisionScale(18, 2, false).WithMessage("Price must have at most 2 decimal places");
        }
    }
}

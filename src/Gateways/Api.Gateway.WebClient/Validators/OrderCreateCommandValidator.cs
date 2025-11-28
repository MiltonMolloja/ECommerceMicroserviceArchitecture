using Api.Gateway.Models.Orders.Commands;
using FluentValidation;
using System.Linq;
using static Api.Gateway.Models.Order.Commons.Enums;

namespace Api.Gateway.WebClient.Validators
{
    public class OrderCreateCommandValidator : AbstractValidator<OrderCreateCommand>
    {
        public OrderCreateCommandValidator()
        {
            // ClientId ya no se valida aquí - se extrae del JWT token en el controller del servicio Order

            RuleFor(x => x.PaymentType)
                .IsInEnum()
                .WithMessage("Invalid payment type");

            RuleFor(x => x.Items)
                .NotNull()
                .WithMessage("Items list cannot be null")
                .NotEmpty()
                .WithMessage("Items list cannot be empty")
                .Must(items => items != null && items.Count() <= 100)
                .WithMessage("Items list cannot contain more than 100 items");

            RuleForEach(x => x.Items)
                .SetValidator(new OrderCreateDetailValidator());
        }
    }

    public class OrderCreateDetailValidator : AbstractValidator<OrderCreateDetail>
    {
        public OrderCreateDetailValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0)
                .WithMessage("Product ID must be greater than zero");

            RuleFor(x => x.Quantity)
                .GreaterThan(0)
                .WithMessage("Quantity must be greater than zero");

            RuleFor(x => x.Price)
                .GreaterThan(0)
                .WithMessage("Price must be greater than zero");
        }
    }
}

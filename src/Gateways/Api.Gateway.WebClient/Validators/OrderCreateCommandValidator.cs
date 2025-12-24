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

            // Shipping Address (REQUERIDA) - Must match Order API validation
            RuleFor(x => x.ShippingRecipientName)
                .NotEmpty().WithMessage("Recipient name is required")
                .MaximumLength(200).WithMessage("Recipient name must not exceed 200 characters");

            RuleFor(x => x.ShippingPhone)
                .NotEmpty().WithMessage("Phone is required")
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters");

            RuleFor(x => x.ShippingAddressLine1)
                .NotEmpty().WithMessage("Address line 1 is required")
                .MaximumLength(200).WithMessage("Address line 1 must not exceed 200 characters");

            RuleFor(x => x.ShippingAddressLine2)
                .MaximumLength(200).WithMessage("Address line 2 must not exceed 200 characters");

            RuleFor(x => x.ShippingCity)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters");

            RuleFor(x => x.ShippingState)
                .MaximumLength(100).WithMessage("State must not exceed 100 characters");

            RuleFor(x => x.ShippingPostalCode)
                .NotEmpty().WithMessage("Postal code is required")
                .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

            RuleFor(x => x.ShippingCountry)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters");

            // Billing Address (solo validar si BillingSameAsShipping = false)
            RuleFor(x => x.BillingAddressLine1)
                .NotEmpty().When(x => !x.BillingSameAsShipping)
                .WithMessage("Billing address is required when different from shipping")
                .MaximumLength(200).WithMessage("Billing address must not exceed 200 characters");

            RuleFor(x => x.BillingCity)
                .NotEmpty().When(x => !x.BillingSameAsShipping)
                .WithMessage("Billing city is required when different from shipping")
                .MaximumLength(100).WithMessage("Billing city must not exceed 100 characters");

            RuleFor(x => x.BillingPostalCode)
                .NotEmpty().When(x => !x.BillingSameAsShipping)
                .WithMessage("Billing postal code is required when different from shipping")
                .MaximumLength(20).WithMessage("Billing postal code must not exceed 20 characters");

            RuleFor(x => x.BillingCountry)
                .NotEmpty().When(x => !x.BillingSameAsShipping)
                .WithMessage("Billing country is required when different from shipping")
                .MaximumLength(100).WithMessage("Billing country must not exceed 100 characters");
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
                .WithMessage("Price must be greater than zero")
                .LessThan(1000000).WithMessage("Price must be less than 1,000,000");
        }
    }
}

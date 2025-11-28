using FluentValidation;
using Payment.Service.EventHandlers.Commands;

namespace Payment.Service.EventHandlers.Validators
{
    public class ProcessPaymentCommandValidator : AbstractValidator<ProcessPaymentCommand>
    {
        public ProcessPaymentCommandValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0)
                .WithMessage("OrderId must be greater than 0");

            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be greater than 0");

            RuleFor(x => x.PaymentMethodId)
                .NotEmpty()
                .WithMessage("PaymentMethodId is required (e.g., master, visa, amex)");

            RuleFor(x => x.Token)
                .NotEmpty()
                .WithMessage("MercadoPago token is required");

            RuleFor(x => x.Installments)
                .GreaterThan(0)
                .WithMessage("Installments must be greater than 0")
                .LessThanOrEqualTo(24)
                .WithMessage("Installments cannot exceed 24");

            RuleFor(x => x.BillingAddress)
                .NotEmpty()
                .WithMessage("Billing address is required");

            RuleFor(x => x.BillingCity)
                .NotEmpty()
                .WithMessage("Billing city is required");

            RuleFor(x => x.BillingCountry)
                .NotEmpty()
                .WithMessage("Billing country is required");

            RuleFor(x => x.BillingZipCode)
                .NotEmpty()
                .WithMessage("Billing zip code is required");
        }
    }
}

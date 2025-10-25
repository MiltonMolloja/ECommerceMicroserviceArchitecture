using FluentValidation;
using Payment.Domain;
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

            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Amount must be greater than 0")
                .LessThanOrEqualTo(1000000)
                .WithMessage("Amount cannot exceed 1,000,000");

            RuleFor(x => x.Currency)
                .NotEmpty()
                .Length(3)
                .WithMessage("Currency must be a 3-letter code (e.g., USD, EUR)");

            RuleFor(x => x.PaymentMethod)
                .IsInEnum()
                .WithMessage("Invalid payment method");

            // Validación condicional para tarjeta
            When(x => x.PaymentMethod == PaymentMethod.CreditCard ||
                     x.PaymentMethod == PaymentMethod.DebitCard, () =>
            {
                RuleFor(x => x.PaymentToken)
                    .NotEmpty()
                    .WithMessage("Payment token is required for card payments");
            });

            RuleFor(x => x.BillingCountry)
                .NotEmpty()
                .WithMessage("Billing country is required");
        }
    }
}

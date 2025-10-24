using FluentValidation;
using Customer.Service.EventHandlers.Commands;
using System.Linq;

namespace Customer.Service.EventHandlers.Validators
{
    public class ClientCreateCommandValidator : AbstractValidator<ClientCreateCommand>
    {
        public ClientCreateCommandValidator()
        {
            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format")
                .MaximumLength(256).WithMessage("Email must not exceed 256 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Phone));
        }
    }

    public class ClientUpdateProfileCommandValidator : AbstractValidator<ClientUpdateProfileCommand>
    {
        public ClientUpdateProfileCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("ClientId must be greater than 0");

            RuleFor(x => x.FirstName)
                .NotEmpty().WithMessage("First name is required")
                .MaximumLength(100).WithMessage("First name must not exceed 100 characters");

            RuleFor(x => x.LastName)
                .NotEmpty().WithMessage("Last name is required")
                .MaximumLength(100).WithMessage("Last name must not exceed 100 characters");

            RuleFor(x => x.Phone)
                .MaximumLength(20).WithMessage("Phone must not exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.MobilePhone)
                .MaximumLength(20).WithMessage("Mobile phone must not exceed 20 characters")
                .When(x => !string.IsNullOrEmpty(x.MobilePhone));

            RuleFor(x => x.Gender)
                .Must(g => g == null || new[] { "M", "F", "Other", "PreferNotToSay" }.Contains(g))
                .WithMessage("Gender must be M, F, Other, or PreferNotToSay");
        }
    }

    public class ClientAddressCreateCommandValidator : AbstractValidator<ClientAddressCreateCommand>
    {
        public ClientAddressCreateCommandValidator()
        {
            RuleFor(x => x.ClientId)
                .GreaterThan(0).WithMessage("ClientId must be greater than 0");

            RuleFor(x => x.AddressType)
                .NotEmpty().WithMessage("Address type is required")
                .Must(t => new[] { "Shipping", "Billing", "Both" }.Contains(t))
                .WithMessage("Address type must be Shipping, Billing, or Both");

            RuleFor(x => x.RecipientName)
                .NotEmpty().WithMessage("Recipient name is required")
                .MaximumLength(200).WithMessage("Recipient name must not exceed 200 characters");

            RuleFor(x => x.AddressLine1)
                .NotEmpty().WithMessage("Address line 1 is required")
                .MaximumLength(200).WithMessage("Address line 1 must not exceed 200 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters");

            RuleFor(x => x.PostalCode)
                .NotEmpty().WithMessage("Postal code is required")
                .MaximumLength(20).WithMessage("Postal code must not exceed 20 characters");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required")
                .MaximumLength(100).WithMessage("Country must not exceed 100 characters");
        }
    }
}

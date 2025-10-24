using Cart.Service.EventHandlers.Commands;
using FluentValidation;

namespace Cart.Service.EventHandlers.Validators
{
    public class AddItemToCartCommandValidator : AbstractValidator<AddItemToCartCommand>
    {
        public AddItemToCartCommandValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than 0");

            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(200).WithMessage("Product name cannot exceed 200 characters");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100");

            RuleFor(x => x.UnitPrice)
                .GreaterThanOrEqualTo(0).WithMessage("Unit price cannot be negative");

            RuleFor(x => x.DiscountPercentage)
                .GreaterThanOrEqualTo(0).WithMessage("Discount percentage cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Discount percentage cannot exceed 100");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0).WithMessage("Tax rate cannot be negative")
                .LessThanOrEqualTo(100).WithMessage("Tax rate cannot exceed 100");

            // Debe tener ClientId O SessionId
            RuleFor(x => x)
                .Must(x => x.ClientId.HasValue || !string.IsNullOrEmpty(x.SessionId))
                .WithMessage("Either ClientId or SessionId must be provided");
        }
    }
}

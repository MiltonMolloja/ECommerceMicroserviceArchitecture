using FluentValidation;
using Catalog.Service.EventHandlers.Commands;

namespace Catalog.Service.EventHandlers.Validators
{
    public class ProductCreateCommandValidator : AbstractValidator<ProductCreateCommand>
    {
        public ProductCreateCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Product name is required")
                .MaximumLength(255).WithMessage("Product name must not exceed 255 characters")
                .MinimumLength(3).WithMessage("Product name must be at least 3 characters");

            RuleFor(x => x.Description)
                .NotEmpty().WithMessage("Product description is required")
                .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters")
                .MinimumLength(10).WithMessage("Product description must be at least 10 characters");

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThan(1000000).WithMessage("Price must be less than 1,000,000")
                .PrecisionScale(18, 2, false).WithMessage("Price must have at most 2 decimal places");
        }
    }
}

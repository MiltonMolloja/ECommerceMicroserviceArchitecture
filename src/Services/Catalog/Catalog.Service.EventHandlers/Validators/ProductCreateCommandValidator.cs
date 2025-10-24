using FluentValidation;
using Catalog.Service.EventHandlers.Commands;

namespace Catalog.Service.EventHandlers.Validators
{
    public class ProductCreateCommandValidator : AbstractValidator<ProductCreateCommand>
    {
        public ProductCreateCommandValidator()
        {
            // Multiidioma - Nombre
            RuleFor(x => x.NameSpanish)
                .NotEmpty().WithMessage("Product name in Spanish is required")
                .MaximumLength(200).WithMessage("Product name in Spanish must not exceed 200 characters")
                .MinimumLength(3).WithMessage("Product name in Spanish must be at least 3 characters");

            RuleFor(x => x.NameEnglish)
                .NotEmpty().WithMessage("Product name in English is required")
                .MaximumLength(200).WithMessage("Product name in English must not exceed 200 characters")
                .MinimumLength(3).WithMessage("Product name in English must be at least 3 characters");

            // Multiidioma - Descripción
            RuleFor(x => x.DescriptionSpanish)
                .NotEmpty().WithMessage("Product description in Spanish is required")
                .MaximumLength(1000).WithMessage("Product description in Spanish must not exceed 1000 characters")
                .MinimumLength(10).WithMessage("Product description in Spanish must be at least 10 characters");

            RuleFor(x => x.DescriptionEnglish)
                .NotEmpty().WithMessage("Product description in English is required")
                .MaximumLength(1000).WithMessage("Product description in English must not exceed 1000 characters")
                .MinimumLength(10).WithMessage("Product description in English must be at least 10 characters");

            // Identificación
            RuleFor(x => x.SKU)
                .NotEmpty().WithMessage("SKU is required")
                .MaximumLength(50).WithMessage("SKU must not exceed 50 characters");

            RuleFor(x => x.Slug)
                .NotEmpty().WithMessage("Slug is required")
                .MaximumLength(200).WithMessage("Slug must not exceed 200 characters")
                .Matches("^[a-z0-9-]+$").WithMessage("Slug must contain only lowercase letters, numbers, and hyphens");

            RuleFor(x => x.Brand)
                .MaximumLength(100).WithMessage("Brand must not exceed 100 characters");

            // Pricing
            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero")
                .LessThan(1000000).WithMessage("Price must be less than 1,000,000")
                .PrecisionScale(18, 2, false).WithMessage("Price must have at most 2 decimal places");

            RuleFor(x => x.DiscountPercentage)
                .GreaterThanOrEqualTo(0).WithMessage("Discount percentage must be greater than or equal to zero")
                .LessThanOrEqualTo(100).WithMessage("Discount percentage must be less than or equal to 100");

            RuleFor(x => x.TaxRate)
                .GreaterThanOrEqualTo(0).WithMessage("Tax rate must be greater than or equal to zero")
                .LessThanOrEqualTo(100).WithMessage("Tax rate must be less than or equal to 100");

            // SEO
            RuleFor(x => x.MetaTitle)
                .MaximumLength(100).WithMessage("Meta title must not exceed 100 characters");

            RuleFor(x => x.MetaDescription)
                .MaximumLength(300).WithMessage("Meta description must not exceed 300 characters");

            RuleFor(x => x.MetaKeywords)
                .MaximumLength(500).WithMessage("Meta keywords must not exceed 500 characters");
        }
    }
}

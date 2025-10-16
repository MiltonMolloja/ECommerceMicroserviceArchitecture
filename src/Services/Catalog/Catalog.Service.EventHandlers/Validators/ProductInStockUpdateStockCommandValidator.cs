using System.Linq;
using FluentValidation;
using Catalog.Service.EventHandlers.Commands;

namespace Catalog.Service.EventHandlers.Validators
{
    public class ProductInStockUpdateStockCommandValidator : AbstractValidator<ProductInStockUpdateStockCommand>
    {
        public ProductInStockUpdateStockCommandValidator()
        {
            RuleFor(x => x.Items)
                .NotNull().WithMessage("Items list cannot be null")
                .NotEmpty().WithMessage("At least one item is required")
                .Must(items => items != null && items.Count() <= 100)
                .WithMessage("Cannot update more than 100 items at once");

            RuleForEach(x => x.Items).SetValidator(new ProductInStockUpdateItemValidator());
        }
    }

    public class ProductInStockUpdateItemValidator : AbstractValidator<ProductInStockUpdateItem>
    {
        public ProductInStockUpdateItemValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("Product ID must be greater than zero");

            RuleFor(x => x.Stock)
                .GreaterThanOrEqualTo(0).WithMessage("Stock cannot be negative")
                .LessThan(1000000).WithMessage("Stock must be less than 1,000,000");

            RuleFor(x => x.Action)
                .IsInEnum().WithMessage("Invalid stock action");
        }
    }
}

using Catalog.Service.Queries.DTOs;
using FluentValidation;
using System;
using System.Linq;

namespace Catalog.Service.Queries.Validators
{
    /// <summary>
    /// Validador para ProductSearchRequest
    /// </summary>
    public class ProductSearchRequestValidator : AbstractValidator<ProductSearchRequest>
    {
        public ProductSearchRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .InclusiveBetween(1, 100)
                .WithMessage("PageSize must be between 1 and 100");

            RuleFor(x => x.Query)
                .MaximumLength(200)
                .WithMessage("Query cannot exceed 200 characters");

            RuleFor(x => x.MinPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinPrice.HasValue)
                .WithMessage("MinPrice cannot be negative");

            RuleFor(x => x.MaxPrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MaxPrice.HasValue)
                .WithMessage("MaxPrice cannot be negative");

            RuleFor(x => x.MaxPrice)
                .GreaterThan(x => x.MinPrice)
                .When(x => x.MinPrice.HasValue && x.MaxPrice.HasValue)
                .WithMessage("MaxPrice must be greater than MinPrice");

            RuleFor(x => x.MinRating)
                .InclusiveBetween(0, 5)
                .When(x => x.MinRating.HasValue)
                .WithMessage("MinRating must be between 0 and 5");

            RuleFor(x => x.BrandIds)
                .Must(BeValidCommaSeparatedList)
                .When(x => !string.IsNullOrWhiteSpace(x.BrandIds))
                .WithMessage("BrandIds must be a valid comma-separated list");
        }

        private bool BeValidCommaSeparatedList(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return true;

            var items = value.Split(',', StringSplitOptions.RemoveEmptyEntries);
            return items.All(item => !string.IsNullOrWhiteSpace(item.Trim()));
        }
    }
}

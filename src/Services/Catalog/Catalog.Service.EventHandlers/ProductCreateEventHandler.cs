using Catalog.Domain;
using Catalog.Persistence.Database;
using Catalog.Service.EventHandlers.Commands;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Catalog.Service.EventHandlers
{
    public class ProductCreateEventHandler :
        INotificationHandler<ProductCreateCommand>
    {
        private readonly ApplicationDbContext _context;

        public ProductCreateEventHandler(
            ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task Handle(ProductCreateCommand notification, CancellationToken cancellationToken)
        {
            await _context.AddAsync(new Product
            {
                // Multiidioma
                NameSpanish = notification.NameSpanish,
                NameEnglish = notification.NameEnglish,
                DescriptionSpanish = notification.DescriptionSpanish,
                DescriptionEnglish = notification.DescriptionEnglish,

                // Identificación
                SKU = notification.SKU,
                Brand = notification.Brand,
                Slug = notification.Slug,

                // Pricing
                Price = notification.Price,
                OriginalPrice = notification.OriginalPrice,
                DiscountPercentage = notification.DiscountPercentage,
                TaxRate = notification.TaxRate,

                // Media
                Images = notification.Images,

                // SEO
                MetaTitle = notification.MetaTitle,
                MetaDescription = notification.MetaDescription,
                MetaKeywords = notification.MetaKeywords,

                // Flags
                IsActive = notification.IsActive,
                IsFeatured = notification.IsFeatured
            });

            await _context.SaveChangesAsync();
        }
    }
}

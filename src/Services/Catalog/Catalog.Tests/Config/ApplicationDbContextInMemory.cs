using Catalog.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Tests.Config
{
    public static class ApplicationDbContextInMemory
    {
        public static ApplicationDbContext Get()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: $"CatalogTestDb_{Guid.NewGuid()}")
                .Options;

            return new ApplicationDbContext(options);
        }
    }
}

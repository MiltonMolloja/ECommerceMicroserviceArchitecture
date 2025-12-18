using Microsoft.EntityFrameworkCore;
using Order.Persistence.Database;

namespace Order.Tests.Config;

public static class ApplicationDbContextInMemory
{
    public static ApplicationDbContext Get()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"OrderDb_{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}

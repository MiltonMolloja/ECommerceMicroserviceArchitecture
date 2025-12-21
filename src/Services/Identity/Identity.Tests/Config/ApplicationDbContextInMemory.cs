using Identity.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Identity.Tests.Config;

public static class ApplicationDbContextInMemory
{
    public static ApplicationDbContext Get()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"IdentityDb_{Guid.NewGuid()}")
            .Options;

        return new ApplicationDbContext(options);
    }
}

using Cart.Persistence.Database;
using Microsoft.EntityFrameworkCore;

namespace Cart.Tests.Config;

/// <summary>
/// Helper para crear instancias de ApplicationDbContext en memoria para tests.
/// </summary>
public static class ApplicationDbContextInMemory
{
    /// <summary>
    /// Crea una nueva instancia de ApplicationDbContext con base de datos en memoria.
    /// Cada llamada crea una base de datos única para evitar conflictos entre tests.
    /// </summary>
    public static ApplicationDbContext Get()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"CartTestDb_{Guid.NewGuid()}")
            .Options;

        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return context;
    }
}

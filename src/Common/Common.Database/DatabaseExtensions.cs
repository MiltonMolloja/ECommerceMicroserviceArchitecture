using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Database;

/// <summary>
/// Extension methods for configuring database providers.
/// Supports both SQL Server and PostgreSQL based on configuration.
/// </summary>
public static class DatabaseExtensions
{
    /// <summary>
    /// Database provider types supported by the application.
    /// </summary>
    public enum DatabaseProvider
    {
        SqlServer,
        PostgreSQL
    }

    /// <summary>
    /// Adds a DbContext with automatic database provider selection based on configuration.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to register.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="schemaName">The database schema name for migrations history table.</param>
    /// <param name="connectionStringName">The name of the connection string in configuration (default: "DefaultConnection").</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddDatabaseContext<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        string schemaName,
        string connectionStringName = "DefaultConnection")
        where TContext : DbContext
    {
        var connectionString = configuration.GetConnectionString(connectionStringName)
            ?? throw new InvalidOperationException($"Connection string '{connectionStringName}' not found in configuration.");

        var providerString = configuration["Database:Provider"] ?? "SqlServer";
        var provider = Enum.Parse<DatabaseProvider>(providerString, ignoreCase: true);

        services.AddDbContext<TContext>(options =>
        {
            ConfigureDbContext(options, provider, connectionString, schemaName);
        });

        return services;
    }

    /// <summary>
    /// Configures the DbContext options based on the selected provider.
    /// </summary>
    private static void ConfigureDbContext(
        DbContextOptionsBuilder options,
        DatabaseProvider provider,
        string connectionString,
        string schemaName)
    {
        switch (provider)
        {
            case DatabaseProvider.PostgreSQL:
                options.UseNpgsql(connectionString, npgsqlOptions =>
                {
                    npgsqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schemaName);
                    // PostgreSQL-specific optimizations
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                });
                break;

            case DatabaseProvider.SqlServer:
            default:
                options.UseSqlServer(connectionString, sqlOptions =>
                {
                    sqlOptions.MigrationsHistoryTable("__EFMigrationsHistory", schemaName);
                    // SQL Server-specific optimizations
                    sqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(30),
                        errorNumbersToAdd: null);
                });
                break;
        }
    }

    /// <summary>
    /// Gets the current database provider from configuration.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The configured database provider.</returns>
    public static DatabaseProvider GetDatabaseProvider(this IConfiguration configuration)
    {
        var providerString = configuration["Database:Provider"] ?? "SqlServer";
        return Enum.Parse<DatabaseProvider>(providerString, ignoreCase: true);
    }

    /// <summary>
    /// Checks if PostgreSQL is the configured database provider.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>True if PostgreSQL is configured, false otherwise.</returns>
    public static bool IsPostgreSQL(this IConfiguration configuration)
    {
        return configuration.GetDatabaseProvider() == DatabaseProvider.PostgreSQL;
    }

    /// <summary>
    /// Checks if SQL Server is the configured database provider.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>True if SQL Server is configured, false otherwise.</returns>
    public static bool IsSqlServer(this IConfiguration configuration)
    {
        return configuration.GetDatabaseProvider() == DatabaseProvider.SqlServer;
    }
}

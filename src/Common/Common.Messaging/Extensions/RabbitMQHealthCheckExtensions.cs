using HealthChecks.RabbitMQ;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Common.Messaging.Extensions;

/// <summary>
/// Extensiones para agregar Health Checks de RabbitMQ.
/// </summary>
public static class RabbitMQHealthCheckExtensions
{
    /// <summary>
    /// Agrega Health Check de RabbitMQ al IHealthChecksBuilder.
    /// </summary>
    /// <param name="builder">Health checks builder.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="name">Nombre del health check (default: "rabbitmq").</param>
    /// <param name="failureStatus">Estado en caso de fallo (default: Degraded).</param>
    /// <param name="tags">Tags para el health check.</param>
    /// <returns>IHealthChecksBuilder para encadenar.</returns>
    public static IHealthChecksBuilder AddRabbitMQHealthCheck(
        this IHealthChecksBuilder builder,
        IConfiguration configuration,
        string name = "rabbitmq",
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null)
    {
        var settings = new RabbitMQSettings();
        configuration.GetSection("RabbitMQ").Bind(settings);

        // Si RabbitMQ está deshabilitado, agregar un check que siempre pasa
        if (!settings.Enabled)
        {
            return builder.AddCheck(name, () => HealthCheckResult.Healthy("RabbitMQ is disabled"), tags ?? Array.Empty<string>());
        }

        var connectionString = new Uri($"amqp://{settings.Username}:{settings.Password}@{settings.Host}:{settings.Port}{settings.VirtualHost}");

        return builder.AddRabbitMQ(
            rabbitConnectionString: connectionString,
            name: name,
            failureStatus: failureStatus ?? HealthStatus.Degraded,
            tags: tags ?? new[] { "rabbitmq", "messaging", "ready" });
    }

    /// <summary>
    /// Agrega Health Checks de RabbitMQ usando la configuración por defecto.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>IHealthChecksBuilder para encadenar más health checks.</returns>
    public static IHealthChecksBuilder AddRabbitMQHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddHealthChecks()
            .AddRabbitMQHealthCheck(configuration);
    }
}

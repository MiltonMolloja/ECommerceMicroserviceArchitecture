using Common.Messaging.DeadLetter;
using MassTransit;
using MassTransit.Topology;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Common.Messaging.Extensions;

/// <summary>
/// Extensiones para configurar MassTransit con RabbitMQ.
/// </summary>
public static class MassTransitExtensions
{
    /// <summary>
    /// Agrega MassTransit con RabbitMQ al contenedor de servicios.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="configureConsumers">Acción opcional para configurar consumidores.</param>
    /// <returns>Colección de servicios.</returns>
    public static IServiceCollection AddRabbitMQMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureConsumers = null)
    {
        var settings = new RabbitMQSettings();
        configuration.GetSection("RabbitMQ").Bind(settings);

        // Si RabbitMQ está deshabilitado, no configurar nada
        if (!settings.Enabled)
        {
            // Registrar un bus en memoria para desarrollo/testing
            services.AddMassTransit(x =>
            {
                configureConsumers?.Invoke(x);
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.ConfigureEndpoints(context);
                });
            });
            return services;
        }

        services.AddMassTransit(x =>
        {
            // Configurar consumidores si se proporcionan
            configureConsumers?.Invoke(x);

            // Usar KebabCaseEndpointNameFormatter globalmente para consistencia
            x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter(false));

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(settings.Host, (ushort)settings.Port, settings.VirtualHost, h =>
                {
                    h.Username(settings.Username);
                    h.Password(settings.Password);
                });

                // Configurar el formateador de nombres de mensajes para usar kebab-case
                cfg.MessageTopology.SetEntityNameFormatter(new KebabCaseEntityNameFormatter());

                // Configuración de reintentos con backoff exponencial
                cfg.UseMessageRetry(r =>
                {
                    r.Intervals(
                        TimeSpan.FromSeconds(settings.RetryIntervalSeconds),
                        TimeSpan.FromSeconds(settings.RetryIntervalSeconds * 2),
                        TimeSpan.FromSeconds(settings.RetryIntervalSeconds * 4)
                    );
                });

                // Prefetch count
                cfg.PrefetchCount = settings.PrefetchCount;

                // Dead Letter Queue Configuration
                if (settings.EnableDeadLetterQueue)
                {
                    // Configurar Dead Letter Exchange para mensajes que fallan después de todos los reintentos
                    cfg.UseDelayedRedelivery(r =>
                    {
                        r.Intervals(
                            TimeSpan.FromMinutes(5),
                            TimeSpan.FromMinutes(15),
                            TimeSpan.FromMinutes(30)
                        );
                    });

                    // Configurar el manejo de excepciones para enviar a DLQ
                    cfg.UseInMemoryOutbox(context);

                    // Registrar el observador de Dead Letter
                    var loggerFactory = context.GetRequiredService<ILoggerFactory>();
                    var observer = new DeadLetterObserver(loggerFactory.CreateLogger<DeadLetterObserver>());
                    cfg.ConnectReceiveObserver(observer);
                }

                // Configurar endpoints automáticamente
                cfg.ConfigureEndpoints(context, new DeadLetterEndpointNameFormatter(settings));
            });
        });

        return services;
    }

    /// <summary>
    /// Agrega MassTransit con RabbitMQ y registra consumidores de un assembly.
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <param name="consumerAssembly">Assembly que contiene los consumidores.</param>
    /// <returns>Colección de servicios.</returns>
    public static IServiceCollection AddRabbitMQMessaging(
        this IServiceCollection services,
        IConfiguration configuration,
        Assembly consumerAssembly)
    {
        return services.AddRabbitMQMessaging(configuration, x =>
        {
            x.AddConsumers(consumerAssembly);
        });
    }

    /// <summary>
    /// Agrega MassTransit solo como publicador (sin consumidores).
    /// </summary>
    /// <param name="services">Colección de servicios.</param>
    /// <param name="configuration">Configuración de la aplicación.</param>
    /// <returns>Colección de servicios.</returns>
    public static IServiceCollection AddRabbitMQPublisher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddRabbitMQMessaging(configuration, configureConsumers: null);
    }
}

/// <summary>
/// Formateador de nombres de endpoints para Dead Letter Queues.
/// </summary>
internal class DeadLetterEndpointNameFormatter : IEndpointNameFormatter
{
    private readonly IEndpointNameFormatter _defaultFormatter;

    public DeadLetterEndpointNameFormatter(RabbitMQSettings settings)
    {
        _ = settings; // Reserved for future custom DLQ naming based on settings
        _defaultFormatter = new KebabCaseEndpointNameFormatter(false);
    }

    public string Separator => _defaultFormatter.Separator;

    public string TemporaryEndpoint(string tag) => _defaultFormatter.TemporaryEndpoint(tag);

    public string Consumer<T>() where T : class, IConsumer => _defaultFormatter.Consumer<T>();

    public string Message<T>() where T : class => _defaultFormatter.Message<T>();

    public string Saga<T>() where T : class, ISaga => _defaultFormatter.Saga<T>();

    public string ExecuteActivity<T, TArguments>() where T : class, IExecuteActivity<TArguments> where TArguments : class
        => _defaultFormatter.ExecuteActivity<T, TArguments>();

    public string CompensateActivity<T, TLog>() where T : class, ICompensateActivity<TLog> where TLog : class
        => _defaultFormatter.CompensateActivity<T, TLog>();

    public string SanitizeName(string name) => _defaultFormatter.SanitizeName(name);
}

/// <summary>
/// Formateador de nombres de entidades (exchanges, queues) en kebab-case.
/// Convierte PaymentCompletedEvent a payment-completed-event.
/// </summary>
internal class KebabCaseEntityNameFormatter : IEntityNameFormatter
{
    public string FormatEntityName<T>()
    {
        return ToKebabCase(typeof(T).Name);
    }

    private static string ToKebabCase(string name)
    {
        if (string.IsNullOrEmpty(name))
            return name;

        // Remover sufijo "Event" si existe
        if (name.EndsWith("Event"))
            name = name.Substring(0, name.Length - 5);

        // Insertar guiones antes de mayúsculas y convertir a minúsculas
        var result = Regex.Replace(name, "(?<!^)([A-Z])", "-$1").ToLowerInvariant();
        return result;
    }
}

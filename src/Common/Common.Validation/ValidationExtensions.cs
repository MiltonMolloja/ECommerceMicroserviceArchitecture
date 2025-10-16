using System.Reflection;
using FluentValidation;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Validation;

/// <summary>
/// Extension methods for registering validators
/// </summary>
public static class ValidationExtensions
{
    /// <summary>
    /// Adds FluentValidation validators from the specified assembly
    /// </summary>
    public static IServiceCollection AddValidatorsFromAssembly(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.AddValidatorsFromAssembly(assembly, ServiceLifetime.Scoped);
        return services;
    }

    /// <summary>
    /// Adds the MediatR validation pipeline behavior for IRequest and decorates IMediator to validate INotification
    /// </summary>
    public static IServiceCollection AddValidationBehavior(this IServiceCollection services)
    {
        // Add pipeline behavior for IRequest validation
        services.AddScoped(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // Decorate IMediator to validate INotification commands
        services.Decorate<MediatR.IMediator, ValidatingMediator>();

        return services;
    }

    /// <summary>
    /// Adds the validation exception middleware to the application pipeline
    /// </summary>
    public static IApplicationBuilder UseValidationExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ValidationExceptionMiddleware>();
    }
}

using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Validation;

/// <summary>
/// MediatR decorator that validates INotification before publishing
/// </summary>
public class ValidatingMediator : IMediator
{
    private readonly IMediator _mediator;
    private readonly IServiceProvider _serviceProvider;

    public ValidatingMediator(IMediator mediator, IServiceProvider serviceProvider)
    {
        _mediator = mediator;
        _serviceProvider = serviceProvider;
    }

    public async Task Publish(object notification, CancellationToken cancellationToken = default)
    {
        // Validate notification before publishing
        await ValidateAsync(notification, cancellationToken);
        await _mediator.Publish(notification, cancellationToken);
    }

    public async Task Publish<TNotification>(TNotification notification, CancellationToken cancellationToken = default)
        where TNotification : INotification
    {
        // Validate notification before publishing
        await ValidateAsync(notification, cancellationToken);
        await _mediator.Publish(notification, cancellationToken);
    }

    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        // Validation for IRequest is handled by ValidationBehavior
        return await _mediator.Send(request, cancellationToken);
    }

    public async Task Send<TRequest>(TRequest request, CancellationToken cancellationToken = default)
        where TRequest : IRequest
    {
        // Validation for IRequest is handled by ValidationBehavior
        await _mediator.Send(request, cancellationToken);
    }

    public async Task<object?> Send(object request, CancellationToken cancellationToken = default)
    {
        // Validation for IRequest is handled by ValidationBehavior
        return await _mediator.Send(request, cancellationToken);
    }

    public IAsyncEnumerable<TResponse> CreateStream<TResponse>(IStreamRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        return _mediator.CreateStream(request, cancellationToken);
    }

    public IAsyncEnumerable<object?> CreateStream(object request, CancellationToken cancellationToken = default)
    {
        return _mediator.CreateStream(request, cancellationToken);
    }

    private async Task ValidateAsync(object notification, CancellationToken cancellationToken)
    {
        var notificationType = notification.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(notificationType);
        var validators = _serviceProvider.GetServices(validatorType).Cast<IValidator>().ToList();

        if (!validators.Any())
        {
            return;
        }

        var context = new ValidationContext<object>(notification);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken))
        );

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
        {
            throw new ValidationException(failures);
        }
    }
}

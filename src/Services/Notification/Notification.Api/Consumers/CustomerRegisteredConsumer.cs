using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common.Messaging.Events.Customers;
using MassTransit;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Services;

namespace Notification.Api.Consumers;

/// <summary>
/// Consumidor del evento CustomerRegistered para enviar email de bienvenida.
/// </summary>
public class CustomerRegisteredConsumer : IConsumer<CustomerRegisteredEvent>
{
    private readonly IEmailNotificationService _emailService;
    private readonly ILogger<CustomerRegisteredConsumer> _logger;

    public CustomerRegisteredConsumer(
        IEmailNotificationService emailService,
        ILogger<CustomerRegisteredConsumer> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<CustomerRegisteredEvent> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Processing CustomerRegisteredEvent - CustomerId: {CustomerId}, Email: {Email}",
            message.CustomerId, message.Email);

        try
        {
            var emailData = new Dictionary<string, object>
            {
                { "CustomerName", message.FullName },
                { "Email", message.Email },
                { "RegisteredAt", message.RegisteredAt.ToString("dd/MM/yyyy HH:mm") }
            };

            await _emailService.SendTemplatedEmailAsync(
                message.Email,
                "welcome-email",
                emailData);

            _logger.LogInformation(
                "Welcome email sent to {Email} for CustomerId: {CustomerId}",
                message.Email, message.CustomerId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error sending welcome email to {Email} for CustomerId: {CustomerId}",
                message.Email, message.CustomerId);
            throw;
        }
    }
}

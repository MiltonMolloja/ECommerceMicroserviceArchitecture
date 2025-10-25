using FluentValidation;
using Notification.Service.EventHandlers.Commands;

namespace Notification.Service.EventHandlers.Validators
{
    public class SendNotificationCommandValidator : AbstractValidator<SendNotificationCommand>
    {
        public SendNotificationCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be greater than 0");

            RuleFor(x => x.Type)
                .IsInEnum()
                .WithMessage("Invalid notification type");

            RuleFor(x => x.Priority)
                .IsInEnum()
                .WithMessage("Invalid notification priority");

            RuleFor(x => x.Variables)
                .NotNull()
                .WithMessage("Variables dictionary cannot be null");

            RuleFor(x => x.Channels)
                .NotEmpty()
                .WithMessage("At least one notification channel must be specified");

            RuleFor(x => x.ExpiresAt)
                .GreaterThan(System.DateTime.UtcNow)
                .When(x => x.ExpiresAt.HasValue)
                .WithMessage("Expiration date must be in the future");
        }
    }
}

using FluentValidation;
using Notification.Service.EventHandlers.Commands;

namespace Notification.Service.EventHandlers.Validators
{
    public class MarkAsReadCommandValidator : AbstractValidator<MarkAsReadCommand>
    {
        public MarkAsReadCommandValidator()
        {
            RuleFor(x => x.NotificationId)
                .GreaterThan(0)
                .WithMessage("NotificationId must be greater than 0");

            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be greater than 0");
        }
    }
}

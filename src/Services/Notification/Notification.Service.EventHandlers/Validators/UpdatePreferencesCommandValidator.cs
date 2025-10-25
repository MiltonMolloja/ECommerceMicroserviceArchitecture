using FluentValidation;
using Notification.Service.EventHandlers.Commands;

namespace Notification.Service.EventHandlers.Validators
{
    public class UpdatePreferencesCommandValidator : AbstractValidator<UpdatePreferencesCommand>
    {
        public UpdatePreferencesCommandValidator()
        {
            RuleFor(x => x.UserId)
                .GreaterThan(0)
                .WithMessage("UserId must be greater than 0");
        }
    }
}

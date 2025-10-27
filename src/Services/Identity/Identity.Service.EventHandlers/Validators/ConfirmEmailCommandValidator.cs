using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class ConfirmEmailCommandValidator : AbstractValidator<ConfirmEmailCommand>
    {
        public ConfirmEmailCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Token)
                .NotEmpty().WithMessage("Token is required");
        }
    }
}

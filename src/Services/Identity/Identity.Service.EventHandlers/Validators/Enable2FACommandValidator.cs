using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class Enable2FACommandValidator : AbstractValidator<Enable2FACommand>
    {
        public Enable2FACommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");
        }
    }
}

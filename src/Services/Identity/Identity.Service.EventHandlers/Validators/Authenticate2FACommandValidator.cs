using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class Authenticate2FACommandValidator : AbstractValidator<Authenticate2FACommand>
    {
        public Authenticate2FACommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("2FA code is required");
        }
    }
}

using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class RevokeAllSessionsCommandValidator : AbstractValidator<RevokeAllSessionsCommand>
    {
        public RevokeAllSessionsCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");
        }
    }
}

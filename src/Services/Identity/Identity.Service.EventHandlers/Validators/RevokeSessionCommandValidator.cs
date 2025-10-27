using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class RevokeSessionCommandValidator : AbstractValidator<RevokeSessionCommand>
    {
        public RevokeSessionCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.SessionId)
                .GreaterThan(0).WithMessage("SessionId must be greater than 0");
        }
    }
}

using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class Verify2FACommandValidator : AbstractValidator<Verify2FACommand>
    {
        public Verify2FACommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(6).WithMessage("Code must be 6 digits");
        }
    }
}

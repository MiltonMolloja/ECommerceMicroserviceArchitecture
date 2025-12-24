using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class Verify2FACommandValidator : AbstractValidator<Verify2FACommand>
    {
        public Verify2FACommandValidator()
        {
            // Note: UserId is set by the controller from JWT token, not from the request body
            // So we don't validate it here

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Code is required")
                .Length(6).WithMessage("Code must be 6 digits");
        }
    }
}

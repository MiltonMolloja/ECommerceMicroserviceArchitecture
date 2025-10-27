using FluentValidation;
using Identity.Service.EventHandlers.Commands;

namespace Identity.Service.EventHandlers.Validators
{
    public class RegenerateBackupCodesCommandValidator : AbstractValidator<RegenerateBackupCodesCommand>
    {
        public RegenerateBackupCodesCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("UserId is required");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Password is required for security verification");

            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("2FA code is required")
                .Length(6).WithMessage("Code must be 6 digits");
        }
    }
}

using FluentValidation;
using Customer.Service.EventHandlers.Commands;

namespace Customer.Service.EventHandlers.Validators
{
    public class ClientCreateCommandValidator : AbstractValidator<ClientCreateCommand>
    {
        public ClientCreateCommandValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Client name is required")
                .MaximumLength(255).WithMessage("Client name must not exceed 255 characters")
                .MinimumLength(3).WithMessage("Client name must be at least 3 characters")
                .Matches("^[a-zA-Z\\s]+$").WithMessage("Client name can only contain letters and spaces");
        }
    }
}

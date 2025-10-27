using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class ResendEmailConfirmationCommand : IRequest<bool>
    {
        public string Email { get; set; }
    }
}

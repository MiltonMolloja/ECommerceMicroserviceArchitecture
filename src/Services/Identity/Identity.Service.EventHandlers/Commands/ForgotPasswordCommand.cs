using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class ForgotPasswordCommand : IRequest<bool>
    {
        public string Email { get; set; }
    }
}

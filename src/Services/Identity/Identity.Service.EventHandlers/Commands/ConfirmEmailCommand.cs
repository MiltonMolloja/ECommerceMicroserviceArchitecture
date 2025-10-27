using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class ConfirmEmailCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public string Token { get; set; }
    }
}

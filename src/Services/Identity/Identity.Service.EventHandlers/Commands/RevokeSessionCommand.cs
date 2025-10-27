using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class RevokeSessionCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public int SessionId { get; set; }
    }
}

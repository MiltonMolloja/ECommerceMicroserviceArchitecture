using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class RevokeAllSessionsCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public string CurrentRefreshToken { get; set; }
    }
}

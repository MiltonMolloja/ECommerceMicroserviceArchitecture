using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class RevokeTokenCommand : IRequest<bool>
    {
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
    }
}

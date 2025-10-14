using Identity.Service.EventHandlers.Responses;
using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class RefreshTokenCommand : IRequest<IdentityAccess>
    {
        public string RefreshToken { get; set; }
        public string IpAddress { get; set; }
    }
}

using Identity.Service.EventHandlers.Responses;
using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class Authenticate2FACommand : IRequest<IdentityAccess>
    {
        public string UserId { get; set; }
        public string Code { get; set; }
        public bool RememberDevice { get; set; }
    }
}

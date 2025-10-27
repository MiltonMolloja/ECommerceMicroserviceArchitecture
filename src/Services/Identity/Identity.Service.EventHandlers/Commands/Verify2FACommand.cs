using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class Verify2FACommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public string Code { get; set; }
    }
}

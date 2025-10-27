using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class Disable2FACommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
    }
}

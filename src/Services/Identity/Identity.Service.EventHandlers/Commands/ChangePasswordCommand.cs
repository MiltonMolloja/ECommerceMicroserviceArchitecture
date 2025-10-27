using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class ChangePasswordCommand : IRequest<bool>
    {
        public string UserId { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmPassword { get; set; }
    }
}

using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class Enable2FACommand : IRequest<Enable2FAResponse>
    {
        public string UserId { get; set; }
    }

    public class Enable2FAResponse
    {
        public bool Succeeded { get; set; }
        public string Secret { get; set; }
        public string QrCodeUri { get; set; }
        public string[] BackupCodes { get; set; }
    }
}

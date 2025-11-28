using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class GetBackupCodesStatusCommand : IRequest<GetBackupCodesStatusResponse>
    {
        public string UserId { get; set; }
    }

    public class GetBackupCodesStatusResponse
    {
        public bool HasBackupCodes { get; set; }
        public int TotalCodes { get; set; }
        public int UnusedCodes { get; set; }
        public int UsedCodes { get; set; }
    }
}

using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    public class RegenerateBackupCodesCommand : IRequest<RegenerateBackupCodesResponse>
    {
        public string UserId { get; set; }
        public string Password { get; set; }
        public string Code { get; set; }
    }

    public class RegenerateBackupCodesResponse
    {
        public bool Succeeded { get; set; }
        public string[] BackupCodes { get; set; }
    }
}

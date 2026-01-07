using MediatR;

namespace Identity.Service.EventHandlers.Commands
{
    /// <summary>
    /// Command to reset the test user to default state.
    /// This command should only be available in development environments.
    /// </summary>
    public class ResetTestUserCommand : IRequest<bool>
    {
        // Test user configuration
        public static readonly string TestUserId = "e6e4a397-235f-45bf-8241-95a97f9fd89d";
        public static readonly string TestUserEmail = "mfmolloja@gmail.com";
        public static readonly string TestUserPasswordHash = "AQAAAAIAAYagAAAAEJ+1XhPpylxGy2hhXHF/Jr/Z2VPUWgBSBn5ULrHZZYukkl7LOF71fjTC8jqc1H96/g==";
    }
}

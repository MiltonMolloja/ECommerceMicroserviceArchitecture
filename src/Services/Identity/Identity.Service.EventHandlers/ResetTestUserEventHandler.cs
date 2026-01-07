using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    /// <summary>
    /// Handler for resetting the test user to its default state.
    /// This is only meant to be used in development environments.
    /// </summary>
    public class ResetTestUserEventHandler : IRequestHandler<ResetTestUserCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<ResetTestUserEventHandler> _logger;

        public ResetTestUserEventHandler(
            UserManager<ApplicationUser> userManager,
            ILogger<ResetTestUserEventHandler> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        public async Task<bool> Handle(ResetTestUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Attempting to reset test user: {Email}", ResetTestUserCommand.TestUserEmail);

                // Find the test user by ID
                var user = await _userManager.FindByIdAsync(ResetTestUserCommand.TestUserId);

                if (user == null)
                {
                    // If user doesn't exist, create it
                    _logger.LogInformation("Test user not found, creating new user");
                    
                    user = new ApplicationUser
                    {
                        Id = ResetTestUserCommand.TestUserId,
                        UserName = ResetTestUserCommand.TestUserEmail,
                        NormalizedUserName = ResetTestUserCommand.TestUserEmail.ToUpperInvariant(),
                        Email = ResetTestUserCommand.TestUserEmail,
                        NormalizedEmail = ResetTestUserCommand.TestUserEmail.ToUpperInvariant(),
                        EmailConfirmed = true,
                        PasswordHash = ResetTestUserCommand.TestUserPasswordHash,
                        SecurityStamp = Guid.NewGuid().ToString(),
                        ConcurrencyStamp = Guid.NewGuid().ToString(),
                        TwoFactorEnabled = false,
                        LockoutEnabled = true,
                        AccessFailedCount = 0,
                        FirstName = "Test",
                        LastName = "User"
                    };

                    var createResult = await _userManager.CreateAsync(user);
                    
                    if (!createResult.Succeeded)
                    {
                        _logger.LogError("Failed to create test user: {Errors}", 
                            string.Join(", ", createResult.Errors));
                        return false;
                    }

                    _logger.LogInformation("Test user created successfully");
                    return true;
                }

                // Reset the existing user's properties
                user.UserName = ResetTestUserCommand.TestUserEmail;
                user.NormalizedUserName = ResetTestUserCommand.TestUserEmail.ToUpperInvariant();
                user.Email = ResetTestUserCommand.TestUserEmail;
                user.NormalizedEmail = ResetTestUserCommand.TestUserEmail.ToUpperInvariant();
                user.EmailConfirmed = true;
                user.PasswordHash = ResetTestUserCommand.TestUserPasswordHash;
                user.SecurityStamp = Guid.NewGuid().ToString();
                user.TwoFactorEnabled = false;
                user.LockoutEnabled = true;
                user.LockoutEnd = null;
                user.AccessFailedCount = 0;

                var updateResult = await _userManager.UpdateAsync(user);

                if (!updateResult.Succeeded)
                {
                    _logger.LogError("Failed to reset test user: {Errors}", 
                        string.Join(", ", updateResult.Errors));
                    return false;
                }

                _logger.LogInformation("Test user reset successfully: {Email}", ResetTestUserCommand.TestUserEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting test user");
                return false;
            }
        }
    }
}

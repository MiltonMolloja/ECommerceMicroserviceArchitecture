using Identity.Domain;
using Identity.Persistence.Database;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Responses;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class Authenticate2FAEventHandler : IRequestHandler<Authenticate2FACommand, IdentityAccess>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _context;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<Authenticate2FAEventHandler> _logger;

        public Authenticate2FAEventHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ITwoFactorService twoFactorService,
            IConfiguration configuration,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<Authenticate2FAEventHandler> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
            _twoFactorService = twoFactorService;
            _configuration = configuration;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IdentityAccess> Handle(Authenticate2FACommand request, CancellationToken cancellationToken)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(request.UserId);
                if (user == null)
                {
                    _logger.LogWarning($"2FA authentication attempt for non-existent user: {request.UserId}");
                    return new IdentityAccess { Succeeded = false };
                }

                var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
                var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

                // Verify 2FA code
                var codeValid = await _userManager.VerifyTwoFactorTokenAsync(
                    user,
                    _userManager.Options.Tokens.AuthenticatorTokenProvider,
                    request.Code);

                // If authenticator code fails, try backup code
                if (!codeValid)
                {
                    codeValid = await _twoFactorService.ValidateBackupCodeAsync(user.Id, request.Code);
                }

                if (!codeValid)
                {
                    _logger.LogWarning($"Invalid 2FA code for user {user.Email}");

                    await _auditService.LogActionAsync(
                        user.Id,
                        "2FAAuthentication",
                        false,
                        ipAddress,
                        userAgent,
                        "Invalid 2FA code");

                    return new IdentityAccess { Succeeded = false };
                }

                // Generate JWT
                var roles = await _userManager.GetRolesAsync(user);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Name, user.FirstName),
                    new Claim(ClaimTypes.Surname, user.LastName),
                    new Claim("EmailConfirmed", user.EmailConfirmed.ToString().ToLower()),
                    new Claim("PasswordChangedAt", user.PasswordChangedAt?.ToString("o") ?? string.Empty)
                };

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("SecretKey"));
                var expirationMinutes = 30;
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    SigningCredentials = new SigningCredentials(
                        new SymmetricSecurityKey(secretKey),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var createdToken = tokenHandler.CreateToken(tokenDescriptor);
                var accessToken = tokenHandler.WriteToken(createdToken);

                // Generate refresh token
                var refreshToken = new RefreshToken
                {
                    Token = Guid.NewGuid().ToString(),
                    UserId = user.Id,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedByIp = ipAddress,
                    UserAgent = userAgent
                };

                _context.RefreshTokens.Add(refreshToken);
                await _context.SaveChangesAsync(cancellationToken);

                // Audit log
                await _auditService.LogActionAsync(
                    user.Id,
                    "2FAAuthentication",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"2FA authentication successful for {user.Email}");

                return new IdentityAccess
                {
                    Succeeded = true,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken.Token,
                    ExpiresAt = tokenDescriptor.Expires.Value
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during 2FA authentication for user {request.UserId}");
                throw;
            }
        }
    }
}

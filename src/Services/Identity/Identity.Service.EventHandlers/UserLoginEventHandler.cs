using Identity.Domain;
using Identity.Persistence.Database;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.EventHandlers.Responses;
using Identity.Service.EventHandlers.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    public class UserLoginEventHandler :
        IRequestHandler<UserLoginCommand, IdentityAccess>
    {
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<UserLoginEventHandler> _logger;

        public UserLoginEventHandler(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserLoginEventHandler> logger)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;
        }

        public async Task<IdentityAccess> Handle(UserLoginCommand notification, CancellationToken cancellationToken)
        {
            var result = new IdentityAccess();

            var user = await _context.Users.SingleOrDefaultAsync(x => x.Email == notification.Email, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning($"Login attempt for non-existent user: {notification.Email}");
                return result;
            }

            var response = await _signInManager.CheckPasswordSignInAsync(user, notification.Password, false);

            var ipAddress = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? notification.IpAddress ?? "Unknown";
            var userAgent = _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString();

            if (response.Succeeded)
            {
                // Check if 2FA is enabled
                if (user.TwoFactorEnabled)
                {
                    _logger.LogInformation($"Login successful for {user.Email}, 2FA required");

                    await _auditService.LogActionAsync(
                        user.Id,
                        "Login",
                        true,
                        ipAddress,
                        userAgent,
                        "2FA required");

                    result.Succeeded = false;
                    result.Requires2FA = true;
                    result.UserId = user.Id;

                    return result;
                }

                result.Succeeded = true;
                await GenerateToken(user, result);

                // Generar refresh token
                var refreshToken = await _refreshTokenService.GenerateRefreshTokenAsync(
                    user.Id,
                    ipAddress
                );

                result.RefreshToken = refreshToken.Token;
                result.ExpiresAt = refreshToken.ExpiresAt;

                await _auditService.LogActionAsync(
                    user.Id,
                    "Login",
                    true,
                    ipAddress,
                    userAgent);

                _logger.LogInformation($"Login successful for {user.Email}");
            }
            else
            {
                await _auditService.LogActionAsync(
                    user.Id,
                    "Login",
                    false,
                    ipAddress,
                    userAgent,
                    "Invalid credentials");

                _logger.LogWarning($"Failed login attempt for {user.Email}");
            }

            return result;
        }

        private async Task GenerateToken(ApplicationUser user, IdentityAccess identity)
        {
            var secretKey = _configuration.GetValue<string>("SecretKey");
            var key = Encoding.ASCII.GetBytes(secretKey);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName)
            };

            var roles = await _context.Roles
                                      .Where(x => x.UserRoles.Any(y => y.UserId == user.Id))
                                      .ToListAsync();

            foreach (var role in roles)
            {
                claims.Add(
                    new Claim(ClaimTypes.Role, role.Name)
                );
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var createdToken = tokenHandler.CreateToken(tokenDescriptor);

            identity.AccessToken = tokenHandler.WriteToken(createdToken);
        }
    }
}

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
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Identity.Service.EventHandlers
{
    public class Authenticate2FAEventHandler : IRequestHandler<Authenticate2FACommand, IdentityAccess>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ITwoFactorService _twoFactorService;
        private readonly IConfiguration _configuration;
        private readonly IAuditService _auditService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly INotificationClient _notificationClient;
        private readonly ILogger<Authenticate2FAEventHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public Authenticate2FAEventHandler(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext context,
            ITwoFactorService twoFactorService,
            IConfiguration configuration,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            INotificationClient notificationClient,
            ILogger<Authenticate2FAEventHandler> logger,
            IHttpClientFactory httpClientFactory)
        {
            _ = signInManager; // 2FA verification uses UserManager directly
            _userManager = userManager;
            _context = context;
            _twoFactorService = twoFactorService;
            _configuration = configuration;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _notificationClient = notificationClient;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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
                    new Claim("PasswordChangedAt", user.PasswordChangedAt?.ToString("o") ?? string.Empty),
                    new Claim("TwoFactorEnabled", user.TwoFactorEnabled.ToString().ToLower())
                };

                // Obtener ClientId desde Customer service
                var clientId = await GetClientIdByUserIdAsync(user.Id);
                if (clientId.HasValue)
                {
                    claims.Add(new Claim("ClientId", clientId.Value.ToString()));
                }

                foreach (var role in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }

                var secretKey = Encoding.ASCII.GetBytes(_configuration.GetValue<string>("Jwt:SecretKey"));
                var expirationSeconds = 30; // TODO: Cambiar a minutos en producción
                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddSeconds(expirationSeconds),
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

                // Send new session alert email
                await SendNewSessionAlertAsync(user, ipAddress, userAgent);

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

        private async Task SendNewSessionAlertAsync(ApplicationUser user, string ipAddress, string userAgent)
        {
            try
            {
                var now = DateTime.UtcNow;

                var device = ParseDevice(userAgent);
                var browser = ParseBrowser(userAgent);
                var location = GetLocationFromIp(ipAddress);
                var formattedIp = FormatIpAddress(ipAddress);

                await _notificationClient.SendEmailAsync(
                    user.Email,
                    "new-session-alert",
                    new
                    {
                        FirstName = user.FirstName,
                        Date = now.ToString("dd/MM/yyyy"),
                        Time = now.ToString("HH:mm:ss"),
                        Device = device,
                        Browser = browser,
                        Location = location,
                        IpAddress = formattedIp,
                        ConfirmLink = $"{_configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4400"}/profile/sessions",
                        SecureLink = $"{_configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4400"}/profile/security"
                    });

                _logger.LogInformation($"New session alert email sent to {user.Email} after 2FA");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending new session alert email to {user.Email}");
                // Don't throw - email failure shouldn't block authentication
            }
        }

        private string FormatIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown";

            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                return "Localhost (Development)";

            return ipAddress;
        }

        private string GetLocationFromIp(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown";

            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                return "Local Machine (Development)";

            return "Unknown";
        }

        private string ParseDevice(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "API Client / Testing Tool";

            var ua = userAgent.ToLower();

            if (ua.Contains("iphone"))
                return "iPhone";
            if (ua.Contains("ipad"))
                return "iPad";
            if (ua.Contains("android") && ua.Contains("mobile"))
                return "Android Phone";
            if (ua.Contains("android"))
                return "Android Tablet";
            if (ua.Contains("windows phone"))
                return "Windows Phone";

            if (ua.Contains("mac os x") || ua.Contains("macintosh"))
                return "Mac";
            if (ua.Contains("windows nt"))
                return "Windows PC";
            if (ua.Contains("linux") && !ua.Contains("android"))
                return "Linux PC";

            if (ua.Contains("postman"))
                return "Postman (API Testing)";
            if (ua.Contains("insomnia"))
                return "Insomnia (API Testing)";
            if (ua.Contains("swagger"))
                return "Swagger UI (API Testing)";
            if (ua.Contains("curl"))
                return "cURL (Command Line)";

            return "Unknown Device";
        }

        private string ParseBrowser(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "API Client / Testing Tool";

            var ua = userAgent.ToLower();

            if (ua.Contains("postman"))
                return "Postman";
            if (ua.Contains("insomnia"))
                return "Insomnia";
            if (ua.Contains("swagger"))
                return "Swagger UI";
            if (ua.Contains("curl"))
                return "cURL";
            if (ua.Contains("python-requests"))
                return "Python Requests";
            if (ua.Contains("java"))
                return "Java HTTP Client";

            if (ua.Contains("edg/") || ua.Contains("edge/"))
                return "Microsoft Edge";
            if (ua.Contains("chrome/") && !ua.Contains("edg"))
                return "Google Chrome";
            if (ua.Contains("firefox/"))
                return "Mozilla Firefox";
            if (ua.Contains("safari/") && !ua.Contains("chrome") && !ua.Contains("chromium"))
                return "Safari";
            if (ua.Contains("opera/") || ua.Contains("opr/"))
                return "Opera";
            if (ua.Contains("msie") || ua.Contains("trident"))
                return "Internet Explorer";

            return "Unknown Browser";
        }

        private async Task<int?> GetClientIdByUserIdAsync(string userId)
        {
            try
            {
                var customerUrl = _configuration.GetValue<string>("ApiUrls:CustomerUrl");
                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Add("X-API-Key", "identity-api-key-2025-secure-ecommerce-service-communication");

                var response = await httpClient.GetAsync($"{customerUrl}/v1/clients/by-user/{userId}");

                if (response.IsSuccessStatusCode)
                {
                    var clientData = await response.Content.ReadFromJsonAsync<ClientResponse>();
                    return clientData?.ClientId;
                }
                else
                {
                    _logger.LogWarning($"Failed to get ClientId for UserId {userId}. Status: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting ClientId for UserId {userId}");
                return null;
            }
        }

        private class ClientResponse
        {
            public int ClientId { get; set; }
        }
    }
}

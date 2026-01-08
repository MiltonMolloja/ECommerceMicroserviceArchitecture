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
using System.Net.Http;
using System.Net.Http.Json;
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
        private readonly INotificationClient _notificationClient;
        private readonly ILogger<UserLoginEventHandler> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public UserLoginEventHandler(
            SignInManager<ApplicationUser> signInManager,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IConfiguration configuration,
            IRefreshTokenService refreshTokenService,
            IAuditService auditService,
            IHttpContextAccessor httpContextAccessor,
            INotificationClient notificationClient,
            ILogger<UserLoginEventHandler> logger,
            IHttpClientFactory httpClientFactory)
        {
            _signInManager = signInManager;
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _refreshTokenService = refreshTokenService;
            _auditService = auditService;
            _httpContextAccessor = httpContextAccessor;
            _notificationClient = notificationClient;
            _logger = logger;
            _httpClientFactory = httpClientFactory;
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
                    ipAddress,
                    userAgent
                );

                result.RefreshToken = refreshToken.Token;
                result.ExpiresAt = refreshToken.ExpiresAt;

                await _auditService.LogActionAsync(
                    user.Id,
                    "Login",
                    true,
                    ipAddress,
                    userAgent);

                // Send new session alert email
                await SendNewSessionAlertAsync(user, ipAddress, userAgent);

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
            var secretKey = _configuration.GetValue<string>("Jwt:SecretKey");
            var key = Encoding.ASCII.GetBytes(secretKey);

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


        private async Task SendNewSessionAlertAsync(ApplicationUser user, string ipAddress, string userAgent)
        {
            try
            {
                var now = DateTime.UtcNow;

                // Log UserAgent for debugging
                _logger.LogInformation($"Processing login alert - IP: {ipAddress}, UserAgent: {userAgent ?? "null"}");

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

                _logger.LogInformation($"New session alert email sent to {user.Email}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending new session alert email to {user.Email}");
                // Don't throw - email failure shouldn't block login
            }
        }

        private string FormatIpAddress(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown";

            // Handle localhost
            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                return "Localhost (Development)";

            return ipAddress;
        }

        private string GetLocationFromIp(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
                return "Unknown";

            // Handle localhost
            if (ipAddress == "::1" || ipAddress == "127.0.0.1")
                return "Local Machine (Development)";

            // TODO: Implement IP geolocation service (e.g., MaxMind, ipapi.co)
            return "Unknown";
        }

        private string ParseDevice(string userAgent)
        {
            if (string.IsNullOrEmpty(userAgent))
                return "API Client / Testing Tool";

            var ua = userAgent.ToLower();

            // Mobile devices
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

            // Desktop OS
            if (ua.Contains("mac os x") || ua.Contains("macintosh"))
                return "Mac";
            if (ua.Contains("windows nt"))
                return "Windows PC";
            if (ua.Contains("linux") && !ua.Contains("android"))
                return "Linux PC";

            // API Clients / Testing tools
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

            // API Clients / Testing tools (check first)
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

            // Real browsers (order matters!)
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
    }
}

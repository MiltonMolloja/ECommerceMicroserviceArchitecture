using Common.Caching;
using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using Identity.Service.Queries;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Identity.Api.Controllers
{
    [ApiController]
    [Route("v1/identity")]
    public class IdentityController : ControllerBase
    {
        private readonly ILogger<IdentityController> _logger;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly ISessionQueryService _sessionQueryService;
        private readonly IAccountActivityQueryService _accountActivityQueryService;
        private readonly IConfiguration _configuration;

        public IdentityController(
            ILogger<IdentityController> logger,
            SignInManager<ApplicationUser> signInManager,
            IMediator mediator,
            ICacheService cacheService,
            ISessionQueryService sessionQueryService,
            IAccountActivityQueryService accountActivityQueryService,
            IOptions<CacheSettings> cacheSettings,
            IConfiguration configuration)
        {
            _logger = logger;
            _signInManager = signInManager;
            _mediator = mediator;
            _cacheService = cacheService;
            _sessionQueryService = sessionQueryService;
            _accountActivityQueryService = accountActivityQueryService;
            _configuration = configuration;
        }

        #region User Registration

        [HttpPost]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Create(UserCreateCommand command)
        {
            if (ModelState.IsValid)
            {
                var result = await _mediator.Send(command);

                if (!result.Succeeded)
                {
                    return BadRequest(result.Errors);
                }

                // Invalidar caché de usuarios cuando se crea uno nuevo
                var pageSizes = new[] { 10, 20, 50, 100 };
                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        var cacheKeyToRemove = $"users:all:page:{page}:take:{pageSize}:ids:all";
                        await _cacheService.RemoveAsync(cacheKeyToRemove);
                    }
                }

                _logger.LogInformation($"User created successfully and user cache invalidated");
                return Ok(new { message = "User created successfully", success = true });
            }

            return BadRequest();
        }

        #endregion

        #region Authentication

        [HttpPost("authentication")]
        [EnableRateLimiting("authentication")]
        public async Task<IActionResult> Authentication(UserLoginCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Obtener IP del cliente
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // Rate limiting: verificar intentos de login fallidos
            var loginAttemptsKey = $"login:attempts:{command.Email}";
            var blockKey = $"login:blocked:{command.Email}";

            // Verificar si el usuario está bloqueado
            var isBlocked = await _cacheService.GetAsync<bool?>(blockKey);
            if (isBlocked == true)
            {
                _logger.LogWarning($"Login attempt blocked for user: {command.Email}");
                return StatusCode(429, new { message = "Too many failed attempts. Please try again later." });
            }

            // Intentar autenticar
            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                // Check if 2FA is required
                if (result.Requires2FA)
                {
                    return Ok(new
                    {
                        succeeded = false,
                        requires2FA = true,
                        userId = result.UserId,
                        message = "2FA authentication required"
                    });
                }

                // Incrementar contador de intentos fallidos
                var attempts = await _cacheService.GetAsync<int?>(loginAttemptsKey) ?? 0;
                attempts++;

                await _cacheService.SetAsync(loginAttemptsKey, attempts, TimeSpan.FromMinutes(15));

                _logger.LogWarning($"Failed login attempt {attempts} for user: {command.Email}");

                // Bloquear después de 5 intentos fallidos
                if (attempts >= 5)
                {
                    await _cacheService.SetAsync(blockKey, true, TimeSpan.FromMinutes(15));
                    _logger.LogWarning($"User blocked due to too many failed attempts: {command.Email}");
                    return StatusCode(429, new { message = "Too many failed attempts. Account blocked for 15 minutes." });
                }

                return BadRequest(new { message = "Access denied", attemptsRemaining = 5 - attempts });
            }

            // Login exitoso: limpiar contadores
            await _cacheService.RemoveAsync(loginAttemptsKey);
            await _cacheService.RemoveAsync(blockKey);

            // Guardar sesión activa (opcional)
            var sessionKey = $"session:{command.Email}:{Guid.NewGuid()}";
            await _cacheService.SetAsync(sessionKey, new { email = command.Email, loginTime = DateTime.UtcNow }, TimeSpan.FromHours(24));

            _logger.LogInformation($"Successful login for user: {command.Email}");
            return Ok(result);
        }

        #endregion

        #region Token Management

        [HttpPost("refresh-token")]
        [EnableRateLimiting("authentication")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Obtener IP del cliente
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                _logger.LogWarning($"Failed refresh token attempt from IP: {command.IpAddress}");
                return Unauthorized(new { message = "Invalid or expired refresh token" });
            }

            _logger.LogInformation($"Successful token refresh from IP: {command.IpAddress}");
            return Ok(result);
        }

        [HttpPost("revoke-token")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> RevokeToken([FromBody] RevokeTokenCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            // Obtener IP del cliente
            command.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            var result = await _mediator.Send(command);

            if (!result)
            {
                _logger.LogWarning($"Failed revoke token attempt from IP: {command.IpAddress}");
                return BadRequest(new { message = "Failed to revoke token" });
            }

            _logger.LogInformation($"Token revoked successfully from IP: {command.IpAddress}");
            return Ok(new { message = "Token revoked successfully" });
        }

        #endregion

        #region Profile Management

        /// <summary>
        /// Update user profile (FirstName and LastName only)
        /// </summary>
        [HttpPut("profile")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateUserProfileCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get userId from JWT claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to update profile." });
            }

            return Ok(new { message = "Profile updated successfully." });
        }

        #endregion

        #region Password Management

        [HttpPost("change-password")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Get userId from JWT claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to change password. Check your current password." });
            }

            return Ok(new { message = "Password changed successfully. All sessions have been logged out." });
        }

        [HttpPost("forgot-password")]
        [EnableRateLimiting("authentication")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);

            // Always return success to not reveal user existence
            return Ok(new { message = "If the email exists, a password reset link has been sent." });
        }

        [HttpPost("reset-password")]
        [EnableRateLimiting("authentication")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to reset password. Invalid or expired token." });
            }

            return Ok(new { message = "Password reset successfully. All sessions have been logged out." });
        }

        #endregion

        #region Email Confirmation

        [HttpPost("confirm-email")]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> ConfirmEmail([FromBody] ConfirmEmailCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to confirm email. Invalid or expired token." });
            }

            return Ok(new { message = "Email confirmed successfully." });
        }

        /// <summary>
        /// Endpoint GET para confirmar email desde enlace de correo
        /// </summary>
        [HttpGet("confirm-email")]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> ConfirmEmailViaLink([FromQuery] string userId, [FromQuery] string token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                return BadRequest(new { message = "UserId and Token are required." });
            }

            _logger.LogInformation($"Email confirmation attempt - UserId: {userId}, Token length: {token.Length}");

            var command = new ConfirmEmailCommand
            {
                UserId = userId,
                Token = token
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to confirm email. Invalid or expired token." });
            }

            // Retornar una página HTML simple de confirmación exitosa
            // Use configured URLs for production, fallback to localhost for development
            var loginServiceUrl = _configuration.GetValue<string>("LoginServiceUrl") ?? "http://localhost:4400";
            var frontendUrl = _configuration.GetValue<string>("FrontendUrl") ?? "http://localhost:4200";
            var loginUrl = $"{loginServiceUrl}/auth/login?returnUrl={Uri.EscapeDataString($"{frontendUrl}/auth/callback?next=%2F")}";
            
            var htmlResponse = $@"
<!DOCTYPE html>
<html lang='es'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Email Confirmado</title>
    <style>
        body {{ font-family: Arial, sans-serif; background-color: #f5f5f5; display: flex; justify-content: center; align-items: center; height: 100vh; margin: 0; }}
        .container {{ background-color: white; padding: 40px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1); text-align: center; max-width: 500px; }}
        .icon {{ font-size: 64px; margin-bottom: 20px; }}
        h1 {{ color: #232F3E; margin-bottom: 20px; }}
        p {{ color: #555; line-height: 1.6; }}
        .button {{ display: inline-block; margin-top: 20px; padding: 12px 24px; background-color: #FF9900; color: #232F3E; text-decoration: none; border-radius: 4px; font-weight: bold; }}
        .button:hover {{ background-color: #FFA500; }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='icon'>✅</div>
        <h1>¡Email Confirmado Exitosamente!</h1>
        <p>Tu dirección de correo electrónico ha sido verificada correctamente.</p>
        <p>Ya puedes iniciar sesión en tu cuenta.</p>
        <a href='{loginUrl}' class='button'>Ir a Iniciar Sesión</a>
    </div>
</body>
</html>";

            return Content(htmlResponse, "text/html");
        }

        [HttpPost("resend-email-confirmation")]
        [EnableRateLimiting("authentication")]
        public async Task<IActionResult> ResendEmailConfirmation([FromBody] ResendEmailConfirmationCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);

            return Ok(new { message = "If the email is not confirmed, a confirmation link has been sent." });
        }

        #endregion

        #region Two-Factor Authentication

        [HttpPost("2fa/enable")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Enable2FA()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new Enable2FACommand { UserId = userId };

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to enable 2FA" });
            }

            return Ok(new
            {
                message = "2FA setup initiated. Scan the QR code with your authenticator app and verify to enable.",
                secret = result.Secret,
                qrCodeUri = result.QrCodeUri,
                backupCodes = result.BackupCodes
            });
        }

        [HttpPost("2fa/verify")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Verify2FA([FromBody] Verify2FACommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Invalid verification code" });
            }

            return Ok(new { message = "2FA enabled successfully" });
        }

        [HttpPost("2fa/authenticate")]
        [EnableRateLimiting("authentication")]
        public async Task<IActionResult> Authenticate2FA([FromBody] Authenticate2FACommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Invalid 2FA code" });
            }

            return Ok(result);
        }

        [HttpPost("2fa/disable")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> Disable2FA([FromBody] Disable2FACommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to disable 2FA. Invalid credentials or code." });
            }

            return Ok(new { message = "2FA disabled successfully" });
        }

        [HttpPost("2fa/backup-codes/regenerate")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> RegenerateBackupCodes([FromBody] RegenerateBackupCodesCommand command)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            command.UserId = userId;

            var result = await _mediator.Send(command);

            if (!result.Succeeded)
            {
                return BadRequest(new { message = "Failed to regenerate backup codes. Invalid credentials or code." });
            }

            return Ok(new
            {
                message = "Backup codes regenerated successfully. Store them securely.",
                backupCodes = result.BackupCodes
            });
        }

        #endregion

        #region Session Management

        [HttpGet("sessions")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("read")]
        public async Task<IActionResult> GetActiveSessions([FromHeader(Name = "Refresh-Token")] string refreshToken = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var sessions = await _sessionQueryService.GetActiveSessionsAsync(userId, refreshToken);

            return Ok(new { sessions });
        }

        [HttpDelete("sessions/{sessionId}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> RevokeSession(int sessionId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new RevokeSessionCommand
            {
                UserId = userId,
                SessionId = sessionId
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                return NotFound(new { message = "Session not found" });
            }

            return Ok(new { message = "Session revoked successfully" });
        }

        [HttpDelete("sessions/all")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> RevokeAllSessions([FromHeader(Name = "Refresh-Token")] string currentRefreshToken = null)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var command = new RevokeAllSessionsCommand
            {
                UserId = userId,
                CurrentRefreshToken = currentRefreshToken
            };

            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to revoke sessions" });
            }

            return Ok(new { message = "All sessions revoked successfully (except current)" });
        }

        #endregion

        #region Account Activity

        /// <summary>
        /// Get recent account activity
        /// </summary>
        [HttpGet("activity")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [EnableRateLimiting("read")]
        public async Task<IActionResult> GetAccountActivity([FromQuery] int limit = 20)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (limit > 100)
            {
                limit = 100; // Limitar a máximo 100 registros
            }

            var activities = await _accountActivityQueryService.GetRecentActivityAsync(userId, limit);

            return Ok(new { activities });
        }

        #endregion

        #region Development Tools

        /// <summary>
        /// Reset test user to default state (Development mode only)
        /// This endpoint is only available in non-production environments.
        /// </summary>
        [HttpPost("dev/reset-test-user")]
        [EnableRateLimiting("write")]
        public async Task<IActionResult> ResetTestUser()
        {
            // Check if we're in production - block the endpoint
            var environment = _configuration.GetValue<string>("ASPNETCORE_ENVIRONMENT") 
                ?? Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
            
            if (string.Equals(environment, "Production", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("Attempt to reset test user in Production environment blocked");
                return NotFound(); // Return 404 to not reveal the endpoint exists
            }

            _logger.LogInformation("Resetting test user (Development mode)");
            
            var command = new ResetTestUserCommand();
            var result = await _mediator.Send(command);

            if (!result)
            {
                return BadRequest(new { message = "Failed to reset test user" });
            }

            return Ok(new { message = "Test user reset successfully", email = ResetTestUserCommand.TestUserEmail });
        }

        #endregion
    }
}

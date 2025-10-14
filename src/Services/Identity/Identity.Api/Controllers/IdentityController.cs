using Common.Caching;
using Identity.Common;
using Identity.Domain;
using Identity.Service.EventHandlers.Commands;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
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

        public IdentityController(
            ILogger<IdentityController> logger,
            SignInManager<ApplicationUser> signInManager,
            IMediator mediator,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _signInManager = signInManager;
            _mediator = mediator;
            _cacheService = cacheService;
        }

        [HttpPost]
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

        [HttpPost("authentication")]
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

        [HttpPost("refresh-token")]
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
    }
}

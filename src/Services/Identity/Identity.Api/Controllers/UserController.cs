using Common.Caching;
using Identity.Common;
using Identity.Service.Queries;
using Identity.Service.Queries.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Common.Collection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Identity.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("v1/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserQueryService _userQueryService;
        private readonly IAuditLogQueryService _auditLogQueryService;
        private readonly ILogger<UserController> _logger;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;

        public UserController(
            ILogger<UserController> logger,
            IMediator mediator,
            IUserQueryService userQueryService,
            IAuditLogQueryService auditLogQueryService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _mediator = mediator;
            _userQueryService = userQueryService;
            _auditLogQueryService = auditLogQueryService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
        }

        [HttpGet]
        [EnableRateLimiting("read")]
        public async Task<DataCollection<UserDto>> GetAll(int page = 1, int take = 10, string ids = null)
        {
            var cacheKey = $"users:all:page:{page}:take:{take}:ids:{ids ?? "all"}";

            // Intentar obtener del caché
            var cachedUsers = await _cacheService.GetAsync<DataCollection<UserDto>>(cacheKey);
            if (cachedUsers != null)
            {
                _logger.LogInformation($"Users retrieved from cache: {cacheKey}");
                return cachedUsers;
            }

            IEnumerable<string> users = ids?.Split(',');
            var result = await _userQueryService.GetAllAsync(page, take, users);

            // Guardar en caché usando configuración de appsettings
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Users cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return result;
        }

        [HttpGet("{id}")]
        [EnableRateLimiting("read")]
        public async Task<UserDto> Get(string id)
        {
            var cacheKey = $"users:id:{id}";

            // Intentar obtener del caché
            var cachedUser = await _cacheService.GetAsync<UserDto>(cacheKey);
            if (cachedUser != null)
            {
                _logger.LogInformation($"User retrieved from cache: {cacheKey}");
                return cachedUser;
            }

            // Si no está en caché, obtener de la base de datos
            var user = await _userQueryService.GetAsync(id);

            // Guardar en caché usando configuración de appsettings
            if (user != null)
            {
                await _cacheService.SetAsync(cacheKey, user, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"User cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");
            }

            return user;
        }

        [HttpGet("{id}/audit-logs")]
        [EnableRateLimiting("read")]
        public async Task<IActionResult> GetUserAuditLogs(string id, int page = 1, int pageSize = 50)
        {
            var auditLogs = await _auditLogQueryService.GetUserAuditLogsAsync(id, page, pageSize);

            return Ok(new { auditLogs, page, pageSize, total = auditLogs.Count });
        }
    }
}

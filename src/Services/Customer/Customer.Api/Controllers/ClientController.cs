using Common.Caching;
using Common.Validation;
using Customer.Common;
using Customer.Service.EventHandlers.Commands;
using Customer.Service.Queries.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Service.Queries;
using Service.Common.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Customer.Api.Controllers
{
    [ApiController]
    [Route("v1/clients")]
    public class ClientController : ControllerBase
    {
        private readonly IClientQueryService _clientQuerService;
        private readonly ILogger<ClientController> _logger;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;

        public ClientController(
            ILogger<ClientController> logger,
            IMediator mediator,
            IClientQueryService clientQuerService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _mediator = mediator;
            _clientQuerService = clientQuerService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<DataCollection<ClientDto>> GetAll(int page = 1, int take = 10, string ids = null)
        {
            var cacheKey = $"clients:all:page:{page}:take:{take}:ids:{ids ?? "all"}";

            // Intentar obtener del caché
            var cachedClients = await _cacheService.GetAsync<DataCollection<ClientDto>>(cacheKey);
            if (cachedClients != null)
            {
                _logger.LogInformation($"Clients retrieved from cache: {cacheKey}");
                return cachedClients;
            }

            IEnumerable<int> clients = null;

            if (!string.IsNullOrEmpty(ids))
            {
                clients = ids.Split(',').Select(x => Convert.ToInt32(x));
            }

            var result = await _clientQuerService.GetAllAsync(page, take, clients);

            // Guardar en caché usando configuración de appsettings
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Clients cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return result;
        }

        [HttpGet("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ClientDto> Get(int id)
        {
            var cacheKey = $"clients:id:{id}";

            // Intentar obtener del caché
            var cachedClient = await _cacheService.GetAsync<ClientDto>(cacheKey);
            if (cachedClient != null)
            {
                _logger.LogInformation($"Client retrieved from cache: {cacheKey}");
                return cachedClient;
            }

            // Si no está en caché, obtener de la base de datos
            var client = await _clientQuerService.GetAsync(id);

            // Guardar en caché usando configuración de appsettings
            if (client != null)
            {
                await _cacheService.SetAsync(cacheKey, client, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Client cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");
            }

            return client;
        }

        [HttpPost]
        public async Task<IActionResult> Create(ClientCreateCommand notification)
        {
            try
            {
                // Crear el cliente
                await _mediator.Publish(notification);

                // Invalidar caché de listado de clientes
                var pageSizes = new[] { 10, 20, 50, 100 };

                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        var cacheKeyToRemove = $"clients:all:page:{page}:take:{pageSize}:ids:all";
                        await _cacheService.RemoveAsync(cacheKeyToRemove);
                    }
                }

                _logger.LogInformation("Client created successfully and cache invalidated");
                return Ok(new { message = "Client created successfully", success = true });
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed for client creation");
                var errors = vex.GetErrorsDictionary();
                return BadRequest(new
                {
                    type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                    title = "One or more validation errors occurred.",
                    status = 400,
                    errors = errors
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating client");
                return StatusCode(500, new { message = "Error creating client", error = ex.Message });
            }
        }
    }
}

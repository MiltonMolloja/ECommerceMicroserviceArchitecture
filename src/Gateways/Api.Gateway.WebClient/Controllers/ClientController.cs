using Api.Gateway.Models;
using Common.Caching;
using Api.Gateway.Models.Customer.DTOs;
using Api.Gateway.Proxies;
using Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("clients")]
    public class ClientController : ControllerBase
    {
        private readonly ICustomerProxy _customerProxy;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<ClientController> _logger;

        public ClientController(
            ICustomerProxy customerProxy,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILogger<ClientController> logger
        )
        {
            _customerProxy = customerProxy;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        [HttpGet]
        public async Task<DataCollection<ClientDto>> GetAll(int page, int take)
        {
            var cacheKey = $"gateway:clients:all:page:{page}:take:{take}";

            // Intentar obtener del caché
            var cachedClients = await _cacheService.GetAsync<DataCollection<ClientDto>>(cacheKey);
            if (cachedClients != null)
            {
                _logger.LogInformation($"Clients retrieved from cache: {cacheKey}");
                return cachedClients;
            }

            // Si no está en caché, llamar al servicio
            var clients = await _customerProxy.GetAllAsync(page, take);

            // Guardar en caché
            await _cacheService.SetAsync(cacheKey, clients, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Clients cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return clients;
        }

        [HttpGet("{id}")]
        public async Task<ClientDto> Get(int id)
        {
            var cacheKey = $"gateway:clients:id:{id}";

            // Intentar obtener del caché
            var cachedClient = await _cacheService.GetAsync<ClientDto>(cacheKey);
            if (cachedClient != null)
            {
                _logger.LogInformation($"Client retrieved from cache: {cacheKey}");
                return cachedClient;
            }

            // Si no está en caché, llamar al servicio
            var client = await _customerProxy.GetAsync(id);

            // Guardar en caché
            if (client != null)
            {
                await _cacheService.SetAsync(cacheKey, client, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Client cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");
            }

            return client;
        }
    }
}

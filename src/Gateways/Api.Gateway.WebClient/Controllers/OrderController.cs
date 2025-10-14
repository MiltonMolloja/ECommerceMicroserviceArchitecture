using Api.Gateway.Models;
using Api.Gateway.Models.Order.DTOs;
using Api.Gateway.Models.Orders.Commands;
using Api.Gateway.Proxies;
using Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderProxy _orderProxy;
        private readonly ICustomerProxy _customerProxy;
        private readonly ICatalogProxy _catalogProxy;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<OrderController> _logger;

        public OrderController(
            IOrderProxy orderProxy,
            ICustomerProxy customerProxy,
            ICatalogProxy catalogProxy,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILogger<OrderController> logger
        )
        {
            _orderProxy = orderProxy;
            _customerProxy = customerProxy;
            _catalogProxy = catalogProxy;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Este método no necesita traer la información de los productos porque lo usaremos para solo mostrar
        /// las cabeceras en el listado. RECUERDA: que este API Gateway alimenta a nuestro Web.Client - El backend de nuestro frontend
        /// </summary>
        /// <param name="page"></param>
        /// <param name="take"></param>
        /// <returns></returns>
        [HttpGet]
        public async Task<DataCollection<OrderDto>> GetAll(int page, int take)
        {
            var cacheKey = $"gateway:orders:all:page:{page}:take:{take}";

            // Intentar obtener del caché
            var cachedOrders = await _cacheService.GetAsync<DataCollection<OrderDto>>(cacheKey);
            if (cachedOrders != null)
            {
                _logger.LogInformation($"Orders retrieved from cache: {cacheKey}");
                return cachedOrders;
            }

            // Si no está en caché, llamar a los servicios
            var result = await _orderProxy.GetAllAsync(page, take);

            if (result.HasItems)
            {
                // Retrieve client ids
                var clientIds = result.Items
                    .Select(x => x.ClientId)
                    .GroupBy(g => g)
                    .Select(x => x.Key).ToList();

                var clients = await _customerProxy.GetAllAsync(1, clientIds.Count(), clientIds);

                foreach (var order in result.Items)
                {
                    order.Client = clients.Items.Single(x => x.ClientId == order.ClientId);
                }
            }

            // Guardar en caché
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Orders cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return result;
        }

        [HttpGet("{id}")]
        public async Task<OrderDto> Get(int id)
        {
            var cacheKey = $"gateway:orders:id:{id}";

            // Intentar obtener del caché
            var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey);
            if (cachedOrder != null)
            {
                _logger.LogInformation($"Order retrieved from cache: {cacheKey}");
                return cachedOrder;
            }

            // Si no está en caché, llamar a los servicios
            var result = await _orderProxy.GetAsync(id);

            // Retrieve client
            result.Client = await _customerProxy.GetAsync(result.ClientId);

            // Retrieve product ids
            var productIds = result.Items
                .Select(x => x.ProductId)
                .GroupBy(g => g)
                .Select(x => x.Key).ToList();

            var products = await _catalogProxy.GetAllAsync(1, productIds.Count(), productIds);

            foreach (var item in result.Items)
            {
                item.Product = products.Items.Single(x => x.ProductId == item.ProductId);
            }

            // Guardar en caché
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Order cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return result;
        }

        [HttpPost]
        public async Task<IActionResult> Create(OrderCreateCommand command)
        {
            try
            {
                await _orderProxy.CreateAsync(command);

                // Invalidar caché de listado de órdenes
                var pageSizes = new[] { 10, 20, 50, 100 };

                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        var cacheKeyToRemove = $"gateway:orders:all:page:{page}:take:{pageSize}";
                        await _cacheService.RemoveAsync(cacheKeyToRemove);
                    }
                }

                _logger.LogInformation("Order created successfully and cache invalidated");
                return Ok(new { message = "Order created successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "Error creating order", error = ex.Message });
            }
        }
    }
}

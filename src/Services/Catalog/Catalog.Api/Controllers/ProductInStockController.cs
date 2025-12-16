using Common.Caching;
using Catalog.Service.EventHandlers.Commands;
using Catalog.Service.Queries;
using Catalog.Service.Queries.DTOs;
using Common.Caching;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Service.Common.Collection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalog.Api.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("v1/stocks")]
    public class ProductInStockController : ControllerBase
    {
        private readonly IProductInStockQueryService _productInStockQueryService;
        private readonly ILogger<ProductInStockController> _logger;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;

        public ProductInStockController(
            ILogger<ProductInStockController> logger,
            IMediator mediator,
            IProductInStockQueryService productInStockQueryService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _mediator = mediator;
            _productInStockQueryService = productInStockQueryService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
        }

        [HttpGet]
        public async Task<DataCollection<ProductInStockDto>> GetAll(int page = 1, int take = 10, string products = null)
        {
            var cacheKey = $"stocks:all:page:{page}:take:{take}:products:{products ?? "all"}";

            // Intentar obtener del caché
            var cachedStocks = await _cacheService.GetAsync<DataCollection<ProductInStockDto>>(cacheKey);
            if (cachedStocks != null)
            {
                _logger.LogInformation($"Product stocks retrieved from cache: {cacheKey}");
                return cachedStocks;
            }

            IEnumerable<int> ids = null;

            if (!string.IsNullOrEmpty(products))
            {
                ids = products.Split(',').Select(x => Convert.ToInt32(x));
            }

            var result = await _productInStockQueryService.GetAllAsync(page, take, ids);

            // Guardar en caché usando configuración de appsettings
            await _cacheService.SetAsync(cacheKey, result, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Product stocks cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return result;
        }

        [HttpPut]
        public async Task<IActionResult> UpdateStock(ProductInStockUpdateStockCommand command)
        {
            try
            {
                // Actualizar el stock
                await _mediator.Publish(command);

                // Invalidar caché de stocks
                var pageSizes = new[] { 10, 20, 50, 100 };

                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        var cacheKeyToRemove = $"stocks:all:page:{page}:take:{pageSize}:products:all";
                        await _cacheService.RemoveAsync(cacheKeyToRemove);
                    }
                }

                _logger.LogInformation("Stock updated successfully and cache invalidated");
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating stock");
                return StatusCode(500, new { message = "Error updating stock", error = ex.Message });
            }
        }
    }
}

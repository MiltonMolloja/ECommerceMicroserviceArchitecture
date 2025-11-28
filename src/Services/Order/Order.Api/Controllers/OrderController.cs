using Common.Caching;
using Common.Validation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Order.Common;
using Order.Service.EventHandlers.Commands;
using Order.Service.Queries;
using Order.Service.Queries.DTOs;
using Service.Common.Collection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Order.Api.Controllers
{
    [ApiController]
    [Route("v1/orders")]
    public class OrderController : ControllerBase
    {
        private readonly IOrderQueryService _orderQueryService;
        private readonly ILogger<OrderController> _logger;
        private readonly IMediator _mediator;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;

        public OrderController(
            ILogger<OrderController> logger,
            IMediator mediator,
            IOrderQueryService orderQueryService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _logger = logger;
            _mediator = mediator;
            _orderQueryService = orderQueryService;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
        }

        [HttpGet]
        public async Task<DataCollection<OrderDto>> GetAll(int page = 1, int take = 10)
        {
            var cacheKey = $"orders:all:page:{page}:take:{take}";

            // Intentar obtener del caché
            var cachedOrders = await _cacheService.GetAsync<DataCollection<OrderDto>>(cacheKey);
            if (cachedOrders != null)
            {
                _logger.LogInformation($"Orders retrieved from cache: {cacheKey}");
                return cachedOrders;
            }

            // Si no está en caché, obtener de la base de datos
            var orders = await _orderQueryService.GetAllAsync(page, take);

            // Guardar en caché usando configuración de appsettings
            await _cacheService.SetAsync(cacheKey, orders, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
            _logger.LogInformation($"Orders cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

            return orders;
        }

        [HttpGet("{id}")]
        public async Task<OrderDto> Get(int id)
        {
            var cacheKey = $"orders:id:{id}";

            // Intentar obtener del caché
            var cachedOrder = await _cacheService.GetAsync<OrderDto>(cacheKey);
            if (cachedOrder != null)
            {
                _logger.LogInformation($"Order retrieved from cache: {cacheKey}");
                return cachedOrder;
            }

            // Si no está en caché, obtener de la base de datos
            var order = await _orderQueryService.GetAsync(id);

            // Guardar en caché usando configuración de appsettings
            if (order != null)
            {
                await _cacheService.SetAsync(cacheKey, order, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Order cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");
            }

            return order;
        }

            [HttpPost]
            public async Task<IActionResult> Create(OrderCreateCommand notification)
        {
            try
            {
                // For regular users: Extract ClientId from JWT token
                // For admin users creating orders: ClientId comes in the command (if provided)
                if (!notification.ClientId.HasValue || notification.ClientId.Value == 0)
                {
                    // Extraer ClientId del token JWT (usuario normal)
                    var clientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ClientId");
                    if (clientIdClaim == null || !int.TryParse(clientIdClaim.Value, out int clientId))
                    {
                        _logger.LogWarning("ClientId not found in JWT token and not provided in command");
                        return Unauthorized(new { message = "Client not found in authentication token" });
                    }
                    notification.ClientId = clientId;
                }
                else
                {
                    // Admin creating order for a specific client
                    // TODO: Add authorization check to ensure user has admin role
                    _logger.LogInformation($"Admin creating order for ClientId: {notification.ClientId}");
                }

                // Crear la orden
                var orderId = await _mediator.Send(notification);

                // Invalidar caché de listado de órdenes
                // Invalidar diferentes combinaciones de paginación
                var pageSizes = new[] { 10, 20, 50, 100 };

                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        var cacheKeyToRemove = $"orders:all:page:{page}:take:{pageSize}";
                        await _cacheService.RemoveAsync(cacheKeyToRemove);
                    }
                }

                _logger.LogInformation($"Order created successfully with OrderId: {orderId} and cache invalidated");
                return Ok(new { message = "Order created successfully", success = true, orderId = orderId });
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed for order creation");
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
                _logger.LogError(ex, "Error creating order");
                return StatusCode(500, new { message = "Error creating order", error = ex.Message });
            }
        }

        [HttpPut("{id}/status")]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateOrderStatusRequest request)
        {
            try
            {
                var command = new UpdateOrderStatusCommand
                {
                    OrderId = id,
                    NewStatus = request.Status,
                    Reason = request.Reason,
                    PaymentTransactionId = request.PaymentTransactionId,
                    PaymentGateway = request.PaymentGateway
                };

                await _mediator.Publish(command);

                // Invalidar caché de la orden
                await _cacheService.RemoveAsync($"orders:id:{id}");

                // Invalidar caché de listados
                var pageSizes = new[] { 10, 20, 50, 100 };
                foreach (var pageSize in pageSizes)
                {
                    for (int page = 1; page <= 20; page++)
                    {
                        await _cacheService.RemoveAsync($"orders:all:page:{page}:take:{pageSize}");
                    }
                }

                _logger.LogInformation($"Order {id} status updated to {request.Status}");
                return Ok(new { message = "Order status updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating order {id} status");
                return BadRequest(new { message = ex.Message, success = false });
            }
        }
    }

    // DTO Request
    public class UpdateOrderStatusRequest
    {
        public Enums.OrderStatus Status { get; set; }
        public string Reason { get; set; }
        public string PaymentTransactionId { get; set; }
        public string PaymentGateway { get; set; }
    }
}

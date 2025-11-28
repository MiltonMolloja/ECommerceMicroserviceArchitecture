using Api.Gateway.Models;
using Api.Gateway.Models.Payment.Commands;
using Api.Gateway.Models.Payment.DTOs;
using Api.Gateway.Proxies;
using Common.Caching;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [ApiController]
    [Route("payments")]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentProxy _paymentProxy;
        private readonly ICacheService _cacheService;
        private readonly CacheSettings _cacheSettings;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentProxy paymentProxy,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings,
            ILogger<PaymentController> logger
        )
        {
            _paymentProxy = paymentProxy;
            _cacheService = cacheService;
            _cacheSettings = cacheSettings.Value;
            _logger = logger;
        }

        /// <summary>
        /// Procesar un nuevo pago
        /// </summary>
        [HttpPost("process")]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
        {
            try
            {
                // Extraer ClientId (UserId) del token JWT
                var clientIdClaim = User.Claims.FirstOrDefault(c => c.Type == "ClientId");
                if (clientIdClaim == null)
                {
                    _logger.LogWarning("ClientId claim not found in token");
                    return Unauthorized(new { message = "Invalid token: ClientId not found" });
                }

                if (!int.TryParse(clientIdClaim.Value, out int userId))
                {
                    _logger.LogWarning("Invalid ClientId format in token: {ClientId}", clientIdClaim.Value);
                    return BadRequest(new { message = "Invalid ClientId format in token" });
                }

                // Asignar el UserId (ClientId) del token al comando (sobrescribir cualquier valor enviado)
                command.UserId = userId;

                var result = await _paymentProxy.ProcessPaymentAsync(command);

                if (result.Success)
                {
                    _logger.LogInformation("Payment processed successfully for OrderId: {OrderId}, UserId: {UserId}, PaymentId: {PaymentId}",
                        command.OrderId, userId, result.PaymentId);
                    return Ok(new
                    {
                        success = true,
                        message = result.Message,
                        paymentId = result.PaymentId,
                        transactionId = result.TransactionId
                    });
                }
                else
                {
                    _logger.LogWarning("Payment failed for OrderId: {OrderId}, UserId: {UserId}. Error: {Error}",
                        command.OrderId, userId, result.Error);
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        error = result.Error,
                        errorCode = result.ErrorCode,
                        paymentId = result.PaymentId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment for OrderId: {OrderId}", command.OrderId);
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error processing payment",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener pago por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PaymentDto>> GetPaymentById(int id)
        {
            var cacheKey = $"gateway:payments:id:{id}";

            try
            {
                // Intentar obtener del caché
                var cachedPayment = await _cacheService.GetAsync<PaymentDto>(cacheKey);
                if (cachedPayment != null)
                {
                    _logger.LogInformation($"Payment retrieved from cache: {cacheKey}");
                    return Ok(cachedPayment);
                }

                // Si no está en caché, llamar al servicio
                var payment = await _paymentProxy.GetPaymentByIdAsync(id);

                if (payment == null)
                {
                    return NotFound(new { message = "Payment not found" });
                }

                // Guardar en caché
                await _cacheService.SetAsync(cacheKey, payment, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Payment cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment {id}");
                return StatusCode(500, new { message = "Error retrieving payment", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener pago por OrderId
        /// </summary>
        [HttpGet("order/{orderId}")]
        public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(int orderId)
        {
            var cacheKey = $"gateway:payments:order:{orderId}";

            try
            {
                // Intentar obtener del caché
                var cachedPayment = await _cacheService.GetAsync<PaymentDto>(cacheKey);
                if (cachedPayment != null)
                {
                    _logger.LogInformation($"Payment retrieved from cache: {cacheKey}");
                    return Ok(cachedPayment);
                }

                // Si no está en caché, llamar al servicio
                var payment = await _paymentProxy.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                {
                    return NotFound(new { message = "Payment not found for this order" });
                }

                // Guardar en caché
                await _cacheService.SetAsync(cacheKey, payment, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Payment cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment for order {orderId}");
                return StatusCode(500, new { message = "Error retrieving payment", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener historial de pagos del usuario autenticado
        /// </summary>
        [HttpGet("history")]
        public async Task<ActionResult<List<PaymentDto>>> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            var cacheKey = $"gateway:payments:history:page:{page}:pageSize:{pageSize}";

            try
            {
                // Intentar obtener del caché
                var cachedHistory = await _cacheService.GetAsync<List<PaymentDto>>(cacheKey);
                if (cachedHistory != null)
                {
                    _logger.LogInformation($"Payment history retrieved from cache: {cacheKey}");
                    return Ok(cachedHistory);
                }

                // Si no está en caché, llamar al servicio
                var payments = await _paymentProxy.GetPaymentHistoryAsync(page, pageSize);

                // Guardar en caché
                await _cacheService.SetAsync(cacheKey, payments, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Payment history cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history");
                return StatusCode(500, new { message = "Error retrieving payment history", error = ex.Message });
            }
        }

        /// <summary>
        /// Procesar reembolso (solo Admin)
        /// </summary>
        [HttpPost("{id}/refund")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ProcessRefund(int id, [FromBody] ProcessRefundCommand command)
        {
            try
            {
                command.PaymentId = id;
                await _paymentProxy.ProcessRefundAsync(command);

                // Invalidar caché del pago
                var cacheKeyToRemove = $"gateway:payments:id:{id}";
                await _cacheService.RemoveAsync(cacheKeyToRemove);

                _logger.LogInformation("Refund processing started for PaymentId: {PaymentId}", id);
                return Ok(new { message = "Refund processing started", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for payment {id}");
                return StatusCode(500, new { message = "Error processing refund", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener transacciones de un pago
        /// </summary>
        [HttpGet("{id}/transactions")]
        public async Task<ActionResult<List<PaymentTransactionDto>>> GetPaymentTransactions(int id)
        {
            var cacheKey = $"gateway:payments:transactions:{id}";

            try
            {
                // Intentar obtener del caché
                var cachedTransactions = await _cacheService.GetAsync<List<PaymentTransactionDto>>(cacheKey);
                if (cachedTransactions != null)
                {
                    _logger.LogInformation($"Payment transactions retrieved from cache: {cacheKey}");
                    return Ok(cachedTransactions);
                }

                // Si no está en caché, llamar al servicio
                var transactions = await _paymentProxy.GetPaymentTransactionsAsync(id);

                // Guardar en caché
                await _cacheService.SetAsync(cacheKey, transactions, TimeSpan.FromMinutes(_cacheSettings.CacheExpirationMinutes));
                _logger.LogInformation($"Payment transactions cached: {cacheKey} for {_cacheSettings.CacheExpirationMinutes} minutes");

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transactions for payment {id}");
                return StatusCode(500, new { message = "Error retrieving transactions", error = ex.Message });
            }
        }
    }
}

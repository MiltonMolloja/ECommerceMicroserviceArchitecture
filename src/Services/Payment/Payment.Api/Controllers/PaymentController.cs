using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Payment.Service.EventHandlers.Commands;
using Payment.Service.Queries;
using Payment.Service.Queries.DTOs;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Payment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IPaymentQueryService _queryService;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IMediator mediator,
            IPaymentQueryService queryService,
            ILogger<PaymentController> logger)
        {
            _mediator = mediator;
            _queryService = queryService;
            _logger = logger;
        }

        /// <summary>
        /// Procesar un nuevo pago
        /// </summary>
        [HttpPost("process")]
        [Authorize]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentCommand command)
        {
            try
            {
                // Obtener UserId del token JWT
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    command.UserId = userId;
                }

                // Obtener Email del token JWT
                var emailClaim = User.FindFirst(ClaimTypes.Email) ?? User.FindFirst("email");
                if (emailClaim != null)
                {
                    command.UserEmail = emailClaim.Value;
                }

                var result = await _mediator.Send(command);

                if (result.Success)
                {
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
                    return BadRequest(new
                    {
                        success = false,
                        message = result.Message,
                        error = result.ErrorMessage,
                        errorCode = result.ErrorCode,
                        paymentId = result.PaymentId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing payment");
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Obtener pago por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> GetPaymentById(int id)
        {
            try
            {
                var payment = await _queryService.GetPaymentByIdAsync(id);

                if (payment == null)
                {
                    return NotFound();
                }

                // Verificar que el usuario sea dueño del pago
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    if (payment.UserId != userId)
                    {
                        return Forbid();
                    }
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtener pago por OrderId
        /// </summary>
        [HttpGet("order/{orderId}")]
        [Authorize]
        public async Task<ActionResult<PaymentDto>> GetPaymentByOrderId(int orderId)
        {
            try
            {
                var payment = await _queryService.GetPaymentByOrderIdAsync(orderId);

                if (payment == null)
                {
                    return NotFound();
                }

                return Ok(payment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting payment for order {orderId}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtener historial de pagos del usuario
        /// </summary>
        [HttpGet("history")]
        [Authorize]
        public async Task<ActionResult<List<PaymentDto>>> GetPaymentHistory([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var payments = await _queryService.GetUserPaymentHistoryAsync(userId, page, pageSize);

                return Ok(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payment history");
                return StatusCode(500, new { error = "Internal server error" });
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

                // Obtener UserId del admin que solicita el reembolso
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    command.RequestedBy = userId;
                }

                await _mediator.Publish(command);

                return Ok(new { message = "Refund processing started" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error processing refund for payment {id}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener transacciones de un pago
        /// </summary>
        [HttpGet("{id}/transactions")]
        [Authorize]
        public async Task<ActionResult<List<PaymentTransactionDto>>> GetPaymentTransactions(int id)
        {
            try
            {
                var transactions = await _queryService.GetPaymentTransactionsAsync(id);

                return Ok(transactions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting transactions for payment {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }
    }
}

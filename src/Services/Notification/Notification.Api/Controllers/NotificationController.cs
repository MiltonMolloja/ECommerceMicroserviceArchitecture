using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Notification.Domain;
using Notification.Service.EventHandlers.Commands;
using Notification.Service.Queries;
using Notification.Service.Queries.DTOs;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Notification.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationQueryService _queryService;
        private readonly ILogger<NotificationController> _logger;

        public NotificationController(
            IMediator mediator,
            INotificationQueryService queryService,
            ILogger<NotificationController> logger)
        {
            _mediator = mediator;
            _queryService = queryService;
            _logger = logger;
        }

        /// <summary>
        /// Enviar una notificación (usado por otros microservicios)
        /// </summary>
        [HttpPost("send")]
        [Authorize]
        public async Task<IActionResult> SendNotification([FromBody] SendNotificationCommand command)
        {
            try
            {
                await _mediator.Publish(command);

                return Ok(new { message = "Notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending notification");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener notificaciones del usuario autenticado
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<List<NotificationDto>>> GetNotifications([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var notifications = await _queryService.GetUserNotificationsAsync(userId, page, pageSize);

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting notifications");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtener notificaciones no leídas del usuario
        /// </summary>
        [HttpGet("unread")]
        [Authorize]
        public async Task<ActionResult<List<NotificationDto>>> GetUnreadNotifications()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var notifications = await _queryService.GetUnreadNotificationsAsync(userId);

                return Ok(notifications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread notifications");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtener cantidad de notificaciones no leídas
        /// </summary>
        [HttpGet("unread-count")]
        [Authorize]
        public async Task<ActionResult<int>> GetUnreadCount()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var count = await _queryService.GetUnreadCountAsync(userId);

                return Ok(count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting unread count");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Obtener notificación por ID
        /// </summary>
        [HttpGet("{id}")]
        [Authorize]
        public async Task<ActionResult<NotificationDto>> GetNotificationById(int id)
        {
            try
            {
                var notification = await _queryService.GetNotificationByIdAsync(id);

                if (notification == null)
                {
                    return NotFound();
                }

                // Verificar que el usuario sea dueño de la notificación
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
                {
                    if (notification.UserId != userId)
                    {
                        return Forbid();
                    }
                }

                return Ok(notification);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error getting notification {id}");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Marcar notificación como leída
        /// </summary>
        [HttpPut("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var command = new MarkAsReadCommand
                {
                    NotificationId = id,
                    UserId = userId
                };

                await _mediator.Publish(command);

                return Ok(new { message = "Notification marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error marking notification {id} as read");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Marcar todas las notificaciones como leídas
        /// </summary>
        [HttpPut("mark-all-read")]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                // Obtener todas las notificaciones no leídas
                var unreadNotifications = await _queryService.GetUnreadNotificationsAsync(userId);

                // Marcar cada una como leída
                foreach (var notification in unreadNotifications)
                {
                    var command = new MarkAsReadCommand
                    {
                        NotificationId = notification.NotificationId,
                        UserId = userId
                    };
                    await _mediator.Publish(command);
                }

                return Ok(new { message = $"{unreadNotifications.Count} notifications marked as read" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking all notifications as read");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Enviar notificación de confirmación de pago (usado por Payment Service)
        /// </summary>
        [HttpPost("payment-confirmation")]
        [Authorize]
        public async Task<IActionResult> SendPaymentConfirmation([FromBody] PaymentNotificationRequest request)
        {
            try
            {
                var command = new SendNotificationCommand
                {
                    UserId = request.UserId,
                    Type = NotificationType.PaymentCompleted,
                    Priority = NotificationPriority.High,
                    Variables = new Dictionary<string, object>
                    {
                        { "paymentId", request.PaymentId }
                    },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                };

                await _mediator.Publish(command);

                _logger.LogInformation($"Payment confirmation notification sent for payment {request.PaymentId}, user {request.UserId}");
                return Ok(new { message = "Payment confirmation notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending payment confirmation notification for payment {request.PaymentId}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Enviar notificación de pago fallido (usado por Payment Service)
        /// </summary>
        [HttpPost("payment-failed")]
        [Authorize]
        public async Task<IActionResult> SendPaymentFailed([FromBody] PaymentFailedNotificationRequest request)
        {
            try
            {
                var command = new SendNotificationCommand
                {
                    UserId = request.UserId,
                    Type = NotificationType.PaymentFailed,
                    Priority = NotificationPriority.High,
                    Variables = new Dictionary<string, object>
                    {
                        { "paymentId", request.PaymentId },
                        { "reason", request.Reason },
                        { "Email", request.Email },
                        { "CustomerName", request.CustomerName },
                        { "OrderNumber", request.OrderNumber },
                        { "AttemptDate", request.AttemptDate ?? DateTime.Now.ToString("dd/MM/yyyy HH:mm") },
                        { "Amount", request.Amount },
                        { "PaymentMethod", request.PaymentMethod },
                        { "FailureReason", request.FailureReason ?? request.Reason }
                    },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                };

                await _mediator.Publish(command);

                _logger.LogInformation($"Payment failed notification sent for payment {request.PaymentId}, user {request.UserId}");
                return Ok(new { message = "Payment failed notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending payment failed notification for payment {request.PaymentId}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Enviar notificación de reembolso procesado (usado por Payment Service)
        /// </summary>
        [HttpPost("refund-processed")]
        [Authorize]
        public async Task<IActionResult> SendRefundProcessed([FromBody] PaymentNotificationRequest request)
        {
            try
            {
                var command = new SendNotificationCommand
                {
                    UserId = request.UserId,
                    Type = NotificationType.PaymentRefunded,
                    Priority = NotificationPriority.High,
                    Variables = new Dictionary<string, object>
                    {
                        { "paymentId", request.PaymentId }
                    },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                };

                await _mediator.Publish(command);

                _logger.LogInformation($"Refund processed notification sent for payment {request.PaymentId}, user {request.UserId}");
                return Ok(new { message = "Refund processed notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending refund processed notification for payment {request.PaymentId}");
                return BadRequest(new { error = ex.Message });
            }
        }

        /// <summary>
        /// Enviar notificación de orden creada con email de confirmación de compra (usado por Payment Service)
        /// </summary>
        [HttpPost("order-placed")]
        [Authorize]
        public async Task<IActionResult> SendOrderPlacedNotification([FromBody] OrderPlacedNotificationRequest request)
        {
            try
            {
                var command = new SendNotificationCommand
                {
                    UserId = request.UserId,
                    Type = NotificationType.OrderPlaced,
                    Priority = NotificationPriority.High,
                    Variables = new Dictionary<string, object>
                    {
                        { "Email", request.UserEmail },
                        { "CustomerName", request.CustomerName },
                        { "OrderNumber", request.OrderNumber },
                        { "Items", request.Items },
                        { "Subtotal", request.Subtotal },
                        { "ShippingCost", request.ShippingCost },
                        { "Tax", request.Tax },
                        { "Total", request.Total },
                        { "EstimatedDelivery", request.EstimatedDelivery }
                    },
                    Channels = new List<NotificationChannel>
                    {
                        NotificationChannel.InApp,
                        NotificationChannel.Email
                    }
                };

                await _mediator.Publish(command);

                _logger.LogInformation($"Order placed notification sent for order {request.OrderNumber}, user {request.UserId}");
                return Ok(new { message = "Order placed notification sent successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending order placed notification for order {request.OrderNumber}");
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    // DTOs para las notificaciones de pago
    public class PaymentNotificationRequest
    {
        public int UserId { get; set; }
        public int PaymentId { get; set; }
    }

    public class PaymentFailedNotificationRequest
    {
        public int UserId { get; set; }
        public int PaymentId { get; set; }
        public string Reason { get; set; }
        public string Email { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public string AttemptDate { get; set; }
        public string Amount { get; set; }
        public string PaymentMethod { get; set; }
        public string FailureReason { get; set; }
    }

    public class OrderPlacedNotificationRequest
    {
        public int UserId { get; set; }
        public string UserEmail { get; set; }
        public string CustomerName { get; set; }
        public string OrderNumber { get; set; }
        public List<OrderItemDto> Items { get; set; }
        public string Subtotal { get; set; }
        public string ShippingCost { get; set; }
        public string Tax { get; set; }
        public string Total { get; set; }
        public string EstimatedDelivery { get; set; }
    }

    public class OrderItemDto
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public string UnitPrice { get; set; }
    }
}

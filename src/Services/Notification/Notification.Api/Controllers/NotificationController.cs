using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
    }
}

using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Notification.Service.EventHandlers.Commands;
using Notification.Service.Queries;
using Notification.Service.Queries.DTOs;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Notification.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly INotificationQueryService _queryService;
        private readonly ILogger<PreferencesController> _logger;

        public PreferencesController(
            IMediator mediator,
            INotificationQueryService queryService,
            ILogger<PreferencesController> logger)
        {
            _mediator = mediator;
            _queryService = queryService;
            _logger = logger;
        }

        /// <summary>
        /// Obtener preferencias de notificación del usuario
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<NotificationPreferencesDto>> GetPreferences()
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                var preferences = await _queryService.GetPreferencesAsync(userId);

                return Ok(preferences);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting preferences");
                return StatusCode(500, new { error = "Internal server error" });
            }
        }

        /// <summary>
        /// Actualizar preferencias de notificación
        /// </summary>
        [HttpPut]
        [Authorize]
        public async Task<IActionResult> UpdatePreferences([FromBody] UpdatePreferencesCommand command)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized();
                }

                command.UserId = userId;

                await _mediator.Publish(command);

                return Ok(new { message = "Preferences updated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating preferences");
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}

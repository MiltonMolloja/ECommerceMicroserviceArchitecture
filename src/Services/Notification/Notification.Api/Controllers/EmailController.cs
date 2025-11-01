using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Notification.Api.Models;
using Notification.Api.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Notification.Api.Controllers
{
    [ApiController]
    [Route("api/v1/notifications")]
    public class EmailController : ControllerBase
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;

        public EmailController(
            IEmailService emailService,
            ILogger<EmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        /// <summary>
        /// Enviar email usando template
        /// </summary>
        /// <param name="request">Datos del email: To, Template, Data</param>
        /// <returns>Ok si el email fue enviado exitosamente</returns>
        [HttpPost("email")]
        [AllowAnonymous] // Permitir llamadas desde otros microservicios sin JWT (usar API Key en producción)
        public async Task<IActionResult> SendEmail([FromBody] EmailRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.To))
                {
                    return BadRequest(new { error = "El campo 'To' es requerido" });
                }

                if (string.IsNullOrEmpty(request.Template))
                {
                    return BadRequest(new { error = "El campo 'Template' es requerido" });
                }

                _logger.LogInformation($"Sending email to {request.To} using template {request.Template}");

                var emailData = request.Data ?? new Dictionary<string, object>();

                await _emailService.SendTemplatedEmailAsync(
                    request.To,
                    request.Template,
                    emailData);

                return Ok(new
                {
                    success = true,
                    message = $"Email sent successfully to {request.To}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email to {request.To}");
                return StatusCode(500, new { error = "Internal server error sending email", details = ex.Message });
            }
        }

        /// <summary>
        /// Enviar email directo (sin template)
        /// </summary>
        [HttpPost("email/direct")]
        [AllowAnonymous]
        public async Task<IActionResult> SendDirectEmail([FromBody] EmailMessage message)
        {
            try
            {
                if (string.IsNullOrEmpty(message.To))
                {
                    return BadRequest(new { error = "El campo 'To' es requerido" });
                }

                if (string.IsNullOrEmpty(message.Subject))
                {
                    return BadRequest(new { error = "El campo 'Subject' es requerido" });
                }

                if (string.IsNullOrEmpty(message.HtmlBody) && string.IsNullOrEmpty(message.TextBody))
                {
                    return BadRequest(new { error = "Debe proporcionar HtmlBody o TextBody" });
                }

                _logger.LogInformation($"Sending direct email to {message.To}");

                await _emailService.SendEmailAsync(message);

                return Ok(new
                {
                    success = true,
                    message = $"Email sent successfully to {message.To}"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending direct email to {message.To}");
                return StatusCode(500, new { error = "Internal server error sending email", details = ex.Message });
            }
        }
    }
}

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Threading.Tasks;

namespace Payment.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Webhook de Stripe para recibir eventos de pago
        /// </summary>
        [HttpPost("stripe")]
        public async Task<IActionResult> StripeWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
                var signatureHeader = Request.Headers["Stripe-Signature"];

                _logger.LogInformation($"Stripe webhook received: {json}");

                // TODO: Implementar lógica de verificación de firma y procesamiento de eventos
                // Ver: https://stripe.com/docs/webhooks/signatures

                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return BadRequest();
            }
        }

        /// <summary>
        /// Webhook de PayPal para recibir eventos de pago
        /// </summary>
        [HttpPost("paypal")]
        public async Task<IActionResult> PayPalWebhook()
        {
            try
            {
                var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

                _logger.LogInformation($"PayPal webhook received: {json}");

                // TODO: Implementar lógica de verificación y procesamiento de eventos de PayPal

                return Ok();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error processing PayPal webhook");
                return BadRequest();
            }
        }
    }
}

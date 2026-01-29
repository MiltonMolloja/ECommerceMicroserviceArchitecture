using Cart.Service.EventHandlers.Commands;
using Cart.Service.Queries;
using Cart.Service.Queries.DTOs;
using Common.Caching;
using Common.Validation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Cart.Api.Controllers
{
    [ApiController]
    [Route("v1/cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartQueryService _cartQueryService;
        private readonly ILogger<CartController> _logger;
        private readonly IMediator _mediator;

        public CartController(
            ILogger<CartController> logger,
            IMediator mediator,
            ICartQueryService cartQueryService,
            ICacheService cacheService,
            IOptions<CacheSettings> cacheSettings)
        {
            _ = cacheService; // Reserved for future caching implementation
            _ = cacheSettings; // Reserved for future caching implementation
            _logger = logger;
            _mediator = mediator;
            _cartQueryService = cartQueryService;
        }

        [HttpGet("client/{clientId}")]
        public async Task<CartDto> GetByClientId(int clientId)
        {
            return await _cartQueryService.GetByClientIdAsync(clientId);
        }

        [HttpGet("session/{sessionId}")]
        public async Task<CartDto> GetBySessionId(string sessionId)
        {
            return await _cartQueryService.GetBySessionIdAsync(sessionId);
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem(AddItemToCartCommand command)
        {
            try
            {
                await _mediator.Publish(command);
                return Ok(new { message = "Item added to cart successfully", success = true });
            }
            catch (ValidationException vex)
            {
                _logger.LogWarning(vex, "Validation failed for add item to cart");
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
                _logger.LogError(ex, "Error adding item to cart");
                return StatusCode(500, new { message = "Error adding item to cart", error = ex.Message });
            }
        }

        [HttpPut("{cartId}/update-quantity")]
        public async Task<IActionResult> UpdateQuantity(int cartId, [FromBody] UpdateCartItemQuantityCommand command)
        {
            try
            {
                command.CartId = cartId;
                await _mediator.Publish(command);
                return Ok(new { message = "Quantity updated successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating quantity");
                return StatusCode(500, new { message = "Error updating quantity", error = ex.Message });
            }
        }

        [HttpDelete("{cartId}/remove-item/{productId}")]
        public async Task<IActionResult> RemoveItem(int cartId, int productId)
        {
            try
            {
                await _mediator.Publish(new RemoveItemFromCartCommand { CartId = cartId, ProductId = productId });
                return Ok(new { message = "Item removed successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item");
                return StatusCode(500, new { message = "Error removing item", error = ex.Message });
            }
        }

        [HttpDelete("{cartId}/clear")]
        public async Task<IActionResult> Clear(int cartId)
        {
            try
            {
                await _mediator.Publish(new ClearCartCommand { CartId = cartId });
                return Ok(new { message = "Cart cleared successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart");
                return StatusCode(500, new { message = "Error clearing cart", error = ex.Message });
            }
        }

        [HttpPost("{cartId}/apply-coupon")]
        public async Task<IActionResult> ApplyCoupon(int cartId, [FromBody] ApplyCouponCommand command)
        {
            try
            {
                command.CartId = cartId;
                await _mediator.Publish(command);
                return Ok(new { message = "Coupon applied successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying coupon");
                return StatusCode(500, new { message = "Error applying coupon", error = ex.Message });
            }
        }

        [HttpDelete("{cartId}/remove-coupon")]
        public async Task<IActionResult> RemoveCoupon(int cartId)
        {
            try
            {
                await _mediator.Publish(new RemoveCouponCommand { CartId = cartId });
                return Ok(new { message = "Coupon removed successfully", success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing coupon");
                return StatusCode(500, new { message = "Error removing coupon", error = ex.Message });
            }
        }
    }
}

using Api.Gateway.Models.Cart.DTOs;
using Api.Gateway.Proxies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Api.Gateway.WebClient.Controllers
{
    [ApiController]
    [Route("cart")]
    public class CartController : ControllerBase
    {
        private readonly ICartProxy _cartProxy;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ICartProxy cartProxy,
            ILogger<CartController> logger
        )
        {
            _cartProxy = cartProxy;
            _logger = logger;
        }

        [HttpGet("client/{clientId}")]
        public async Task<CartDto> GetByClientId(int clientId)
        {
            _logger.LogInformation("Getting cart for client {ClientId}", clientId);
            return await _cartProxy.GetByClientIdAsync(clientId);
        }

        [HttpGet("session/{sessionId}")]
        public async Task<CartDto> GetBySessionId(string sessionId)
        {
            _logger.LogInformation("Getting cart for session {SessionId}", sessionId);
            return await _cartProxy.GetBySessionIdAsync(sessionId);
        }

        [HttpPost("add-item")]
        public async Task<IActionResult> AddItem([FromBody] object command)
        {
            _logger.LogInformation("Adding item to cart");
            var result = await _cartProxy.AddItemAsync(command);
            return Ok(result);
        }

        [HttpPut("{cartId}/update-quantity")]
        public async Task<IActionResult> UpdateQuantity(int cartId, [FromBody] object command)
        {
            _logger.LogInformation("Updating quantity for cart {CartId}", cartId);
            var result = await _cartProxy.UpdateQuantityAsync(cartId, command);
            return Ok(result);
        }

        [HttpDelete("{cartId}/remove-item/{productId}")]
        public async Task<IActionResult> RemoveItem(int cartId, int productId)
        {
            _logger.LogInformation("Removing item {ProductId} from cart {CartId}", productId, cartId);
            var result = await _cartProxy.RemoveItemAsync(cartId, productId);
            return Ok(result);
        }

        [HttpDelete("{cartId}/clear")]
        public async Task<IActionResult> Clear(int cartId)
        {
            _logger.LogInformation("Clearing cart {CartId}", cartId);
            var result = await _cartProxy.ClearAsync(cartId);
            return Ok(result);
        }

        [HttpPost("{cartId}/apply-coupon")]
        public async Task<IActionResult> ApplyCoupon(int cartId, [FromBody] object command)
        {
            _logger.LogInformation("Applying coupon to cart {CartId}", cartId);
            var result = await _cartProxy.ApplyCouponAsync(cartId, command);
            return Ok(result);
        }

        [HttpDelete("{cartId}/remove-coupon")]
        public async Task<IActionResult> RemoveCoupon(int cartId)
        {
            _logger.LogInformation("Removing coupon from cart {CartId}", cartId);
            var result = await _cartProxy.RemoveCouponAsync(cartId);
            return Ok(result);
        }
    }
}

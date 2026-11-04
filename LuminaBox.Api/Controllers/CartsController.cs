using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Cart;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/carts")]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;

        public CartsController(ICartService cartService)
        {
            _cartService = cartService;
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get current user cart",
            Description = "Gets the shopping cart of the current user grouped by organizer and stall. Requires authentication.")]
        public async Task<IActionResult> GetMyCart()
        {
            var result = await _cartService.GetMyCartAsync();
            return Ok(ApiResult<CartResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("items/product")]
        [SwaggerOperation(
            Summary = "Add product item to cart",
            Description = "Adds a product variant with quantity to the current user cart. Requires authentication.")]
        public async Task<IActionResult> AddProductToCart([FromBody] AddProductToCartRequest request)
        {
            var result = await _cartService.AddProductToCartAsync(request);
            return Ok(ApiResult<CartResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("items/box")]
        [SwaggerOperation(
            Summary = "Add box item to cart",
            Description = "Adds a box combo item with quantity to the current user cart. Requires authentication.")]
        public async Task<IActionResult> AddBoxToCart([FromBody] AddBoxToCartRequest request)
        {
            var result = await _cartService.AddBoxToCartAsync(request);
            return Ok(ApiResult<CartResponse>.Success(result));
        }

        [Authorize]
        [HttpPut("items/{itemId:guid}")]
        [SwaggerOperation(
            Summary = "Update cart item",
            Description = "Updates the quantity or variant of an item in the current user cart. Requires authentication.")]
        public async Task<IActionResult> UpdateCartItem(Guid itemId, [FromBody] UpdateCartItemRequest request)
        {
            var result = await _cartService.UpdateCartItemAsync(itemId, request);
            return Ok(ApiResult<CartResponse>.Success(result));
        }

        [Authorize]
        [HttpDelete("items/{itemId:guid}")]
        [SwaggerOperation(
            Summary = "Remove item from cart",
            Description = "Removes an item from the current user cart. Requires authentication.")]
        public async Task<IActionResult> RemoveCartItem(Guid itemId)
        {
            await _cartService.RemoveCartItemAsync(itemId);
            return Ok(ApiResult.Success(message: "Cart item removed successfully."));
        }
    }
}


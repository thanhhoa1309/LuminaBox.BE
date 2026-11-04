using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Order;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        [Authorize]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create order directly",
            Description = "Creates one or more orders directly from selected items grouped by organizer and stall. Requires authentication.")]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return Ok(ApiResult<List<OrderResponse>>.Success(result));
        }

        [Authorize]
        [HttpPost("from-cart")]
        [SwaggerOperation(
            Summary = "Create order from cart items",
            Description = "Creates one or more orders from selected cart items and removes them from the cart. Requires authentication.")]
        public async Task<IActionResult> CreateOrderFromCart([FromBody] CreateOrderFromCartRequest request)
        {
            var result = await _orderService.CreateOrderFromCartAsync(request);
            return Ok(ApiResult<List<OrderResponse>>.Success(result));
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get paginated list of current user orders",
            Description = "Gets a paginated list of orders placed by the current user. Requires authentication.")]
        public async Task<IActionResult> GetMyOrders(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _orderService.GetMyOrdersAsync(
                search,
                sortBy,
                isDescending,
                page,
                pageSize,
                status,
                fromDate,
                toDate);
            return Ok(ApiResult<Pagination<OrderSummaryResponse>>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("organizer/me")]
        [SwaggerOperation(
            Summary = "Get paginated list of current organizer orders",
            Description = "Gets a paginated list of orders belonging to the current organizer. Requires Organization role.")]
        public async Task<IActionResult> GetMyOrganizerOrders(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] OrderStatus? status = null,
            [FromQuery] Guid? stallId = null,
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            var result = await _orderService.GetMyOrganizerOrdersAsync(
                search,
                sortBy,
                isDescending,
                page,
                pageSize,
                status,
                stallId,
                fromDate,
                toDate);
            return Ok(ApiResult<Pagination<OrderSummaryResponse>>.Success(result));
        }

        [Authorize]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get order details by id",
            Description = "Gets the details of an order by id. Requires authentication.")]
        public async Task<IActionResult> GetOrderByDetail(Guid id)
        {
            var result = await _orderService.GetOrderByDetail(id);
            return Ok(ApiResult<OrderResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("{id:guid}/confirm")]
        [SwaggerOperation(
            Summary = "Confirm pending order",
            Description = "Confirms a pending order and settles wallet and commission for Stripe payments. Requires Organization role.")]
        public async Task<IActionResult> ConfirmOrder(Guid id)
        {
            var result = await _orderService.ConfirmOrderAsync(id);
            return Ok(ApiResult<OrderResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("{id:guid}/reject")]
        [SwaggerOperation(
            Summary = "Reject pending order",
            Description = "Rejects a pending order and refunds payment or restores vouchers. Requires Organization role.")]
        public async Task<IActionResult> RejectOrder(Guid id, [FromBody] RejectOrderRequest request)
        {
            var result = await _orderService.RejectOrderAsync(id, request);
            return Ok(ApiResult<OrderResponse>.Success(result));
        }

        [Authorize]
        [HttpPost("{id:guid}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel order",
            Description = "Cancels an order by buyer or organizer according to status rules. Requires authentication.")]
        public async Task<IActionResult> CancelOrder(Guid id, [FromBody] CancelOrderRequest request)
        {
            var result = await _orderService.CancelOrderAsync(id, request);
            return Ok(ApiResult<OrderResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("{id:guid}/ship")]
        [SwaggerOperation(
            Summary = "Start delivering order",
            Description = "Marks a confirmed order as delivering. Requires Organization role.")]
        public async Task<IActionResult> ShipOrder(Guid id)
        {
            var result = await _orderService.ShipOrderAsync(id);
            return Ok(ApiResult<OrderResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("{id:guid}/mark-delivered")]
        [SwaggerOperation(
            Summary = "Mark order as delivered",
            Description = "Marks a delivering (or confirmed pickup) order as delivered. Requires Organization role.")]
        public async Task<IActionResult> MarkOrderDelivered(Guid id)
        {
            var result = await _orderService.MarkOrderDeliveredAsync(id);
            return Ok(ApiResult<OrderResponse>.Success(result));
        }
    }
}

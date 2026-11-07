using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Box;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/box-items")]
    public class BoxItemsController : ControllerBase
    {
        private readonly IBoxItemService _service;

        public BoxItemsController(IBoxItemService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of box items",
            Description = "Gets active boxes with stock, optionally at one stall. Public.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] Guid? stallId = null,
            [FromQuery] Guid? organizerId = null)
        {
            var result = await _service.GetAllAsync(search, page, pageSize, stallId, organizerId);
            return Ok(ApiResult<Pagination<BoxItemResponse>>.Success(result));
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get box item details by id",
            Description = "Gets an active box with images, tiers and stock per stall. Public.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get my box items",
            Description = "Gets a paginated list of the current organizer's boxes in any status. Requires Organization role.")]
        public async Task<IActionResult> GetMine(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ProductStatus? status = null)
        {
            var result = await _service.GetMineAsync(search, page, pageSize, status);
            return Ok(ApiResult<Pagination<BoxItemResponse>>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a box item",
            Description = "Creates a draft box. Requires Organization role.")]
        public async Task<IActionResult> Create([FromBody] CreateBoxItemRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a box item",
            Description = "Partially updates an own box. Requires Organization role.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBoxItemRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(
            Summary = "Change box item status",
            Description = "Shows or hides an own box. Requires Organization role.")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] UpdateProductStatusRequest request)
        {
            var result = await _service.SetStatusAsync(id, request.Status);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a box item",
            Description = "Soft deletes an own box. Requires Organization role.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("{id:guid}/images")]
        [SwaggerOperation(
            Summary = "Add a box item image",
            Description = "Adds an uploaded image url to an own box. Requires Organization role.")]
        public async Task<IActionResult> AddImage(Guid id, [FromBody] AddProductItemImageRequest request)
        {
            var result = await _service.AddImageAsync(id, request);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpDelete("{id:guid}/images/{imageId:guid}")]
        [SwaggerOperation(
            Summary = "Remove a box item image",
            Description = "Removes an image of an own box; the last image cannot be removed. Requires Organization role.")]
        public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
        {
            var result = await _service.RemoveImageAsync(id, imageId);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("{id:guid}/price-tiers")]
        [SwaggerOperation(
            Summary = "Set wholesale price tiers",
            Description = "Replaces the wholesale tiers of a box; an empty list removes them. Requires Organization role.")]
        public async Task<IActionResult> SetPriceTiers(Guid id, [FromBody] List<PriceTierRequest> tiers)
        {
            var result = await _service.SetPriceTiersAsync(id, tiers);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("{id:guid}/stocks")]
        [SwaggerOperation(
            Summary = "Set box stock per stall",
            Description = "Creates or updates the empty-box stock per stall; a null stall is the online stock. Requires Organization role.")]
        public async Task<IActionResult> SetStocks(Guid id, [FromBody] List<StockRequest> stocks)
        {
            var result = await _service.SetStocksAsync(id, stocks);
            return Ok(ApiResult<BoxItemResponse>.Success(result));
        }
    }
}

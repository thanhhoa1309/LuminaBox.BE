using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Authorize(Roles = "Organization")]
    [Route("api/product-items")]
    public class ProductItemsController : ControllerBase
    {
        private readonly IProductItemService _service;

        public ProductItemsController(IProductItemService service)
        {
            _service = service;
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a product item",
            Description = "Partially updates a variant of an own product. Requires Organization role.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductItemRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResult<ProductItemResponse>.Success(result));
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a product item",
            Description = "Soft deletes a variant of an own product. Requires Organization role.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }

        [HttpPost("{id:guid}/images")]
        [SwaggerOperation(
            Summary = "Add a product item image",
            Description = "Adds an uploaded image url to a variant of an own product. Requires Organization role.")]
        public async Task<IActionResult> AddImage(Guid id, [FromBody] AddProductItemImageRequest request)
        {
            var result = await _service.AddImageAsync(id, request);
            return Ok(ApiResult<ProductItemResponse>.Success(result));
        }

        [HttpDelete("{id:guid}/images/{imageId:guid}")]
        [SwaggerOperation(
            Summary = "Remove a product item image",
            Description = "Removes an image of a variant; the last image cannot be removed. Requires Organization role.")]
        public async Task<IActionResult> RemoveImage(Guid id, Guid imageId)
        {
            var result = await _service.RemoveImageAsync(id, imageId);
            return Ok(ApiResult<ProductItemResponse>.Success(result));
        }

        [HttpPut("{id:guid}/price-tiers")]
        [SwaggerOperation(
            Summary = "Set wholesale price tiers",
            Description = "Replaces the wholesale tiers of a variant; an empty list removes them. Requires Organization role.")]
        public async Task<IActionResult> SetPriceTiers(Guid id, [FromBody] List<PriceTierRequest> tiers)
        {
            var result = await _service.SetPriceTiersAsync(id, tiers);
            return Ok(ApiResult<ProductItemResponse>.Success(result));
        }

        [HttpPut("{id:guid}/stocks")]
        [SwaggerOperation(
            Summary = "Set stock per stall",
            Description = "Creates or updates the stock of a variant per stall; a null stall is the online stock. Requires Organization role.")]
        public async Task<IActionResult> SetStocks(Guid id, [FromBody] List<StockRequest> stocks)
        {
            var result = await _service.SetStocksAsync(id, stocks);
            return Ok(ApiResult<ProductItemResponse>.Success(result));
        }
    }
}

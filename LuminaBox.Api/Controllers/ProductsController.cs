using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/products")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly IProductItemService _productItemService;

        public ProductsController(IProductService productService, IProductItemService productItemService)
        {
            _productService = productService;
            _productItemService = productItemService;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of products",
            Description = "Gets a paginated list of active products with search, filter and sort. Public.")]
        public async Task<IActionResult> GetAll([FromQuery] ProductListQuery filter)
        {
            var result = await _productService.GetAllAsync(filter);
            return Ok(ApiResult<Pagination<ProductSummaryResponse>>.Success(result));
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get product details by id",
            Description = "Gets an active product with its items. Public.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            return Ok(ApiResult<ProductResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get my products",
            Description = "Gets a paginated list of the current organizer's products in any status. Requires Organization role.")]
        public async Task<IActionResult> GetMine([FromQuery] MyProductListQuery filter)
        {
            var result = await _productService.GetMineAsync(filter);
            return Ok(ApiResult<Pagination<ProductResponse>>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a product",
            Description = "Creates a draft product. Requires Organization role.")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
        {
            var result = await _productService.CreateAsync(request);
            return Ok(ApiResult<ProductResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a product",
            Description = "Partially updates the name and description of an own product. Requires Organization role.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
        {
            var result = await _productService.UpdateAsync(id, request);
            return Ok(ApiResult<ProductResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(
            Summary = "Change product status",
            Description = "Shows or hides an own product. Activating needs at least one item. Requires Organization role.")]
        public async Task<IActionResult> SetStatus(Guid id, [FromBody] UpdateProductStatusRequest request)
        {
            var result = await _productService.SetStatusAsync(id, request);
            return Ok(ApiResult<ProductResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a product",
            Description = "Soft deletes an own product together with its items. Requires Organization role.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("{id:guid}/items")]
        [SwaggerOperation(
            Summary = "Add a product item",
            Description = "Adds a variant with at least one image to an own product. Requires Organization role.")]
        public async Task<IActionResult> AddItem(Guid id, [FromBody] CreateProductItemRequest request)
        {
            var result = await _productItemService.CreateAsync(id, request);
            return Ok(ApiResult<ProductItemResponse>.Success(result));
        }
    }
}

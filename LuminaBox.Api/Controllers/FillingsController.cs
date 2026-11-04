using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Catalog;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/fillings")]
    public class FillingsController : ControllerBase
    {
        private readonly IFillingService _service;

        public FillingsController(IFillingService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of fillings",
            Description = "Gets a paginated list of fillings with search and sort. Public.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _service.GetAllAsync(search, sortBy, isDescending, page, pageSize);
            return Ok(ApiResult<Pagination<LookupResponse>>.Success(result));
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get filling by id",
            Description = "Gets a filling by id. Public.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResult<LookupResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a filling",
            Description = "Creates a filling. Requires Manager role.")]
        public async Task<IActionResult> Create([FromBody] LookupRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(ApiResult<LookupResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a filling",
            Description = "Updates a filling. Requires Manager role.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] LookupRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResult<LookupResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a filling",
            Description = "Soft deletes a filling that no product item uses. Requires Manager role.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }
    }
}

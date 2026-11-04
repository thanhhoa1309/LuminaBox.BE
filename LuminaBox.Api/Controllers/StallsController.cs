using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Stall;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/stalls")]
    public class StallsController : ControllerBase
    {
        private readonly IStallService _service;

        public StallsController(IStallService service)
        {
            _service = service;
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of stalls",
            Description = "Gets a paginated list of stalls with search and status, province, district and ward filters. Public.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] StallStatus? status = null,
            [FromQuery] Guid? provinceId = null,
            [FromQuery] Guid? districtId = null,
            [FromQuery] Guid? wardId = null)
        {
            var result = await _service.GetAllAsync(search, page, pageSize, status, provinceId, districtId, wardId);
            return Ok(ApiResult<Pagination<StallResponse>>.Success(result));
        }

        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get stall details by id",
            Description = "Gets a stall with its full address. Public.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResult<StallResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get my rented stalls",
            Description = "Gets the stalls with an active rental of the current organizer. Requires Organization role.")]
        public async Task<IActionResult> GetMine()
        {
            var result = await _service.GetMineAsync();
            return Ok(ApiResult<List<StallResponse>>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Create a stall",
            Description = "Creates a stall; district and province are derived from the ward. Requires Manager role.")]
        public async Task<IActionResult> Create([FromBody] CreateStallRequest request)
        {
            var result = await _service.CreateAsync(request);
            return Ok(ApiResult<StallResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update a stall",
            Description = "Partially updates a stall; status cannot be changed here. Requires Manager role.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateStallRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return Ok(ApiResult<StallResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete a stall",
            Description = "Soft deletes an available stall without rentals. Requires Manager role.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }
    }
}

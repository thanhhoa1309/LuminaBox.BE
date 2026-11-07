using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.StallRental;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StallRentalsController : ControllerBase
    {
        private readonly IStallRentalService _service;

        public StallRentalsController(IStallRentalService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [SwaggerOperation(Summary = "List stall rental contracts", Description = "Manager only. Search and filter rental history across all stalls.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RentalStatus? status = null,
            [FromQuery] Guid? stallId = null,
            [FromQuery] Guid? organizerId = null)
        {
            var result = await _service.GetAllAsync(search, sortBy, isDescending, page, pageSize, status, stallId, organizerId);
            return Ok(ApiResult<Pagination<StallRentalResponse>>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("me")]
        [SwaggerOperation(Summary = "Get my stall rental contracts", Description = "Organization only. Shows contracts belonging to the current organizer.")]
        public async Task<IActionResult> GetMine(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RentalStatus? status = null)
        {
            var result = await _service.GetMineAsync(search, sortBy, isDescending, page, pageSize, status);
            return Ok(ApiResult<Pagination<StallRentalResponse>>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(Summary = "Get a stall rental contract", Description = "Manager only.")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(ApiResult<StallRentalResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id:guid}/terminate")]
        [SwaggerOperation(Summary = "Terminate an active stall rental", Description = "Manager only. Releases the stall and deactivates stock for this organizer at this stall. No body.")]
        public async Task<IActionResult> Terminate(Guid id)
        {
            var result = await _service.TerminateAsync(id);
            return Ok(ApiResult<StallRentalResponse>.Success(result));
        }
    }
}

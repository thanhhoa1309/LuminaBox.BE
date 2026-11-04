using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Address;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/addresses")]
    public class AddressesController : ControllerBase
    {
        private readonly IAddressService _addressService;

        public AddressesController(IAddressService addressService)
        {
            _addressService = addressService;
        }

        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get my addresses",
            Description = "Gets the addresses of the current user, default first, optionally filtered by type. Requires authentication.")]
        public async Task<IActionResult> GetMine([FromQuery] AddressType? type = null)
        {
            var result = await _addressService.GetMineAsync(type);
            return Ok(ApiResult<List<AddressResponse>>.Success(result));
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Create an address",
            Description = "Creates an address for the current user. The client sends only the ward; the district and province are derived. Pickup addresses require Organization role.")]
        public async Task<IActionResult> Create([FromBody] CreateAddressRequest request)
        {
            var result = await _addressService.CreateAsync(request);
            return Ok(ApiResult<AddressResponse>.Success(result));
        }

        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update an address",
            Description = "Partially updates an address of the current user. Requires authentication.")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAddressRequest request)
        {
            var result = await _addressService.UpdateAsync(id, request);
            return Ok(ApiResult<AddressResponse>.Success(result));
        }

        [HttpPut("{id:guid}/default")]
        [SwaggerOperation(
            Summary = "Set an address as default",
            Description = "Sets an address of the current user as the default of its type. Requires authentication.")]
        public async Task<IActionResult> SetDefault(Guid id)
        {
            var result = await _addressService.SetDefaultAsync(id);
            return Ok(ApiResult<AddressResponse>.Success(result));
        }

        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete an address",
            Description = "Soft deletes an address of the current user. Requires authentication.")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _addressService.DeleteAsync(id);
            return Ok(ApiResult.Success());
        }
    }
}

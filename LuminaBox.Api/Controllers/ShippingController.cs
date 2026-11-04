using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Shipping;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/shipping")]
    public class ShippingController : ControllerBase
    {
        private readonly IShippingFeeService _service;

        public ShippingController(IShippingFeeService service)
        {
            _service = service;
        }

        [HttpPost("fee-estimate")]
        [SwaggerOperation(
            Summary = "Estimate the shipping fee of an order",
            Description = "Calculates the GHN fee from the stall or the organizer's default pickup address to an own shipping address. Requires authentication.")]
        public async Task<IActionResult> Estimate([FromBody] ShippingFeeRequest request)
        {
            var result = await _service.EstimateAsync(request);
            return Ok(ApiResult<ShippingFeeResponse>.Success(result));
        }
    }
}

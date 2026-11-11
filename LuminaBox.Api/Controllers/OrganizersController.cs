using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Dtos.Organizer;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/organizers")]
    public class OrganizersController : ControllerBase
    {
        private readonly IOrganizerProfileService _service;

        public OrganizersController(IOrganizerProfileService service)
        {
            _service = service;
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get my organizer profile",
            Description = "Brand name, contact details, business info and status of the current organizer. Suspended organizers can still read it. Requires Organization role.")]
        public async Task<IActionResult> GetMine()
        {
            var result = await _service.GetMineAsync();
            return Ok(ApiResult<OrganizerProfileResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPut("me")]
        [SwaggerOperation(
            Summary = "Update my organizer profile",
            Description = "Partial update: fields left null are kept, an empty string clears an optional field. Requires Organization role.")]
        public async Task<IActionResult> UpdateMine([FromBody] UpdateOrganizerProfileRequest request)
        {
            var result = await _service.UpdateMineAsync(request);
            return Ok(ApiResult<OrganizerProfileResponse>.Success(result));
        }
    }
}

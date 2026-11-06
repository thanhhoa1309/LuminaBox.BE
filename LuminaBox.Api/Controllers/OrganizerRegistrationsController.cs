using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.OrganizerRegistration;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/organizer-registrations")]
    public class OrganizerRegistrationsController : ControllerBase
    {
        private readonly IOrganizerRegistrationService _organizerRegistrationService;

        public OrganizerRegistrationsController(IOrganizerRegistrationService organizerRegistrationService)
        {
            _organizerRegistrationService = organizerRegistrationService;
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of organizer registrations",
            Description = "Gets a paginated list of organizer registrations with search, filter and sort. Requires Manager role.")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RegistrationType? type = null,
            [FromQuery] RegistrationStatus? status = null,
            [FromQuery] Guid? stallId = null)
        {
            var result = await _organizerRegistrationService.GetAllAsync(search, sortBy, isDescending, page, pageSize, type, status, stallId);
            return Ok(ApiResult<Pagination<OrganizerRegistrationResponse>>.Success(result));
        }

        [Authorize]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get my organizer registrations",
            Description = "Gets a paginated list of the current user's organizer registrations. Requires authentication.")]
        public async Task<IActionResult> GetMyRegistrations(
            [FromQuery] string? search,
            [FromQuery] string? sortBy,
            [FromQuery] bool isDescending = true,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RegistrationType? type = null,
            [FromQuery] RegistrationStatus? status = null)
        {
            var result = await _organizerRegistrationService.GetMyRegistrationsAsync(search, sortBy, isDescending, page, pageSize, type, status);
            return Ok(ApiResult<Pagination<OrganizerRegistrationResponse>>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get organizer registration details by id",
            Description = "Gets the details of an organizer registration by id. Requires Manager role.")]
        public async Task<IActionResult> GetOrganizerRegistrationByDetail(Guid id)
        {
            var result = await _organizerRegistrationService.GetOrganizerRegistrationByDetail(id);
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }

        [Authorize(Roles = "User")]
        [HttpPost]
        [SwaggerOperation(
            Summary = "Submit a first organizer registration",
            Description = "Submits a first stall or online organizer registration. Requires User role.")]
        public async Task<IActionResult> CreateOrganizerRegistration([FromBody] CreateOrganizerRegistrationRequest request)
        {
            var result = await _organizerRegistrationService.CreateOrganizerRegistrationAsync(request);
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("additional-stalls")]
        [SwaggerOperation(
            Summary = "Request additional stalls",
            Description = "Submits one stall registration per requested stall for the current organizer. Requires Organization role.")]
        public async Task<IActionResult> CreateAdditionalStalls([FromBody] CreateAdditionalStallsRequest request)
        {
            var result = await _organizerRegistrationService.CreateAdditionalStallsAsync(request);
            return Ok(ApiResult<List<OrganizerRegistrationResponse>>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("additional-online")]
        [SwaggerOperation(
            Summary = "Request online selling",
            Description = "Submits an online registration for the current organizer. Requires Organization role.")]
        public async Task<IActionResult> CreateAdditionalOnline()
        {
            var result = await _organizerRegistrationService.CreateAdditionalOnlineAsync();
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id:guid}/approve")]
        [SwaggerOperation(
            Summary = "Approve an organizer registration",
            Description = "Approves a pending registration, with a commission rate for online registrations. Requires Manager role.")]
        public async Task<IActionResult> ApproveOrganizerRegistration(
            Guid id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] ApproveOrganizerRegistrationRequest? request)
        {
            var result = await _organizerRegistrationService.ApproveOrganizerRegistrationAsync(id, request);
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpPut("{id:guid}/reject")]
        [SwaggerOperation(
            Summary = "Reject an organizer registration",
            Description = "Rejects a pending registration with a reason. Requires Manager role.")]
        public async Task<IActionResult> RejectOrganizerRegistration(Guid id, [FromBody] RejectOrganizerRegistrationRequest request)
        {
            var result = await _organizerRegistrationService.RejectOrganizerRegistrationAsync(id, request);
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }

        [Authorize]
        [HttpPut("{id:guid}/confirm")]
        [SwaggerOperation(
            Summary = "Confirm the proposed commission rate",
            Description = "Accepts the commission rate of the current user's online registration. Requires authentication.")]
        public async Task<IActionResult> ConfirmOrganizerRegistration(Guid id)
        {
            var result = await _organizerRegistrationService.ConfirmOrganizerRegistrationAsync(id);
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }

        [Authorize]
        [HttpPut("{id:guid}/cancel")]
        [SwaggerOperation(
            Summary = "Cancel an organizer registration",
            Description = "Cancels the current user's organizer registration. Requires authentication.")]
        public async Task<IActionResult> CancelOrganizerRegistration(
            Guid id,
            [FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Allow)] CancelOrganizerRegistrationRequest? request)
        {
            var result = await _organizerRegistrationService.CancelOrganizerRegistrationAsync(id, request);
            return Ok(ApiResult<OrganizerRegistrationResponse>.Success(result));
        }
    }
}

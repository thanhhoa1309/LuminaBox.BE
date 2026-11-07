using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Voucher;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/vouchers")]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;

        public VouchersController(IVoucherService voucherService)
        {
            _voucherService = voucherService;
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("platform")]
        [SwaggerOperation(
            Summary = "Create platform voucher",
            Description = "Creates a new platform-wide voucher. Requires Manager role.")]
        public async Task<IActionResult> CreatePlatformVoucher([FromBody] CreateVoucherRequest request)
        {
            var result = await _voucherService.CreatePlatformVoucherAsync(request);
            return Ok(ApiResult<VoucherResponse>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpPost("organizer")]
        [SwaggerOperation(
            Summary = "Create organizer voucher",
            Description = "Creates a new organizer-scoped voucher for the current organizer. Requires Organization role.")]
        public async Task<IActionResult> CreateOrganizerVoucher([FromBody] CreateVoucherRequest request)
        {
            var result = await _voucherService.CreateOrganizerVoucherAsync(request);
            return Ok(ApiResult<VoucherResponse>.Success(result));
        }

        [Authorize(Roles = "Manager")]
        [HttpGet]
        [SwaggerOperation(
            Summary = "Get paginated list of vouchers",
            Description = "Gets a paginated list of all vouchers across the platform. Requires Manager role.")]
        public async Task<IActionResult> GetAllVouchers([FromQuery] AllVoucherListQuery filter)
        {
            var result = await _voucherService.GetAllVouchersAsync(filter);
            return Ok(ApiResult<Pagination<VoucherResponse>>.Success(result));
        }

        [Authorize(Roles = "Organization")]
        [HttpGet("me")]
        [SwaggerOperation(
            Summary = "Get paginated list of current organizer vouchers",
            Description = "Gets a paginated list of vouchers belonging to the current organizer. Requires Organization role.")]
        public async Task<IActionResult> GetMyOrganizerVouchers([FromQuery] VoucherListQuery filter)
        {
            var result = await _voucherService.GetMyOrganizerVouchersAsync(filter);
            return Ok(ApiResult<Pagination<VoucherResponse>>.Success(result));
        }

        [Authorize]
        [HttpGet("available")]
        [SwaggerOperation(
            Summary = "Get available vouchers to claim",
            Description = "Gets active vouchers that the current user can claim into their voucher wallet. Requires authentication.")]
        public async Task<IActionResult> GetAvailableVouchers([FromQuery] Guid? organizerId = null)
        {
            var result = await _voucherService.GetAvailableVouchersAsync(organizerId);
            return Ok(ApiResult<List<VoucherResponse>>.Success(result));
        }

        [Authorize]
        [HttpPost("{id:guid}/claim")]
        [SwaggerOperation(
            Summary = "Claim voucher into wallet",
            Description = "Claims an active voucher into the current user's voucher wallet. Requires authentication.")]
        public async Task<IActionResult> ClaimVoucher(Guid id)
        {
            var result = await _voucherService.ClaimVoucherAsync(id);
            return Ok(ApiResult<UserVoucherResponse>.Success(result));
        }

        [Authorize]
        [HttpGet("my")]
        [SwaggerOperation(
            Summary = "Get claimed vouchers in current user wallet",
            Description = "Gets all vouchers claimed by the current user for checkout. Requires authentication.")]
        public async Task<IActionResult> GetMyClaimedVouchers()
        {
            var result = await _voucherService.GetMyClaimedVouchersAsync();
            return Ok(ApiResult<List<UserVoucherResponse>>.Success(result));
        }

        [Authorize]
        [HttpDelete("my/{userVoucherId:guid}")]
        [SwaggerOperation(
            Summary = "Remove claimed voucher from wallet",
            Description = "Removes a claimed voucher from the current user's voucher wallet. Requires authentication.")]
        public async Task<IActionResult> RemoveClaimedVoucher(Guid userVoucherId)
        {
            await _voucherService.RemoveClaimedVoucherAsync(userVoucherId);
            return Ok(ApiResult<object?>.Success(null, "Claimed voucher removed successfully."));
        }

        [Authorize(Roles = "Manager,Organization")]
        [HttpGet("{id:guid}")]
        [SwaggerOperation(
            Summary = "Get voucher details by id",
            Description = "Gets the details of a voucher by id. Requires Manager or Organization role.")]
        public async Task<IActionResult> GetVoucherByDetail(Guid id)
        {
            var result = await _voucherService.GetVoucherByDetail(id);
            return Ok(ApiResult<VoucherResponse>.Success(result));
        }

        [Authorize(Roles = "Manager,Organization")]
        [HttpPut("{id:guid}")]
        [SwaggerOperation(
            Summary = "Update voucher",
            Description = "Updates an existing voucher's properties. Requires Manager or Organization role.")]
        public async Task<IActionResult> UpdateVoucher(Guid id, [FromBody] UpdateVoucherRequest request)
        {
            var result = await _voucherService.UpdateVoucherAsync(id, request);
            return Ok(ApiResult<VoucherResponse>.Success(result));
        }

        [Authorize(Roles = "Manager,Organization")]
        [HttpPut("{id:guid}/status")]
        [SwaggerOperation(
            Summary = "Update voucher status",
            Description = "Activates or deactivates an existing voucher. Requires Manager or Organization role.")]
        public async Task<IActionResult> UpdateVoucherStatus(Guid id, [FromBody] UpdateVoucherStatusRequest request)
        {
            var result = await _voucherService.UpdateVoucherStatusAsync(id, request);
            return Ok(ApiResult<VoucherResponse>.Success(result));
        }

        [Authorize(Roles = "Manager,Organization")]
        [HttpDelete("{id:guid}")]
        [SwaggerOperation(
            Summary = "Delete voucher",
            Description = "Soft deletes a voucher that has not been used. Requires Manager or Organization role.")]
        public async Task<IActionResult> DeleteVoucher(Guid id)
        {
            await _voucherService.DeleteVoucherAsync(id);
            return Ok(ApiResult<object?>.Success(null, "Voucher deleted successfully."));
        }
    }
}


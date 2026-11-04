using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Geography;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api")]
    public class LocationsController : ControllerBase
    {
        private readonly ILocationService _locationService;
        private readonly IGeoSyncService _geoSyncService;

        public LocationsController(ILocationService locationService, IGeoSyncService geoSyncService)
        {
            _locationService = locationService;
            _geoSyncService = geoSyncService;
        }

        [Authorize(Roles = "Manager")]
        [HttpPost("locations/sync")]
        [SwaggerOperation(
            Summary = "Sync provinces, districts and wards",
            Description = "Imports missing units and refreshes GHN ids. Existing rows are kept, so it is safe to repeat; a second run while one is active returns 409. Requires Manager role.")]
        public async Task<IActionResult> Sync(CancellationToken cancellationToken)
        {
            var result = await _geoSyncService.SyncAsync(cancellationToken);
            return Ok(ApiResult<GeoSyncReport>.Success(result));
        }

        [HttpGet("provinces")]
        [SwaggerOperation(
            Summary = "Get paginated list of provinces",
            Description = "Gets a paginated list of provinces with search. Public.")]
        public async Task<IActionResult> GetProvinces(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _locationService.GetProvincesAsync(search, page, pageSize);
            return Ok(ApiResult<Pagination<ProvinceResponse>>.Success(result));
        }

        [HttpGet("provinces/{provinceId:guid}/districts")]
        [SwaggerOperation(
            Summary = "Get paginated list of districts of a province",
            Description = "Gets a paginated list of districts of a province with search. Public.")]
        public async Task<IActionResult> GetDistricts(
            Guid provinceId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _locationService.GetDistrictsAsync(provinceId, search, page, pageSize);
            return Ok(ApiResult<Pagination<DistrictResponse>>.Success(result));
        }

        [HttpGet("districts/{districtId:guid}/wards")]
        [SwaggerOperation(
            Summary = "Get paginated list of wards of a district",
            Description = "Gets a paginated list of wards of a district with search. Public.")]
        public async Task<IActionResult> GetWards(
            Guid districtId,
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50)
        {
            var result = await _locationService.GetWardsAsync(districtId, search, page, pageSize);
            return Ok(ApiResult<Pagination<WardResponse>>.Success(result));
        }
    }
}

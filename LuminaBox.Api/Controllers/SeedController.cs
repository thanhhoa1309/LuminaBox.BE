using Microsoft.AspNetCore.Mvc;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using Swashbuckle.AspNetCore.Annotations;

namespace LuminaBox.Api.Controllers
{
    [ApiController]
    [Route("api/seed")]
    public class SeedController : ControllerBase
    {
        private readonly ISeedService _seedService;
        private readonly IHostEnvironment _environment;

        public SeedController(ISeedService seedService, IHostEnvironment environment)
        {
            _seedService = seedService;
            _environment = environment;
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Seed sample data",
            Description = "Seeds users, catalog, stalls, organizers, products, boxes, vouchers and events. Development only. Requires geography sync first.")]
        public async Task<IActionResult> SeedAllData()
        {
            EnsureDevelopment();
            await _seedService.SeedAllDataAsync();
            return Ok(ApiResult.Success("200", "All sample data seeded successfully."));
        }

        [HttpDelete]
        [SwaggerOperation(
            Summary = "Clear application data",
            Description = "Truncates all application tables except Provinces/Districts/Wards and EF migration history. Development only.")]
        public async Task<IActionResult> ClearAllData()
        {
            EnsureDevelopment();
            await _seedService.ClearAllDataAsync();
            return Ok(ApiResult.Success("200", "All application data cleared successfully. Geography catalog preserved."));
        }

        private void EnsureDevelopment()
        {
            if (!_environment.IsDevelopment())
                throw ErrorHelper.Forbidden("Seed endpoints are only available in Development.");
        }
    }
}

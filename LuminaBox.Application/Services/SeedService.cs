using Microsoft.Extensions.Logging;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public partial class SeedService : ISeedService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SeedService> _logger;
        private DateTime _seedNow;

        public SeedService(IUnitOfWork unitOfWork, ILogger<SeedService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task SeedAllDataAsync()
        {
            _logger.LogInformation("Starting LuminaBox seed");
            _seedNow = DateTime.UtcNow;

            await SeedUsersAsync();
            await SeedCakeCategoriesAsync();
            await SeedFillingsAsync();
            await SeedStallsAsync();
            await SeedOrganizersAsync();
            await SeedSalesAccessAsync();
            await SeedProductsAsync();
            await SeedBoxItemsAsync();
            await SeedVouchersAsync();
            await SeedEventsAsync();
            await SeedSampleOrdersAsync();

            _logger.LogInformation("Finished LuminaBox seed");
        }

        public async Task ClearAllDataAsync()
        {
            _logger.LogInformation("Starting clear all application data");

            await _unitOfWork.TruncateAllApplicationTablesAsync();

            if (await _unitOfWork.Users.AnyIncludingDeletedAsync())
                throw ErrorHelper.Internal("Clear incomplete: Users table still has rows.");

            if (await _unitOfWork.Products.AnyIncludingDeletedAsync())
                throw ErrorHelper.Internal("Clear incomplete: Products table still has rows.");

            _logger.LogInformation("Finished clear all application data (Provinces/Districts/Wards preserved)");
        }

        private async Task SaveAsync()
        {
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

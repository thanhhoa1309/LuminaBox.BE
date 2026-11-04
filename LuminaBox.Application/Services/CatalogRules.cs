using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    internal static class CatalogRules
    {
        public static void ValidateTiers(List<PriceTierRequest> tiers, decimal basePrice)
        {
            if (tiers.Any(t => t.MinQuantity < 2 || t.UnitPrice <= 0))
                throw ErrorHelper.BadRequest("Each tier needs a minimum quantity of at least 2 and a positive unit price.", "E5.8");

            if (tiers.GroupBy(t => t.MinQuantity).Any(g => g.Count() > 1))
                throw ErrorHelper.BadRequest("Minimum quantities must be unique.", "E5.8");

            var ordered = tiers.OrderBy(t => t.MinQuantity).ToList();
            var previous = basePrice;

            foreach (var tier in ordered)
            {
                if (tier.UnitPrice >= previous)
                    throw ErrorHelper.BadRequest("Tier prices must be below the base price and decrease as quantity grows.", "E5.8");

                previous = tier.UnitPrice;
            }
        }

        public static async Task ValidateStocksAsync(IUnitOfWork unitOfWork, Guid organizerId, List<StockRequest> stocks)
        {
            if (stocks.Any(s => s.StockQuantity < 0))
                throw ErrorHelper.BadRequest("Stock quantity must not be negative.");

            if (stocks.GroupBy(s => s.StallId).Any(g => g.Count() > 1))
                throw ErrorHelper.BadRequest("Each stall can appear only once.");

            var stallIds = stocks.Where(s => s.StallId.HasValue).Select(s => s.StallId!.Value).ToList();

            if (stallIds.Count == 0)
                return;

            var rentedStallIds = await unitOfWork.StallRentals.GetQueryable()
                .Where(r => r.OrganizerId == organizerId
                    && r.Status == RentalStatus.Active
                    && stallIds.Contains(r.StallId))
                .Select(r => r.StallId)
                .ToListAsync();

            if (stallIds.Except(rentedStallIds).Any())
                throw ErrorHelper.Conflict("Stock can only be allocated to stalls with your active rental.", "E5.6");
        }
    }
}

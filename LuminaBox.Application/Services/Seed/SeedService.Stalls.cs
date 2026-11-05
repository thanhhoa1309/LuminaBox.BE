using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private static readonly (string Code, string Name, string AddressLine, decimal RentalPrice, string? Description)[] StallSeeds =
        {
            ("STL-001", "Quầy Ánh Trăng", "Lô A1, Khu ẩm thực trung tâm", 5_000_000m, "Gian hàng lớn gần lối chính"),
            ("STL-002", "Quầy Sen Vàng", "Lô B2, Khu lễ hội", 3_500_000m, "Quầy vừa, view đường đi bộ"),
            ("STL-003", "Quầy Mini Gift", "Lô C3, Khu quà tặng", 2_000_000m, "Quầy nhỏ bán hộp quà")
        };

        private async Task SeedStallsAsync()
        {
            var ward = await _unitOfWork.Wards.GetQueryable()
                .OrderBy(w => w.Name)
                .FirstOrDefaultAsync();

            if (ward is null)
            {
                throw ErrorHelper.BadRequest(
                    "No Ward found. Run geography sync first: POST /api/locations/sync (Manager) or `dotnet run --project LuminaBox.Api -- sync-geo`.");
            }

            foreach (var seed in StallSeeds)
            {
                var exists = await _unitOfWork.Stalls.GetQueryable()
                    .AnyAsync(s => s.Code == seed.Code);
                if (exists)
                    continue;

                await _unitOfWork.Stalls.AddAsync(new Stall
                {
                    Id = Guid.NewGuid(),
                    WardId = ward.Id,
                    Code = seed.Code,
                    Name = seed.Name,
                    AddressLine = seed.AddressLine,
                    RentalPrice = seed.RentalPrice,
                    Description = seed.Description,
                    Status = StallStatus.Available,
                    NewWardName = ward.Name
                });
            }

            await SaveAsync();
        }

        private Task<Stall?> FindStallByCodeAsync(string code)
        {
            return _unitOfWork.Stalls.GetQueryable()
                .FirstOrDefaultAsync(s => s.Code == code);
        }
    }
}

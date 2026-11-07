using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private static readonly (string Code, string Name, string AddressLine, decimal RentalPrice, string Description, decimal Latitude, decimal Longitude)[] StallSeeds =
        {
            ("STL-001", "Quầy Ánh Trăng", "Lô A1, Phố đi bộ Nguyễn Huệ", 5_000_000m, "Gian lớn ở trung tâm phố đi bộ", 10.7743m, 106.7036m),
            ("STL-002", "Quầy Sen Vàng", "Lô B2, Công viên Tao Đàn", 3_500_000m, "Gian vừa cạnh lối vào lễ hội", 10.7747m, 106.6925m),
            ("STL-003", "Quầy Lồng Đèn", "Lô C3, Hồ Con Rùa", 4_000_000m, "Gian góc, đông khách buổi tối", 10.7826m, 106.6958m),
            ("STL-004", "Quầy Mini Gift", "Lô D4, Phố sách Nguyễn Văn Bình", 2_000_000m, "Gian nhỏ chuyên quà tặng", 10.7804m, 106.7001m),
            ("STL-005", "Quầy Hội Trăng Rằm", "Lô E5, Công viên Lê Văn Tám", 3_000_000m, "Gian vừa, có mái che", 10.7884m, 106.6927m),
            ("STL-006", "Quầy Bến Nhà Rồng", "Lô F6, Bến Nhà Rồng", 2_800_000m, "Gian ven sông, có chỗ để xe", 10.7680m, 106.7068m)
        };

        private async Task SeedStallsAsync()
        {
            var wards = await FindSeedWardsAsync();

            if (wards.Count == 0)
            {
                throw ErrorHelper.BadRequest(
                    "No Ward found. Run geography sync first: POST /api/locations/sync (Manager) or `dotnet run --project LuminaBox.Api -- sync-geo`.");
            }

            for (var i = 0; i < StallSeeds.Length; i++)
            {
                var seed = StallSeeds[i];
                var exists = await _unitOfWork.Stalls.GetQueryable()
                    .AnyAsync(s => s.Code == seed.Code);
                if (exists)
                    continue;

                var ward = wards[i % wards.Count];

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
                    Latitude = seed.Latitude,
                    Longitude = seed.Longitude,
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

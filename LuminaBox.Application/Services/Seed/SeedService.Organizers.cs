using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private const string Moonlight = "Moonlight Bakery";
        private const string Luna = "Luna Online Cakes";
        private const string TrangVang = "Trăng Vàng Heritage";
        private const string HoaSen = "Hoa Sen Patisserie";
        private const string MayTra = "Mây Trà Artisan";

        private static readonly (string Email, string Brand, string Phone, string BusinessInfo, string[] StallCodes)[] OrganizerSeeds =
        {
            (Org1Email, Moonlight, "0901000003", "Tiệm bánh nướng thủ công, có quầy tại phố đi bộ và công viên Tao Đàn.", new[] { "STL-001", "STL-002" }),
            (Org2Email, Luna, "0901000004", "Thương hiệu bánh hiện đại, chỉ bán online và giao toàn quốc.", Array.Empty<string>()),
            (Org3Email, TrangVang, "0901000008", "Công thức gia truyền ba thế hệ, nguyên liệu chọn lọc.", new[] { "STL-003" }),
            (Org4Email, HoaSen, "0901000009", "Bánh dẻo tinh tế theo mùa, gói quà sẵn.", new[] { "STL-004" }),
            (Org5Email, MayTra, "0901000010", "Bánh trà và chocolate ít ngọt, làm theo mẻ nhỏ.", Array.Empty<string>())
        };

        private async Task SeedOrganizersAsync()
        {
            var manager = await RequireUserByEmailAsync(ManagerEmail);

            foreach (var seed in OrganizerSeeds)
            {
                var user = await RequireUserByEmailAsync(seed.Email);
                var organizer = await EnsureOrganizerAsync(user, manager.Id, seed.Brand, seed.Phone, seed.Email, seed.BusinessInfo);

                foreach (var stallCode in seed.StallCodes)
                    await EnsureStallRentalAsync(organizer, stallCode);
            }

            await SaveAsync();
        }

        private async Task<Organizer> EnsureOrganizerAsync(
            User user,
            Guid approvedBy,
            string brandName,
            string contactPhone,
            string contactEmail,
            string businessInfo,
            OrganizerStatus status = OrganizerStatus.Active)
        {
            var existing = await _unitOfWork.Organizers.GetQueryable()
                .FirstOrDefaultAsync(o => o.UserId == user.Id);

            if (existing is not null)
                return existing;

            if (user.Role != UserRole.Organization)
            {
                user.Role = UserRole.Organization;
                await _unitOfWork.Users.Update(user);
            }

            var organizer = new Organizer
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                BrandName = brandName,
                ContactPhone = contactPhone,
                ContactEmail = contactEmail,
                BusinessInfo = businessInfo,
                Status = status,
                ApprovedBy = approvedBy,
                ApprovedAt = _seedNow
            };

            await _unitOfWork.Organizers.AddAsync(organizer);
            await SaveAsync();
            return organizer;
        }

        private async Task EnsureStallRentalAsync(Organizer organizer, string stallCode, RentalStatus status = RentalStatus.Active)
        {
            var stall = await FindStallByCodeAsync(stallCode)
                ?? throw ErrorHelper.Internal($"Seed stall '{stallCode}' not found.");

            var existing = await _unitOfWork.StallRentals.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizer.Id && r.StallId == stall.Id);

            if (existing)
                return;

            var isActive = status == RentalStatus.Active;
            var start = DateOnly.FromDateTime(_seedNow.AddDays(isActive ? -7 : -60));
            var end = DateOnly.FromDateTime(isActive ? _seedNow.AddMonths(3) : _seedNow.AddDays(-5));

            await _unitOfWork.StallRentals.AddAsync(new StallRental
            {
                Id = Guid.NewGuid(),
                StallId = stall.Id,
                OrganizerId = organizer.Id,
                StartDate = start,
                EndDate = end,
                Status = status
            });

            if (isActive && stall.Status != StallStatus.Rented)
            {
                stall.Status = StallStatus.Rented;
                await _unitOfWork.Stalls.Update(stall);
            }

            await SaveAsync();
        }

        private Task<Organizer> RequireOrganizerByBrandAsync(string brandName)
        {
            return _unitOfWork.Organizers.GetQueryable()
                .FirstAsync(o => o.BrandName == brandName);
        }
    }
}

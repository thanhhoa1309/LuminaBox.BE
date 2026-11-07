using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private const decimal SeedCommissionRate = 10m;

        private async Task SeedSalesAccessAsync()
        {
            var manager = await RequireUserByEmailAsync(ManagerEmail);
            var wards = await FindSeedWardsAsync();

            for (var i = 0; i < OrganizerSeeds.Length; i++)
            {
                var organizer = await RequireOrganizerByBrandAsync(OrganizerSeeds[i].Brand);

                await EnsureOnlineContractAsync(organizer, manager.Id);
                await EnsureAddressAsync(
                    organizer.UserId,
                    wards[i % wards.Count],
                    AddressType.Pickup,
                    organizer.BrandName,
                    organizer.ContactPhone ?? OrganizerSeeds[i].Phone,
                    $"Kho {organizer.BrandName}, số {12 + i} Lê Lợi");
            }

            var buyers = new[] { UserEmail, User2Email, User3Email, User4Email };

            for (var i = 0; i < buyers.Length; i++)
            {
                var user = await RequireUserByEmailAsync(buyers[i]);

                await EnsureAddressAsync(
                    user.Id,
                    wards[(i + 1) % wards.Count],
                    AddressType.Shipping,
                    user.FullName,
                    user.Phone ?? "0901000002",
                    $"{45 + i} Nguyễn Thị Minh Khai");
            }

            var mainBuyer = await RequireUserByEmailAsync(UserEmail);
            await EnsureAddressAsync(
                mainBuyer.Id,
                wards[2 % wards.Count],
                AddressType.Shipping,
                "Văn phòng Minh Anh",
                mainBuyer.Phone ?? "0901000002",
                "Tầng 8, 72 Lê Thánh Tôn");

            await SaveAsync();
        }

        // GHN can only price wards it knows, so seeded addresses prefer wards matched by sync-geo
        private async Task<List<Ward>> FindSeedWardsAsync()
        {
            var matched = _unitOfWork.Wards.GetQueryable()
                .Where(w => w.GhnWardCode != null && w.District.GhnDistrictId != null);

            var inHoChiMinh = await matched
                .Where(w => w.District.Province.Name.Contains("Hồ Chí Minh"))
                .OrderBy(w => w.Code)
                .Take(12)
                .ToListAsync();

            if (inHoChiMinh.Count > 0)
                return inHoChiMinh;

            var anyMatched = await matched.OrderBy(w => w.Code).Take(12).ToListAsync();

            if (anyMatched.Count > 0)
                return anyMatched;

            return await _unitOfWork.Wards.GetQueryable().OrderBy(w => w.Code).Take(12).ToListAsync();
        }

        private async Task EnsureOnlineContractAsync(Organizer organizer, Guid managerId)
        {
            var exists = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizer.Id
                    && r.Type == RegistrationType.Online
                    && r.Status == RegistrationStatus.Approved);

            if (exists)
                return;

            await _unitOfWork.OrganizerRegistrations.AddAsync(new OrganizerRegistration
            {
                Id = Guid.NewGuid(),
                UserId = organizer.UserId,
                OrganizerId = organizer.Id,
                Type = RegistrationType.Online,
                BrandName = organizer.BrandName,
                ContactPhone = organizer.ContactPhone,
                BusinessInfo = organizer.BusinessInfo,
                CommissionRate = SeedCommissionRate,
                Status = RegistrationStatus.Approved,
                ReviewedBy = managerId,
                ReviewedAt = _seedNow,
                ConfirmedAt = _seedNow
            });

            await SaveAsync();
        }

        private async Task EnsureAddressAsync(
            Guid userId,
            Ward ward,
            AddressType type,
            string contactName,
            string phone,
            string addressLine)
        {
            var addresses = await _unitOfWork.Addresses.GetQueryable()
                .Where(a => a.UserId == userId && a.Type == type)
                .Select(a => a.AddressLine)
                .ToListAsync();

            if (addresses.Contains(addressLine))
                return;

            await _unitOfWork.Addresses.AddAsync(new Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                WardId = ward.Id,
                Type = type,
                ContactName = contactName,
                Phone = phone,
                AddressLine = addressLine,
                IsDefault = addresses.Count == 0
            });

            await SaveAsync();
        }
    }
}

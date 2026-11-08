using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private const decimal SeedCommissionRate = 10m;
        public const string User1VinhomesAddressLine = "S7.03 Vinhomes, Long Bình";

        private async Task SeedSalesAccessAsync()
        {
            var manager = await RequireUserByEmailAsync(ManagerEmail);
            var wards = await FindSeedWardsAsync();
            var longBinhWard = await FindLongBinhHcmcWardAsync() ?? wards[0];

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

            var mainBuyer = await RequireUserByEmailAsync(UserEmail);
            await EnsureDefaultShippingAddressAsync(
                mainBuyer.Id,
                longBinhWard,
                mainBuyer.FullName,
                mainBuyer.Phone ?? "0901000002",
                User1VinhomesAddressLine);

            await EnsureAddressAsync(
                mainBuyer.Id,
                wards[2 % wards.Count],
                AddressType.Shipping,
                "Văn phòng Minh Anh",
                mainBuyer.Phone ?? "0901000002",
                "Tầng 8, 72 Lê Thánh Tôn");

            var otherBuyers = new[] { User2Email, User3Email, User4Email };

            for (var i = 0; i < otherBuyers.Length; i++)
            {
                var user = await RequireUserByEmailAsync(otherBuyers[i]);

                await EnsureAddressAsync(
                    user.Id,
                    wards[(i + 1) % wards.Count],
                    AddressType.Shipping,
                    user.FullName,
                    user.Phone ?? "0901000002",
                    $"{46 + i} Nguyễn Thị Minh Khai");
            }

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

        private async Task<Ward?> FindLongBinhHcmcWardAsync()
        {
            return await _unitOfWork.Wards.GetQueryable()
                .Include(w => w.District)
                    .ThenInclude(d => d.Province)
                .Where(w => w.GhnWardCode != null && w.District.GhnDistrictId != null)
                .Where(w => w.District.Province.Name.Contains("Hồ Chí Minh"))
                .Where(w => w.Name == "Phường Long Bình")
                .OrderBy(w => w.Code)
                .FirstOrDefaultAsync();
        }

        private async Task EnsureDefaultShippingAddressAsync(
            Guid userId,
            Ward ward,
            string contactName,
            string phone,
            string addressLine)
        {
            var shippingAddresses = await _unitOfWork.Addresses.GetQueryable()
                .Where(a => a.UserId == userId && a.Type == AddressType.Shipping)
                .ToListAsync();

            var target = shippingAddresses.FirstOrDefault(a => a.AddressLine == addressLine);

            if (target is null)
            {
                target = new Address
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    WardId = ward.Id,
                    Type = AddressType.Shipping,
                    ContactName = contactName,
                    Phone = phone,
                    AddressLine = addressLine,
                    IsDefault = true
                };
                await _unitOfWork.Addresses.AddAsync(target);
                shippingAddresses.Add(target);
            }
            else
            {
                target.WardId = ward.Id;
                target.ContactName = contactName;
                target.Phone = phone;
                target.IsDefault = true;
                await _unitOfWork.Addresses.Update(target);
            }

            foreach (var other in shippingAddresses.Where(a => a.Id != target.Id && a.IsDefault))
            {
                other.IsDefault = false;
                await _unitOfWork.Addresses.Update(other);
            }

            await SaveAsync();
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

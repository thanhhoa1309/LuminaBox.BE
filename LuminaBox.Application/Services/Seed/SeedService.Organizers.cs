using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private async Task SeedOrganizersAsync()
        {
            var manager = await RequireUserByEmailAsync(ManagerEmail);
            var org1User = await RequireUserByEmailAsync(Org1Email);
            var org2User = await RequireUserByEmailAsync(Org2Email);

            var org1 = await EnsureOrganizerAsync(
                org1User,
                manager.Id,
                "Moonlight Bakery",
                "0901000003",
                Org1Email,
                "Specialty baked mooncakes at physical stalls.");

            await EnsureOrganizerAsync(
                org2User,
                manager.Id,
                "Luna Online Cakes",
                "0901000004",
                Org2Email,
                "Online-only mooncake boutique.");

            await EnsureStallRentalAsync(org1, "STL-001", manager.Id);
            await SaveAsync();
        }

        private async Task<Organizer> EnsureOrganizerAsync(
            User user,
            Guid approvedBy,
            string brandName,
            string contactPhone,
            string contactEmail,
            string businessInfo)
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
                Status = OrganizerStatus.Active,
                ApprovedBy = approvedBy,
                ApprovedAt = _seedNow
            };

            await _unitOfWork.Organizers.AddAsync(organizer);
            await SaveAsync();
            return organizer;
        }

        private async Task EnsureStallRentalAsync(Organizer organizer, string stallCode, Guid _)
        {
            var stall = await FindStallByCodeAsync(stallCode)
                ?? throw ErrorHelper.Internal($"Seed stall '{stallCode}' not found.");

            var existing = await _unitOfWork.StallRentals.GetQueryable()
                .AnyAsync(r =>
                    r.OrganizerId == organizer.Id &&
                    r.StallId == stall.Id &&
                    (r.Status == RentalStatus.Active || r.Status == RentalStatus.Confirmed));

            if (existing)
                return;

            var start = DateOnly.FromDateTime(_seedNow.AddDays(-7));
            var end = DateOnly.FromDateTime(_seedNow.AddMonths(3));

            await _unitOfWork.StallRentals.AddAsync(new StallRental
            {
                Id = Guid.NewGuid(),
                StallId = stall.Id,
                OrganizerId = organizer.Id,
                StartDate = start,
                EndDate = end,
                Status = RentalStatus.Active
            });

            if (stall.Status != StallStatus.Rented)
            {
                stall.Status = StallStatus.Rented;
                await _unitOfWork.Stalls.Update(stall);
            }
        }

        private Task<Organizer> RequireOrganizerByBrandAsync(string brandName)
        {
            return _unitOfWork.Organizers.GetQueryable()
                .FirstAsync(o => o.BrandName == brandName);
        }
    }
}

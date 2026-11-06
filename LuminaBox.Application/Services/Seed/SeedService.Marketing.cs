using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private sealed record VoucherSeed(
            string Code,
            VoucherScope Scope,
            string? Brand,
            string Name,
            string Description,
            DiscountType DiscountType,
            decimal DiscountValue,
            decimal? MaxDiscountAmount,
            decimal MinOrderAmount,
            VoucherStatus Status = VoucherStatus.Active,
            int StartOffsetDays = -1,
            int EndOffsetDays = 60,
            int UsageLimit = 100,
            int UsedCount = 0,
            int UsageLimitPerUser = 1);

        private static readonly VoucherSeed[] VoucherSeeds =
        {
            new("LUMINA10", VoucherScope.Platform, null, "Giảm 10% toàn sàn", "Đơn từ 200.000đ, giảm tối đa 50.000đ", DiscountType.Percent, 10m, 50_000m, 200_000m),
            new("WELCOME50K", VoucherScope.Platform, null, "Giảm 50K chào mừng", "Đơn từ 300.000đ", DiscountType.Fixed, 50_000m, null, 300_000m),
            new("TRUNGTHU30K", VoucherScope.Platform, null, "Rằm tháng Tám giảm 30K", "Đơn từ 150.000đ", DiscountType.Fixed, 30_000m, null, 150_000m),
            new("MOON15", VoucherScope.Organizer, Moonlight, "Moonlight giảm 15%", "Đơn từ 250.000đ, giảm tối đa 80.000đ", DiscountType.Percent, 15m, 80_000m, 250_000m),
            new("LUNA20K", VoucherScope.Organizer, Luna, "Luna giảm 20K", "Đơn từ 100.000đ", DiscountType.Fixed, 20_000m, null, 100_000m),
            new("TRANGVANG10", VoucherScope.Organizer, TrangVang, "Trăng Vàng giảm 10%", "Đơn từ 300.000đ, giảm tối đa 60.000đ", DiscountType.Percent, 10m, 60_000m, 300_000m),
            new("HOASEN25K", VoucherScope.Organizer, HoaSen, "Hoa Sen giảm 25K", "Đơn từ 200.000đ", DiscountType.Fixed, 25_000m, null, 200_000m),
            new("MAYTRA12", VoucherScope.Organizer, MayTra, "Mây Trà giảm 12%", "Đơn từ 150.000đ, giảm tối đa 40.000đ", DiscountType.Percent, 12m, 40_000m, 150_000m)
        };

        private async Task SeedVouchersAsync()
        {
            foreach (var seed in VoucherSeeds)
                await EnsureVoucherAsync(seed);

            await SaveAsync();
        }

        private async Task<Voucher> EnsureVoucherAsync(VoucherSeed seed)
        {
            var existing = await _unitOfWork.Vouchers.GetQueryable()
                .FirstOrDefaultAsync(v => v.Code == seed.Code);

            if (existing is not null)
                return existing;

            var manager = await RequireUserByEmailAsync(ManagerEmail);
            var organizer = seed.Brand is null ? null : await RequireOrganizerByBrandAsync(seed.Brand);

            var voucher = new Voucher
            {
                Id = Guid.NewGuid(),
                Code = seed.Code,
                Scope = seed.Scope,
                OrganizerId = organizer?.Id,
                CreatedBy = organizer?.UserId ?? manager.Id,
                Name = seed.Name,
                Description = seed.Description,
                DiscountType = seed.DiscountType,
                DiscountValue = seed.DiscountValue,
                MaxDiscountAmount = seed.MaxDiscountAmount,
                MinOrderAmount = seed.MinOrderAmount,
                UsageLimit = seed.UsageLimit,
                UsageLimitPerUser = seed.UsageLimitPerUser,
                UsedCount = seed.UsedCount,
                StartAt = _seedNow.AddDays(seed.StartOffsetDays),
                EndAt = _seedNow.AddDays(seed.EndOffsetDays),
                Status = seed.Status
            };

            await _unitOfWork.Vouchers.AddAsync(voucher);
            await SaveAsync();
            return voucher;
        }

        private async Task SeedEventsAsync()
        {
            var org1 = await RequireOrganizerByBrandAsync(Moonlight);
            var org3 = await RequireOrganizerByBrandAsync(TrangVang);

            await EnsureEventAsync(null, "Lễ hội Bánh Trung Thu LuminaBox", "Sự kiện toàn sàn mùa Trung Thu", _seedNow.AddDays(7), _seedNow.AddDays(14), EventStatus.Published);
            await EnsureEventAsync(org1.Id, "Moonlight Open Stall Day", "Ngày mở quầy Moonlight Bakery", _seedNow.AddDays(3), _seedNow.AddDays(5), EventStatus.Published);
            await EnsureEventAsync(org3.Id, "Đêm Trăng Gia Truyền", "Trải nghiệm làm bánh cùng nghệ nhân", _seedNow.AddDays(10), _seedNow.AddDays(11), EventStatus.Published);

            await SaveAsync();
        }

        private async Task EnsureEventAsync(
            Guid? organizerId,
            string title,
            string description,
            DateTime startAt,
            DateTime endAt,
            EventStatus status)
        {
            var existing = await _unitOfWork.Events.GetQueryable()
                .FirstOrDefaultAsync(e => e.Title == title && e.OrganizerId == organizerId);

            if (existing is not null)
                return;

            var evt = new Event
            {
                Id = Guid.NewGuid(),
                OrganizerId = organizerId,
                Title = title,
                Description = description,
                StartAt = startAt,
                EndAt = endAt,
                Status = status
            };

            await _unitOfWork.Events.AddAsync(evt);
            await SaveAsync();

            await _unitOfWork.EventImages.AddAsync(new EventImage
            {
                Id = Guid.NewGuid(),
                EventId = evt.Id,
                ImageUrl = SeedImage(title, "7d2a2d"),
                SortOrder = 0,
                IsPrimary = true
            });
        }
    }
}

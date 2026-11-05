using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private async Task SeedVouchersAsync()
        {
            var manager = await RequireUserByEmailAsync(ManagerEmail);
            var org1 = await RequireOrganizerByBrandAsync("Moonlight Bakery");

            await EnsureVoucherAsync(
                code: "LUMINA10",
                scope: VoucherScope.Platform,
                organizerId: null,
                createdBy: manager.Id,
                name: "Giảm 10% toàn sàn",
                description: "Áp dụng đơn từ 200.000đ",
                discountType: DiscountType.Percent,
                discountValue: 10m,
                maxDiscountAmount: 50_000m,
                minOrderAmount: 200_000m);

            await EnsureVoucherAsync(
                code: "WELCOME50K",
                scope: VoucherScope.Platform,
                organizerId: null,
                createdBy: manager.Id,
                name: "Giảm 50K chào mừng",
                description: "Giảm cố định 50.000đ",
                discountType: DiscountType.Fixed,
                discountValue: 50_000m,
                maxDiscountAmount: null,
                minOrderAmount: 300_000m);

            await EnsureVoucherAsync(
                code: "MOON15",
                scope: VoucherScope.Organizer,
                organizerId: org1.Id,
                createdBy: org1.UserId,
                name: "Moonlight giảm 15%",
                description: "Voucher riêng Moonlight Bakery",
                discountType: DiscountType.Percent,
                discountValue: 15m,
                maxDiscountAmount: 80_000m,
                minOrderAmount: 250_000m);

            await SaveAsync();
        }

        private async Task EnsureVoucherAsync(
            string code,
            VoucherScope scope,
            Guid? organizerId,
            Guid createdBy,
            string name,
            string description,
            DiscountType discountType,
            decimal discountValue,
            decimal? maxDiscountAmount,
            decimal minOrderAmount)
        {
            var exists = await _unitOfWork.Vouchers.GetQueryable()
                .AnyAsync(v => v.Code == code);
            if (exists)
                return;

            await _unitOfWork.Vouchers.AddAsync(new Voucher
            {
                Id = Guid.NewGuid(),
                Code = code,
                Scope = scope,
                OrganizerId = organizerId,
                CreatedBy = createdBy,
                Name = name,
                Description = description,
                DiscountType = discountType,
                DiscountValue = discountValue,
                MaxDiscountAmount = maxDiscountAmount,
                MinOrderAmount = minOrderAmount,
                UsageLimit = 100,
                UsageLimitPerUser = 1,
                UsedCount = 0,
                StartAt = _seedNow.AddDays(-1),
                EndAt = _seedNow.AddMonths(2),
                Status = VoucherStatus.Active
            });
        }

        private async Task SeedEventsAsync()
        {
            var org1 = await RequireOrganizerByBrandAsync("Moonlight Bakery");

            await EnsureEventAsync(
                organizerId: null,
                title: "Lễ hội Bánh Trung Thu LuminaBox",
                description: "Sự kiện toàn sàn mùa Trung Thu",
                startAt: _seedNow.AddDays(7),
                endAt: _seedNow.AddDays(14),
                status: EventStatus.Published);

            await EnsureEventAsync(
                organizerId: org1.Id,
                title: "Moonlight Open Stall Day",
                description: "Ngày mở quầy Moonlight Bakery",
                startAt: _seedNow.AddDays(3),
                endAt: _seedNow.AddDays(5),
                status: EventStatus.Published);

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
                ImageUrl = PlaceholderImage,
                SortOrder = 0,
                IsPrimary = true
            });
        }
    }
}

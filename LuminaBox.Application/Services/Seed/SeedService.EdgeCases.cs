using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        public const string EdgeNoAddressEmail = "edge.noaddress@luminabox.com";
        public const string EdgeLockedEmail = "edge.locked@luminabox.com";
        public const string EdgeUnverifiedEmail = "edge.unverified@luminabox.com";
        public const string EdgeCartEmail = "edge.cart@luminabox.com";
        public const string EdgeBakeryEmail = "edge.bakery@luminabox.com";
        public const string EdgeSuspendedEmail = "edge.suspended@luminabox.com";
        public const string EdgeNoOnlineEmail = "edge.noonline@luminabox.com";
        public const string EdgeNoPickupEmail = "edge.nopickup@luminabox.com";
        public const string EdgeExpiredRentalEmail = "edge.expiredrental@luminabox.com";

        private const string EdgeBakery = "Edge Bakery";
        private const string EdgeSuspended = "Edge Shop Bị Đình Chỉ";
        private const string EdgeNoOnline = "Edge Shop Chưa Có Hợp Đồng Online";
        private const string EdgeNoPickup = "Edge Shop Thiếu Điểm Lấy Hàng";
        private const string EdgeExpiredRental = "Edge Shop Hết Hạn Thuê";

        private static readonly ProductSeed[] EdgeProductSeeds =
        {
            new(EdgeBakery, "Edge - Hết hàng", "Tồn kho bằng 0.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Baked, MungBean, "Hết hàng", 1, 150, 80_000m, 0)
            }),
            new(EdgeBakery, "Edge - Còn đúng 1 cái", "Mua 1 được, mua 2 bị chặn.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Baked, Lotus, "Còn 1", 1, 150, 90_000m, 1)
            }),
            new(EdgeBakery, "Edge - Kho đang tắt", "Còn hàng nhưng dòng kho đã tắt.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Soft, Lotus, "Kho tắt", 0, 120, 70_000m, 20)
            }),
            new(EdgeBakery, "Edge - Hai biến thể cùng loại và nhân", "Khác nhau ở số trứng, giá chênh nhiều.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Baked, Mixed, "Biến thể 1 trứng", 1, 150, 100_000m, 30),
                new(Baked, Mixed, "Biến thể 2 trứng", 2, 220, 200_000m, 30)
            }),
            new(EdgeBakery, "Edge - Chỉ bán tại 2 gian hàng", "Không có kho online, phải chọn gian hàng.", new[] { "STL-005", "STL-006" }, new CakeSeed[]
            {
                new(Soft, MungBean, "Chỉ bán tại quầy", 0, 120, 75_000m, null, 15)
            }),
            new(EdgeBakery, "Edge - Giá 50 triệu", "Kiểm tra hiển thị số lớn và giới hạn Stripe.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Premium, Mixed, "Giá rất cao", 2, 300, 50_000_000m, 5)
            }),
            new(EdgeBakery, "Edge - Tên sản phẩm rất dài để kiểm tra giao diện có bị vỡ dòng hay tràn khung khi hiển thị ở thẻ sản phẩm và giỏ hàng", "Kiểm tra hiển thị.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Mini, Chocolate, "Biến thể cũng có tên dài để kiểm tra dòng trong giỏ hàng và trang thanh toán", 0, 60, 40_000m, 50)
            }),
            new(EdgeNoOnline, "Edge - Shop chưa có hợp đồng Online", "Hiện ở danh sách nhưng thêm giỏ bị chặn.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Baked, MungBean, "Không bán được", 1, 150, 80_000m, 30)
            }),
            new(EdgeNoPickup, "Edge - Shop thiếu điểm lấy hàng", "Thêm giỏ được, lỗi khi tính phí ship.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Soft, Lotus, "Thiếu điểm lấy hàng", 0, 120, 70_000m, 30)
            }),
            new(EdgeExpiredRental, "Edge - Gian hàng đã hết hạn thuê", "Còn kho tại quầy nhưng hợp đồng đã hết hạn.", new[] { "STL-006" }, new CakeSeed[]
            {
                new(Baked, Lotus, "Quầy hết hạn", 1, 150, 85_000m, null, 10)
            }),
            new(EdgeSuspended, "Edge - Shop bị đình chỉ", "Ẩn khỏi danh sách, chỉ còn trong giỏ của edge.cart.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Baked, MungBean, "Shop đình chỉ", 1, 150, 80_000m, 30)
            })
        };

        private static readonly VoucherSeed[] EdgeVoucherSeeds =
        {
            new("EDGE-EXPIRED", VoucherScope.Platform, null, "Edge: đã hết hạn", "Hết hạn từ hôm qua", DiscountType.Fixed, 10_000m, null, 0m, StartOffsetDays: -30, EndOffsetDays: -1),
            new("EDGE-NOTSTARTED", VoucherScope.Platform, null, "Edge: chưa bắt đầu", "Năm ngày nữa mới dùng được", DiscountType.Fixed, 10_000m, null, 0m, StartOffsetDays: 5, EndOffsetDays: 30),
            new("EDGE-EXHAUSTED", VoucherScope.Platform, null, "Edge: hết lượt", "Đã dùng hết 5/5 lượt", DiscountType.Fixed, 10_000m, null, 0m, UsageLimit: 5, UsedCount: 5),
            new("EDGE-INACTIVE", VoucherScope.Platform, null, "Edge: đã tắt", "Voucher đang tắt", DiscountType.Fixed, 10_000m, null, 0m, Status: VoucherStatus.Inactive),
            new("EDGE-MIN5M", VoucherScope.Platform, null, "Edge: đơn tối thiểu 5 triệu", "Đơn thường không đủ điều kiện", DiscountType.Fixed, 100_000m, null, 5_000_000m),
            new("EDGE-FULL100", VoucherScope.Platform, null, "Edge: giảm 100%", "Tiền hàng về 0, chỉ còn phí ship", DiscountType.Percent, 100m, null, 0m),
            new("EDGE-OTHERSHOP", VoucherScope.Organizer, Moonlight, "Edge: chỉ dùng cho Moonlight", "Sai shop khi áp cho Edge Bakery", DiscountType.Fixed, 10_000m, null, 0m)
        };

        public async Task SeedEdgeCasesAsync()
        {
            await SeedAllDataAsync();

            var manager = await RequireUserByEmailAsync(ManagerEmail);
            var wards = await FindSeedWardsAsync();

            await SeedEdgeUsersAsync();
            await SeedEdgeOrganizersAsync(manager.Id, wards[0]);

            foreach (var seed in EdgeProductSeeds)
                await EnsureProductSeedAsync(seed);

            await EnsureProductSeedAsync(EdgeHiddenProduct("Edge - Sản phẩm đã ẩn"), ProductStatus.Inactive);
            await EnsureProductSeedAsync(EdgeHiddenProduct("Edge - Sản phẩm nháp"), ProductStatus.Draft);

            await DisableStockAsync("Kho tắt");

            await EnsureBoxSeedAsync(new BoxSeed(EdgeBakery, "Edge - Hộp còn 1", "Mua 2 hộp bị chặn.", 3, 300, 90_000m, Array.Empty<string>(), 1, 0));

            foreach (var seed in EdgeVoucherSeeds)
                await EnsureVoucherAsync(seed);

            await SeedEdgeCartAsync(wards[0]);
        }

        private static ProductSeed EdgeHiddenProduct(string name)
            => new(EdgeBakery, name, "Không hiện ở danh sách công khai.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Baked, Chocolate, name, 0, 100, 60_000m, 20)
            });

        private async Task SeedEdgeUsersAsync()
        {
            await EnsureUserAsync(EdgeNoAddressEmail, "Edge Chưa Có Địa Chỉ", "0902000001", UserRole.User, UserPassword);
            await EnsureUserAsync(EdgeLockedEmail, "Edge Bị Khóa", "0902000002", UserRole.User, UserPassword, UserStatus.Locked);
            await EnsureUserAsync(EdgeUnverifiedEmail, "Edge Chưa Xác Thực", "0902000003", UserRole.User, UserPassword, isEmailVerified: false);
            await EnsureUserAsync(EdgeCartEmail, "Edge Giỏ Có Vấn Đề", "0902000004", UserRole.User, UserPassword);
            await EnsureUserAsync(EdgeBakeryEmail, "Chủ Edge Bakery", "0902000005", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(EdgeSuspendedEmail, "Chủ Shop Đình Chỉ", "0902000006", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(EdgeNoOnlineEmail, "Chủ Shop Chưa Online", "0902000007", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(EdgeNoPickupEmail, "Chủ Shop Thiếu Lấy Hàng", "0902000008", UserRole.Organization, OrganizationPassword);
            await EnsureUserAsync(EdgeExpiredRentalEmail, "Chủ Shop Hết Hạn", "0902000009", UserRole.Organization, OrganizationPassword);
            await SaveAsync();
        }

        private async Task SeedEdgeOrganizersAsync(Guid managerId, Ward ward)
        {
            var bakery = await EnsureEdgeOrganizerAsync(EdgeBakeryEmail, EdgeBakery, managerId);
            await EnsureOnlineContractAsync(bakery, managerId);
            await EnsureAddressAsync(bakery.UserId, ward, AddressType.Pickup, EdgeBakery, "0902000005", "Kho Edge Bakery");
            await EnsureStallRentalAsync(bakery, "STL-005");
            await EnsureStallRentalAsync(bakery, "STL-006");

            var suspended = await EnsureEdgeOrganizerAsync(EdgeSuspendedEmail, EdgeSuspended, managerId, OrganizerStatus.Suspended);
            await EnsureOnlineContractAsync(suspended, managerId);
            await EnsureAddressAsync(suspended.UserId, ward, AddressType.Pickup, EdgeSuspended, "0902000006", "Kho Shop Đình Chỉ");

            var noOnline = await EnsureEdgeOrganizerAsync(EdgeNoOnlineEmail, EdgeNoOnline, managerId);
            await EnsureAddressAsync(noOnline.UserId, ward, AddressType.Pickup, EdgeNoOnline, "0902000007", "Kho Shop Chưa Online");

            var noPickup = await EnsureEdgeOrganizerAsync(EdgeNoPickupEmail, EdgeNoPickup, managerId);
            await EnsureOnlineContractAsync(noPickup, managerId);

            var expiredRental = await EnsureEdgeOrganizerAsync(EdgeExpiredRentalEmail, EdgeExpiredRental, managerId);
            await EnsureOnlineContractAsync(expiredRental, managerId);
            await EnsureAddressAsync(expiredRental.UserId, ward, AddressType.Pickup, EdgeExpiredRental, "0902000009", "Kho Shop Hết Hạn");
            await EnsureStallRentalAsync(expiredRental, "STL-006", RentalStatus.Expired);
        }

        private async Task<Organizer> EnsureEdgeOrganizerAsync(string email, string brand, Guid managerId, OrganizerStatus status = OrganizerStatus.Active)
        {
            var user = await RequireUserByEmailAsync(email);
            return await EnsureOrganizerAsync(user, managerId, brand, user.Phone ?? "0902000000", email, "Dữ liệu kiểm thử trường hợp biên.", status);
        }

        private async Task DisableStockAsync(string productItemName)
        {
            var stocks = await _unitOfWork.StallProductItems.GetQueryable()
                .Where(s => s.ProductItem.Name == productItemName)
                .ToListAsync();

            foreach (var stock in stocks)
                stock.IsActive = false;

            await SaveAsync();
        }

        private async Task SeedEdgeCartAsync(Ward ward)
        {
            var user = await RequireUserByEmailAsync(EdgeCartEmail);

            await EnsureAddressAsync(user.Id, ward, AddressType.Shipping, user.FullName, user.Phone ?? "0902000004", "12 Đường Kiểm Thử");

            var cart = await _unitOfWork.Carts.GetQueryable().FirstOrDefaultAsync(c => c.UserId == user.Id);

            if (cart is null)
            {
                cart = new Cart { Id = Guid.NewGuid(), UserId = user.Id };
                await _unitOfWork.Carts.AddAsync(cart);
                await SaveAsync();
            }

            await EnsureCartLineAsync(cart.Id, "Còn 1", 1, null);
            await EnsureCartLineAsync(cart.Id, "Hết hàng", 2, null);
            await EnsureCartLineAsync(cart.Id, "Kho tắt", 1, null);
            await EnsureCartLineAsync(cart.Id, "Edge - Sản phẩm đã ẩn", 1, null);
            await EnsureCartLineAsync(cart.Id, "Shop đình chỉ", 1, null);
            await EnsureCartLineAsync(cart.Id, "Biến thể 2 trứng", 1, 1_000m);

            var codes = new[] { "EDGE-EXPIRED", "EDGE-INACTIVE", "EDGE-EXHAUSTED", "EDGE-MIN5M", "EDGE-FULL100", "EDGE-OTHERSHOP" };

            foreach (var code in codes)
            {
                var voucher = await _unitOfWork.Vouchers.GetQueryable().FirstAsync(v => v.Code == code);
                var claimed = await _unitOfWork.UserVouchers.GetQueryable()
                    .AnyAsync(uv => uv.UserId == user.Id && uv.VoucherId == voucher.Id);

                if (!claimed)
                {
                    await _unitOfWork.UserVouchers.AddAsync(new UserVoucher
                    {
                        Id = Guid.NewGuid(),
                        UserId = user.Id,
                        VoucherId = voucher.Id,
                        ClaimedAt = _seedNow
                    });
                }
            }

            await SaveAsync();
        }

        private async Task EnsureCartLineAsync(Guid cartId, string productItemName, int quantity, decimal? stalePrice)
        {
            var item = await _unitOfWork.ProductItems.GetQueryable().FirstAsync(i => i.Name == productItemName);

            if (await _unitOfWork.CartItems.GetQueryable().AnyAsync(c => c.CartId == cartId && c.ProductItemId == item.Id))
                return;

            await _unitOfWork.CartItems.AddAsync(new CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cartId,
                ItemType = OrderItemType.Product,
                ProductItemId = item.Id,
                Quantity = quantity,
                UnitPrice = stalePrice ?? item.Price
            });
        }
    }
}

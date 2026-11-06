using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private const string Baked = "Bánh Nướng";
        private const string Soft = "Bánh Dẻo";
        private const string Modern = "Bánh Hiện Đại";
        private const string Mini = "Mini Cake";
        private const string Premium = "Bánh Tinh Tế";

        private const string MungBean = "Đậu xanh";
        private const string Lotus = "Hạt sen";
        private const string Mixed = "Thập cẩm";
        private const string Chocolate = "Chocolate";
        private const string Matcha = "Trà xanh";
        private const string Durian = "Sầu riêng";
        private const string Taro = "Khoai môn";
        private const string SaltedEgg = "Trứng muối";

        private sealed record CakeSeed(string Category, string Filling, string Name, int EggCount, int Weight, decimal Price, int? OnlineStock, int StallStock = 0);

        private sealed record ProductSeed(string Brand, string Name, string Description, string[] StallCodes, CakeSeed[] Cakes);

        private sealed record BoxSeed(string Brand, string Name, string Description, int Capacity, int Weight, decimal Price, string[] StallCodes, int OnlineStock, int StallStock);

        private static readonly Dictionary<string, (string Main, string Alternate)> CategoryColors = new()
        {
            [Baked] = ("a3262b", "7d2a2d"),
            [Soft] = ("2f7d4f", "4f9a70"),
            [Modern] = ("5b3a29", "8a5a3c"),
            [Mini] = ("c49a1c", "b7791f"),
            [Premium] = ("341618", "7d2a2d")
        };

        private static readonly ProductSeed[] ProductSeeds =
        {
            new(Moonlight, "Bánh Trung Thu Cổ Điển", "Vỏ nướng vàng óng, nhân truyền thống ít ngọt.", new[] { "STL-001", "STL-002" }, new CakeSeed[]
            {
                new(Baked, MungBean, "Nướng Đậu Xanh 1 trứng", 1, 150, 85_000m, 120, 60),
                new(Baked, Lotus, "Nướng Hạt Sen 1 trứng", 1, 150, 90_000m, 100, 50),
                new(Baked, Mixed, "Nướng Thập Cẩm 2 trứng", 2, 200, 125_000m, 80, 40)
            }),
            new(Moonlight, "Bánh Nướng Trứng Muối Đặc Biệt", "Trứng muối chọn lọc, có hai cỡ 1 trứng và 2 trứng.", new[] { "STL-001" }, new CakeSeed[]
            {
                new(Baked, SaltedEgg, "Trứng Muối 1 trứng", 1, 180, 110_000m, 70, 30),
                new(Baked, SaltedEgg, "Trứng Muối 2 trứng", 2, 220, 145_000m, 50, 25)
            }),
            new(Moonlight, "Bánh Dẻo Sen Vàng", "Vỏ dẻo mềm, thơm hương bưởi.", new[] { "STL-002" }, new CakeSeed[]
            {
                new(Soft, Lotus, "Dẻo Hạt Sen", 0, 120, 75_000m, 90, 45),
                new(Soft, MungBean, "Dẻo Đậu Xanh", 0, 120, 70_000m, 90, 45)
            }),
            new(Luna, "Bánh Hiện Đại Luna", "Vị mới cho người trẻ, giao toàn quốc.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Modern, Chocolate, "Chocolate Lava", 0, 100, 95_000m, 120),
                new(Modern, Matcha, "Matcha Cloud", 0, 100, 92_000m, 110)
            }),
            new(Luna, "Mini Cake Luna", "Bánh nhỏ một miếng, hợp để thử nhiều vị.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Mini, Chocolate, "Mini Chocolate", 0, 60, 45_000m, 200),
                new(Mini, Matcha, "Mini Trà Xanh", 0, 60, 45_000m, 200),
                new(Mini, Taro, "Mini Khoai Môn", 0, 60, 42_000m, 180)
            }),
            new(Luna, "Bánh Lava Sầu Riêng", "Nhân sầu riêng chảy, dùng ngon nhất khi hâm nóng.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Modern, Durian, "Lava Sầu Riêng", 0, 130, 130_000m, 60)
            }),
            new(TrangVang, "Bánh Nướng Thượng Hạng", "Dòng cao cấp, nhân vi cá giả và hạt điều.", new[] { "STL-003" }, new CakeSeed[]
            {
                new(Premium, Mixed, "Thượng Hạng Thập Cẩm 2 trứng", 2, 250, 210_000m, 45, 20),
                new(Premium, Lotus, "Thượng Hạng Hạt Sen 1 trứng", 1, 200, 180_000m, 50, 25)
            }),
            new(TrangVang, "Bánh Nướng Gia Truyền", "Công thức gia đình từ năm 1968.", new[] { "STL-003" }, new CakeSeed[]
            {
                new(Baked, MungBean, "Gia Truyền Đậu Xanh", 1, 160, 95_000m, 90, 40),
                new(Baked, Taro, "Gia Truyền Khoai Môn", 1, 160, 98_000m, 80, 35)
            }),
            new(HoaSen, "Bánh Dẻo Hoa Sen", "Tạo hình hoa sen, gói sẵn từng chiếc.", new[] { "STL-004" }, new CakeSeed[]
            {
                new(Soft, Lotus, "Hoa Sen Hạt Sen", 0, 130, 80_000m, 85, 40),
                new(Soft, Matcha, "Hoa Sen Trà Xanh", 0, 130, 82_000m, 75, 35)
            }),
            new(HoaSen, "Bánh Dẻo Khoai Môn", "Sắc tím tự nhiên, vị bùi nhẹ.", new[] { "STL-004" }, new CakeSeed[]
            {
                new(Soft, Taro, "Dẻo Khoai Môn", 0, 125, 78_000m, 70, 30)
            }),
            new(MayTra, "Bánh Trà Xanh Mây", "Bột trà Thái Nguyên, vị đắng nhẹ.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Modern, Matcha, "Mây Trà Xanh", 0, 110, 105_000m, 70),
                new(Mini, Matcha, "Mây Trà Xanh Mini", 0, 60, 48_000m, 150)
            }),
            new(MayTra, "Bánh Chocolate Mây", "Chocolate đen 70%, ít đường.", Array.Empty<string>(), new CakeSeed[]
            {
                new(Modern, Chocolate, "Mây Chocolate", 0, 110, 115_000m, 65)
            })
        };

        private static readonly BoxSeed[] BoxSeeds =
        {
            new(Moonlight, "Hộp 4 Bánh Moonlight", "Hộp cứng 4 bánh, phù hợp quà tặng.", 4, 400, 120_000m, new[] { "STL-001" }, 40, 20),
            new(Moonlight, "Hộp 6 Bánh Premium", "Hộp sơn mài 6 bánh, kèm túi giấy.", 6, 550, 180_000m, new[] { "STL-001" }, 25, 15),
            new(Luna, "Hộp Online Luna 4", "Hộp ship toàn quốc 4 bánh.", 4, 380, 99_000m, Array.Empty<string>(), 80, 0),
            new(TrangVang, "Hộp Gia Truyền 4", "Hộp gỗ khắc tên thương hiệu.", 4, 600, 250_000m, new[] { "STL-003" }, 30, 12),
            new(HoaSen, "Hộp Hoa Sen 6", "Sáu bánh dẻo, hộp giấy dó.", 6, 420, 160_000m, new[] { "STL-004" }, 35, 15),
            new(MayTra, "Hộp Mây Trà 4", "Bốn bánh ít ngọt kèm gói trà.", 4, 360, 110_000m, Array.Empty<string>(), 50, 0)
        };

        private async Task SeedProductsAsync()
        {
            foreach (var seed in ProductSeeds)
                await EnsureProductSeedAsync(seed);

            await SaveAsync();
        }

        private async Task SeedBoxItemsAsync()
        {
            foreach (var seed in BoxSeeds)
                await EnsureBoxSeedAsync(seed);

            await SaveAsync();
        }

        private async Task<Product> EnsureProductSeedAsync(ProductSeed seed, ProductStatus status = ProductStatus.Active)
        {
            var organizer = await RequireOrganizerByBrandAsync(seed.Brand);
            var product = await EnsureProductAsync(organizer.Id, seed.Name, seed.Description, status);
            var stallIds = await ResolveStallIdsAsync(seed.StallCodes);

            foreach (var cake in seed.Cakes)
                await EnsureProductItemWithStockAsync(product, cake, stallIds);

            await SaveAsync();
            return product;
        }

        private async Task<BoxItem> EnsureBoxSeedAsync(BoxSeed seed)
        {
            var organizer = await RequireOrganizerByBrandAsync(seed.Brand);
            var stallIds = await ResolveStallIdsAsync(seed.StallCodes);
            var box = await _unitOfWork.BoxItems.GetQueryable()
                .FirstOrDefaultAsync(b => b.OrganizerId == organizer.Id && b.Name == seed.Name);

            if (box is null)
            {
                box = new BoxItem
                {
                    Id = Guid.NewGuid(),
                    OrganizerId = organizer.Id,
                    Name = seed.Name,
                    Description = seed.Description,
                    Capacity = seed.Capacity,
                    Weight = seed.Weight,
                    LengthCm = 25,
                    WidthCm = 25,
                    HeightCm = 8,
                    Price = seed.Price,
                    Status = ProductStatus.Active
                };

                await _unitOfWork.BoxItems.AddAsync(box);
                await SaveAsync();

                await _unitOfWork.BoxItemImages.AddAsync(new BoxItemImage
                {
                    Id = Guid.NewGuid(),
                    BoxItemId = box.Id,
                    Url = SeedImage(seed.Name, "c49a1c"),
                    SortOrder = 0
                });

                await _unitOfWork.PriceTiers.AddAsync(new PriceTier
                {
                    Id = Guid.NewGuid(),
                    BoxItemId = box.Id,
                    ItemType = OrderItemType.Box,
                    MinQuantity = 5,
                    UnitPrice = Math.Round(seed.Price * 0.88m, 0)
                });

                await SaveAsync();
            }

            await EnsureBoxStockAsync(box.Id, null, seed.OnlineStock);

            foreach (var stallId in stallIds)
                await EnsureBoxStockAsync(box.Id, stallId, seed.StallStock);

            return box;
        }

        private async Task<List<Guid>> ResolveStallIdsAsync(string[] stallCodes)
        {
            var stallIds = new List<Guid>();

            foreach (var code in stallCodes)
            {
                var stall = await FindStallByCodeAsync(code);
                if (stall is not null)
                    stallIds.Add(stall.Id);
            }

            return stallIds;
        }

        private async Task<Product> EnsureProductAsync(
            Guid organizerId,
            string name,
            string description,
            ProductStatus status)
        {
            var existing = await _unitOfWork.Products.GetQueryable()
                .FirstOrDefaultAsync(p => p.OrganizerId == organizerId && p.Name == name);

            if (existing is not null)
                return existing;

            var product = new Product
            {
                Id = Guid.NewGuid(),
                OrganizerId = organizerId,
                Name = name,
                Description = description,
                Status = status
            };

            await _unitOfWork.Products.AddAsync(product);
            await SaveAsync();
            return product;
        }

        private async Task EnsureProductItemWithStockAsync(Product product, CakeSeed cake, List<Guid> stallIds)
        {
            var category = await _unitOfWork.CakeCategories.GetQueryable().FirstAsync(c => c.Name == cake.Category);
            var filling = await _unitOfWork.Fillings.GetQueryable().FirstAsync(f => f.Name == cake.Filling);

            var item = await _unitOfWork.ProductItems.GetQueryable()
                .FirstOrDefaultAsync(i =>
                    i.ProductId == product.Id &&
                    i.CategoryId == category.Id &&
                    i.FillingId == filling.Id &&
                    i.EggCount == cake.EggCount &&
                    i.Weight == cake.Weight);

            if (item is null)
            {
                item = new ProductItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    CategoryId = category.Id,
                    FillingId = filling.Id,
                    Name = cake.Name,
                    EggCount = cake.EggCount,
                    Weight = cake.Weight,
                    LengthCm = 9,
                    WidthCm = 9,
                    HeightCm = 4.5m,
                    Price = cake.Price
                };

                await _unitOfWork.ProductItems.AddAsync(item);
                await SaveAsync();

                var colors = CategoryColors.GetValueOrDefault(cake.Category, ("a3262b", "7d2a2d"));

                await _unitOfWork.ProductItemImages.AddAsync(new ProductItemImage
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = item.Id,
                    Url = SeedImage(cake.Name, colors.Item1),
                    SortOrder = 0
                });

                await _unitOfWork.ProductItemImages.AddAsync(new ProductItemImage
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = item.Id,
                    Url = SeedImage(cake.Filling, colors.Item2),
                    SortOrder = 1
                });

                await _unitOfWork.PriceTiers.AddAsync(new PriceTier
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = item.Id,
                    ItemType = OrderItemType.Product,
                    MinQuantity = 10,
                    UnitPrice = Math.Round(cake.Price * 0.9m, 0)
                });

                await SaveAsync();
            }

            if (cake.OnlineStock.HasValue)
                await EnsureProductStockAsync(item.Id, null, cake.OnlineStock.Value);

            foreach (var stallId in stallIds)
                await EnsureProductStockAsync(item.Id, stallId, cake.StallStock);
        }

        private static string SeedImage(string text, string background)
        {
            var label = Uri.EscapeDataString(RemoveDiacritics(text));
            return $"https://placehold.co/800x800/{background}/fbf5ea/png?text={label}";
        }

        private static string RemoveDiacritics(string text)
        {
            var builder = new StringBuilder();

            foreach (var character in text.Normalize(NormalizationForm.FormD))
            {
                if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                    builder.Append(character);
            }

            return builder.ToString().Replace('đ', 'd').Replace('Đ', 'D');
        }

        private async Task EnsureProductStockAsync(Guid productItemId, Guid? stallId, int quantity)
        {
            var exists = stallId.HasValue
                ? await _unitOfWork.StallProductItems.GetQueryable()
                    .AnyAsync(s => s.ProductItemId == productItemId && s.StallId == stallId)
                : await _unitOfWork.StallProductItems.GetQueryable()
                    .AnyAsync(s => s.ProductItemId == productItemId && s.StallId == null);

            if (exists)
                return;

            await _unitOfWork.StallProductItems.AddAsync(new StallProductItem
            {
                Id = Guid.NewGuid(),
                ProductItemId = productItemId,
                StallId = stallId,
                StockQuantity = quantity,
                IsActive = true
            });
        }

        private async Task EnsureBoxStockAsync(Guid boxItemId, Guid? stallId, int quantity)
        {
            var exists = stallId.HasValue
                ? await _unitOfWork.StallBoxItems.GetQueryable()
                    .AnyAsync(s => s.BoxItemId == boxItemId && s.StallId == stallId)
                : await _unitOfWork.StallBoxItems.GetQueryable()
                    .AnyAsync(s => s.BoxItemId == boxItemId && s.StallId == null);

            if (exists)
                return;

            await _unitOfWork.StallBoxItems.AddAsync(new StallBoxItem
            {
                Id = Guid.NewGuid(),
                BoxItemId = boxItemId,
                StallId = stallId,
                StockQuantity = quantity,
                IsActive = true
            });
        }
    }
}

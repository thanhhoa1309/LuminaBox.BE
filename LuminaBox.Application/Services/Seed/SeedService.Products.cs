using Microsoft.EntityFrameworkCore;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Services
{
    public partial class SeedService
    {
        private const string PlaceholderImage = "https://placehold.co/600x400/png?text=LuminaBox";

        private async Task SeedProductsAsync()
        {
            var org1 = await RequireOrganizerByBrandAsync("Moonlight Bakery");
            var org2 = await RequireOrganizerByBrandAsync("Luna Online Cakes");

            var categories = await _unitOfWork.CakeCategories.GetQueryable().ToListAsync();
            var fillings = await _unitOfWork.Fillings.GetQueryable().ToListAsync();
            var stall = await FindStallByCodeAsync("STL-001");

            var baked = categories.First(c => c.Name == "Bánh Nướng");
            var soft = categories.First(c => c.Name == "Bánh Dẻo");
            var modern = categories.First(c => c.Name == "Bánh Hiện Đại");

            var mung = fillings.First(f => f.Name == "Đậu xanh");
            var lotus = fillings.First(f => f.Name == "Hạt sen");
            var mixed = fillings.First(f => f.Name == "Thập cẩm");
            var chocolate = fillings.First(f => f.Name == "Chocolate");
            var matcha = fillings.First(f => f.Name == "Trà xanh");

            var classic = await EnsureProductAsync(org1.Id, "Bánh Trung Thu Cổ Điển", "Bánh nướng truyền thống", ProductStatus.Active);
            await EnsureProductItemWithStockAsync(classic, baked, mung, "Bánh Nướng Đậu Xanh 1 trứng", 1, 150, 8, 8, 4, 85_000m, stall?.Id, 100, 50);
            await EnsureProductItemWithStockAsync(classic, baked, lotus, "Bánh Nướng Hạt Sen 1 trứng", 1, 150, 8, 8, 4, 90_000m, stall?.Id, 80, 40);
            await EnsureProductItemWithStockAsync(classic, baked, mixed, "Bánh Nướng Thập Cẩm 2 trứng", 2, 200, 9, 9, 4.5m, 110_000m, stall?.Id, 60, 30);

            var softLine = await EnsureProductAsync(org1.Id, "Bánh Dẻo Sen Vàng", "Bánh dẻo nhân hạt sen", ProductStatus.Active);
            await EnsureProductItemWithStockAsync(softLine, soft, lotus, "Bánh Dẻo Hạt Sen", 0, 120, 7, 7, 3.5m, 75_000m, stall?.Id, 70, 35);

            var onlineModern = await EnsureProductAsync(org2.Id, "Bánh Hiện Đại Luna", "Online exclusive modern cakes", ProductStatus.Active);
            await EnsureProductItemWithStockAsync(onlineModern, modern, chocolate, "Chocolate Lava Mini", 0, 100, 6, 6, 3, 95_000m, null, 120, null);
            await EnsureProductItemWithStockAsync(onlineModern, modern, matcha, "Matcha Cloud Cake", 0, 100, 6, 6, 3, 92_000m, null, 100, null);

            await SaveAsync();
        }

        private async Task SeedBoxItemsAsync()
        {
            var org1 = await RequireOrganizerByBrandAsync("Moonlight Bakery");
            var org2 = await RequireOrganizerByBrandAsync("Luna Online Cakes");
            var stall = await FindStallByCodeAsync("STL-001");

            await EnsureBoxItemWithStockAsync(
                org1.Id,
                "Hộp 4 Bánh Moonlight",
                "Hộp cứng 4 bánh, phù hợp quà tặng",
                4, 400, 25, 25, 8, 120_000m,
                stall?.Id, 40, 20);

            await EnsureBoxItemWithStockAsync(
                org1.Id,
                "Hộp 6 Bánh Premium",
                "Hộp cao cấp 6 bánh",
                6, 550, 30, 22, 9, 180_000m,
                stall?.Id, 25, 15);

            await EnsureBoxItemWithStockAsync(
                org2.Id,
                "Hộp Online Luna 4",
                "Hộp ship toàn quốc 4 bánh",
                4, 380, 24, 24, 8, 99_000m,
                null, 80, null);

            await SaveAsync();
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

        private async Task EnsureProductItemWithStockAsync(
            Product product,
            CakeCategory category,
            Filling filling,
            string name,
            int eggCount,
            int weight,
            decimal lengthCm,
            decimal widthCm,
            decimal heightCm,
            decimal price,
            Guid? stallId,
            int onlineStock,
            int? stallStock)
        {
            var item = await _unitOfWork.ProductItems.GetQueryable()
                .FirstOrDefaultAsync(i =>
                    i.ProductId == product.Id &&
                    i.CategoryId == category.Id &&
                    i.FillingId == filling.Id &&
                    i.EggCount == eggCount &&
                    i.Weight == weight);

            if (item is null)
            {
                item = new ProductItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = product.Id,
                    CategoryId = category.Id,
                    FillingId = filling.Id,
                    Name = name,
                    EggCount = eggCount,
                    Weight = weight,
                    LengthCm = lengthCm,
                    WidthCm = widthCm,
                    HeightCm = heightCm,
                    Price = price
                };

                await _unitOfWork.ProductItems.AddAsync(item);
                await SaveAsync();

                await _unitOfWork.ProductItemImages.AddAsync(new ProductItemImage
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = item.Id,
                    Url = PlaceholderImage,
                    SortOrder = 0
                });

                await _unitOfWork.PriceTiers.AddAsync(new PriceTier
                {
                    Id = Guid.NewGuid(),
                    ProductItemId = item.Id,
                    ItemType = OrderItemType.Product,
                    MinQuantity = 10,
                    UnitPrice = Math.Round(price * 0.9m, 0)
                });

                await SaveAsync();
            }

            await EnsureProductStockAsync(item.Id, null, onlineStock);
            if (stallId.HasValue && stallStock.HasValue)
                await EnsureProductStockAsync(item.Id, stallId, stallStock.Value);
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

        private async Task EnsureBoxItemWithStockAsync(
            Guid organizerId,
            string name,
            string description,
            int capacity,
            int weight,
            decimal lengthCm,
            decimal widthCm,
            decimal heightCm,
            decimal price,
            Guid? stallId,
            int onlineStock,
            int? stallStock)
        {
            var box = await _unitOfWork.BoxItems.GetQueryable()
                .FirstOrDefaultAsync(b => b.OrganizerId == organizerId && b.Name == name);

            if (box is null)
            {
                box = new BoxItem
                {
                    Id = Guid.NewGuid(),
                    OrganizerId = organizerId,
                    Name = name,
                    Description = description,
                    Capacity = capacity,
                    Weight = weight,
                    LengthCm = lengthCm,
                    WidthCm = widthCm,
                    HeightCm = heightCm,
                    Price = price,
                    Status = ProductStatus.Active
                };

                await _unitOfWork.BoxItems.AddAsync(box);
                await SaveAsync();

                await _unitOfWork.BoxItemImages.AddAsync(new BoxItemImage
                {
                    Id = Guid.NewGuid(),
                    BoxItemId = box.Id,
                    Url = PlaceholderImage,
                    SortOrder = 0
                });

                await _unitOfWork.PriceTiers.AddAsync(new PriceTier
                {
                    Id = Guid.NewGuid(),
                    BoxItemId = box.Id,
                    ItemType = OrderItemType.Box,
                    MinQuantity = 5,
                    UnitPrice = Math.Round(price * 0.88m, 0)
                });

                await SaveAsync();
            }

            await EnsureBoxStockAsync(box.Id, null, onlineStock);
            if (stallId.HasValue && stallStock.HasValue)
                await EnsureBoxStockAsync(box.Id, stallId, stallStock.Value);
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

using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class ProductItemService : IProductItemService
    {
        private const int MaxImagesPerItem = 10;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrganizerAccessService _organizerAccess;

        public ProductItemService(IUnitOfWork unitOfWork, IOrganizerAccessService organizerAccess)
        {
            _unitOfWork = unitOfWork;
            _organizerAccess = organizerAccess;
        }

        public async Task<ProductItemResponse> CreateAsync(Guid productId, CreateProductItemRequest request)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            var product = await _unitOfWork.Products.GetByIdAsync(productId)
                ?? throw ErrorHelper.NotFound("Product not found.", "E5.5");

            if (product.OrganizerId != organizer.Id)
                throw ErrorHelper.Forbidden("You cannot modify this product.", "E5.4");

            var category = await _unitOfWork.CakeCategories.GetByIdAsync(request.CategoryId)
                ?? throw ErrorHelper.NotFound("Cake category not found.", "E5.5");

            var filling = await _unitOfWork.Fillings.GetByIdAsync(request.FillingId)
                ?? throw ErrorHelper.NotFound("Filling not found.", "E5.5");

            await EnsureCombinationAvailableAsync(productId, category.Id, filling.Id, request.EggCount, request.Weight, null);

            var item = new ProductItem
            {
                ProductId = productId,
                CategoryId = category.Id,
                FillingId = filling.Id,
                Name = string.IsNullOrWhiteSpace(request.Name)
                    ? $"{category.Name} {filling.Name} {request.Weight}g"
                    : request.Name.Trim(),
                EggCount = request.EggCount,
                Weight = request.Weight,
                LengthCm = request.LengthCm,
                WidthCm = request.WidthCm,
                HeightCm = request.HeightCm,
                Price = request.Price,
                Images = request.ImageUrls
                    .Select((url, index) => new ProductItemImage { Url = url.Trim(), SortOrder = index })
                    .ToList()
            };

            await _unitOfWork.ProductItems.AddAsync(item);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(item.Id);
        }

        public async Task<ProductItemResponse> UpdateAsync(Guid id, UpdateProductItemRequest request)
        {
            var item = await GetOwnedAsync(id);

            var categoryId = request.CategoryId ?? item.CategoryId;
            var fillingId = request.FillingId ?? item.FillingId;
            var eggCount = request.EggCount ?? item.EggCount;
            var weight = request.Weight ?? item.Weight;

            if (request.CategoryId.HasValue && request.CategoryId.Value != item.CategoryId
                && !await _unitOfWork.CakeCategories.GetQueryable().AnyAsync(c => c.Id == categoryId))
            {
                throw ErrorHelper.NotFound("Cake category not found.", "E5.5");
            }

            if (request.FillingId.HasValue && request.FillingId.Value != item.FillingId
                && !await _unitOfWork.Fillings.GetQueryable().AnyAsync(f => f.Id == fillingId))
            {
                throw ErrorHelper.NotFound("Filling not found.", "E5.5");
            }

            await EnsureCombinationAvailableAsync(item.ProductId, categoryId, fillingId, eggCount, weight, id);

            if (request.Price.HasValue)
            {
                var highestTier = await _unitOfWork.PriceTiers.GetQueryable()
                    .Where(t => t.ProductItemId == id)
                    .MaxAsync(t => (decimal?)t.UnitPrice);

                if (highestTier.HasValue && highestTier.Value >= request.Price.Value)
                    throw ErrorHelper.BadRequest("Price must stay above every wholesale tier price.", "E5.8");
            }

            item.CategoryId = categoryId;
            item.FillingId = fillingId;
            item.EggCount = eggCount;
            item.Weight = weight;

            if (request.Name is not null)
                item.Name = request.Name.Trim();

            if (request.LengthCm.HasValue)
                item.LengthCm = request.LengthCm.Value;

            if (request.WidthCm.HasValue)
                item.WidthCm = request.WidthCm.Value;

            if (request.HeightCm.HasValue)
                item.HeightCm = request.HeightCm.Value;

            if (request.Price.HasValue)
                item.Price = request.Price.Value;

            await _unitOfWork.ProductItems.Update(item);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var item = await GetOwnedAsync(id);

            await _unitOfWork.ProductItems.SoftRemove(item);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<ProductItemResponse> AddImageAsync(Guid id, AddProductItemImageRequest request)
        {
            await GetOwnedAsync(id);

            var existing = await _unitOfWork.ProductItemImages.GetQueryable()
                .Where(im => im.ProductItemId == id)
                .Select(im => im.SortOrder)
                .ToListAsync();

            if (existing.Count >= MaxImagesPerItem)
                throw ErrorHelper.BadRequest($"A product item can have at most {MaxImagesPerItem} images.");

            await _unitOfWork.ProductItemImages.AddAsync(new ProductItemImage
            {
                ProductItemId = id,
                Url = request.Url.Trim(),
                SortOrder = request.SortOrder ?? (existing.Count == 0 ? 0 : existing.Max() + 1)
            });
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task<ProductItemResponse> RemoveImageAsync(Guid id, Guid imageId)
        {
            await GetOwnedAsync(id);

            var images = await _unitOfWork.ProductItemImages.GetAllAsync(im => im.ProductItemId == id);
            var image = images.FirstOrDefault(im => im.Id == imageId)
                ?? throw ErrorHelper.NotFound("Image not found.", "E5.5");

            if (images.Count == 1)
                throw ErrorHelper.BadRequest("A product item must keep at least one image.", "E5.7");

            await _unitOfWork.ProductItemImages.SoftRemove(image);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task<ProductItemResponse> SetPriceTiersAsync(Guid id, List<PriceTierRequest> tiers)
        {
            var item = await GetOwnedAsync(id);

            CatalogRules.ValidateTiers(tiers, item.Price);

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var existing = await _unitOfWork.PriceTiers.GetAllAsync(t => t.ProductItemId == id);

                if (existing.Count > 0)
                {
                    await _unitOfWork.PriceTiers.HardRemoveRange(existing);
                    await _unitOfWork.SaveChangesAsync();
                }

                if (tiers.Count > 0)
                {
                    await _unitOfWork.PriceTiers.AddRangeAsync(tiers
                        .Select(t => new PriceTier
                        {
                            ProductItemId = id,
                            ItemType = OrderItemType.Product,
                            MinQuantity = t.MinQuantity,
                            UnitPrice = t.UnitPrice
                        })
                        .ToList());
                    await _unitOfWork.SaveChangesAsync();
                }

                return true;
            });

            return await GetResponseAsync(id);
        }

        public async Task<ProductItemResponse> SetStocksAsync(Guid id, List<StockRequest> stocks)
        {
            var item = await GetOwnedAsync(id);
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            await CatalogRules.ValidateStocksAsync(_unitOfWork, organizer.Id, stocks);

            var existing = await _unitOfWork.StallProductItems.GetAllAsync(s => s.ProductItemId == item.Id);
            var toAdd = new List<StallProductItem>();
            var toUpdate = new List<StallProductItem>();

            foreach (var request in stocks)
            {
                var row = existing.FirstOrDefault(s => s.StallId == request.StallId);

                if (row is null)
                {
                    toAdd.Add(new StallProductItem
                    {
                        ProductItemId = item.Id,
                        StallId = request.StallId,
                        StockQuantity = request.StockQuantity,
                        IsActive = request.IsActive
                    });
                    continue;
                }

                row.StockQuantity = request.StockQuantity;
                row.IsActive = request.IsActive;
                toUpdate.Add(row);
            }

            if (toAdd.Count > 0)
                await _unitOfWork.StallProductItems.AddRangeAsync(toAdd);

            if (toUpdate.Count > 0)
                await _unitOfWork.StallProductItems.UpdateRange(toUpdate);

            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        private async Task<ProductItem> GetOwnedAsync(Guid id)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            var item = await _unitOfWork.ProductItems.GetQueryable()
                .Include(i => i.Product)
                .FirstOrDefaultAsync(i => i.Id == id)
                ?? throw ErrorHelper.NotFound("Product item not found.", "E5.5");

            if (item.Product.OrganizerId != organizer.Id)
                throw ErrorHelper.Forbidden("You cannot modify this product item.", "E5.4");

            return item;
        }

        private async Task EnsureCombinationAvailableAsync(
            Guid productId, Guid categoryId, Guid fillingId, int eggCount, int weight, Guid? exceptId)
        {
            var exists = await _unitOfWork.ProductItems.GetQueryable().AnyAsync(i =>
                i.ProductId == productId
                && i.CategoryId == categoryId
                && i.FillingId == fillingId
                && i.EggCount == eggCount
                && i.Weight == weight
                && (exceptId == null || i.Id != exceptId));

            if (exists)
                throw ErrorHelper.Conflict("This category, filling, egg count and weight combination already exists in the product.", "E5.2");
        }

        private async Task<ProductItemResponse> GetResponseAsync(Guid id)
        {
            return await _unitOfWork.ProductItems.GetQueryable()
                .AsNoTracking()
                .Where(i => i.Id == id)
                .Select(ProductProjections.Item)
                .FirstAsync();
        }
    }
}

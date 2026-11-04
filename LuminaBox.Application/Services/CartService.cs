using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Dtos.Cart;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IClaimsService _claimsService;

        public CartService(IUnitOfWork unitOfWork, IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<CartResponse> GetMyCartAsync()
        {
            var userId = _claimsService.GetRequiredUserId();

            var cart = await _unitOfWork.Carts.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                return new CartResponse
                {
                    Id = Guid.Empty,
                    UserId = userId,
                    TotalItems = 0,
                    Groups = new List<CartGroupResponse>()
                };
            }

            var items = await _unitOfWork.CartItems.GetQueryable()
                .AsNoTracking()
                .Where(ci => ci.CartId == cart.Id)
                .OrderByDescending(ci => ci.CreatedAt)
                .Select(ci => new CartItemResponse
                {
                    Id = ci.Id,
                    ItemType = ci.ItemType,
                    OrganizerId = ci.ItemType == OrderItemType.Product
                        ? ci.ProductItem!.Product.OrganizerId
                        : ci.BoxItem!.OrganizerId,
                    OrganizerBrandName = ci.ItemType == OrderItemType.Product
                        ? ci.ProductItem!.Product.Organizer.BrandName
                        : ci.BoxItem!.Organizer.BrandName,
                    StallId = ci.StallId,
                    StallCode = ci.Stall != null ? ci.Stall.Code : null,
                    StallName = ci.Stall != null ? ci.Stall.Name : null,
                    ProductId = ci.ProductItem != null ? ci.ProductItem.ProductId : null,
                    ProductName = ci.ProductItem != null ? ci.ProductItem.Product.Name : null,
                    ProductItemId = ci.ProductItemId,
                    ProductItemName = ci.ProductItem != null ? ci.ProductItem.Name : null,
                    EggCount = ci.ProductItem != null ? ci.ProductItem.EggCount : null,
                    Weight = ci.ItemType == OrderItemType.Product
                        ? ci.ProductItem!.Weight
                        : ci.BoxItem!.Weight,
                    BoxItemId = ci.BoxItemId,
                    BoxItemName = ci.BoxItem != null ? ci.BoxItem.Name : null,
                    BoxCapacity = ci.BoxItem != null ? ci.BoxItem.Capacity : null,
                    ImageUrl = ci.ItemType == OrderItemType.Product
                        ? ci.ProductItem!.Images
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault()
                        : ci.BoxItem!.Images
                            .OrderBy(img => img.SortOrder)
                            .Select(img => img.Url)
                            .FirstOrDefault(),
                    CategoryId = ci.ProductItem != null ? ci.ProductItem.CategoryId : null,
                    CategoryName = ci.ProductItem != null ? ci.ProductItem.Category.Name : null,
                    FillingId = ci.ProductItem != null ? ci.ProductItem.FillingId : null,
                    FillingName = ci.ProductItem != null ? ci.ProductItem.Filling.Name : null,
                    Quantity = ci.Quantity,
                    UnitPrice = ci.UnitPrice,
                    Subtotal = ci.UnitPrice * ci.Quantity,
                    CreatedAt = ci.CreatedAt
                })
                .ToListAsync();

            var groups = items
                .GroupBy(i => new { i.OrganizerId, i.OrganizerBrandName, i.StallId, i.StallCode, i.StallName })
                .Select(g => new CartGroupResponse
                {
                    OrganizerId = g.Key.OrganizerId,
                    OrganizerBrandName = g.Key.OrganizerBrandName,
                    StallId = g.Key.StallId,
                    StallCode = g.Key.StallCode,
                    StallName = g.Key.StallName,
                    GroupSubtotal = g.Sum(x => x.Subtotal),
                    Items = g.ToList()
                })
                .ToList();

            return new CartResponse
            {
                Id = cart.Id,
                UserId = userId,
                TotalItems = items.Count,
                Groups = groups
            };
        }

        public async Task<CartResponse> AddProductToCartAsync(AddProductToCartRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var productItem = await _unitOfWork.ProductItems.GetQueryable()
                .Include(pi => pi.Product)
                    .ThenInclude(p => p.Organizer)
                .Include(pi => pi.PriceTiers)
                .Include(pi => pi.Stocks)
                .FirstOrDefaultAsync(pi => pi.Id == request.ProductItemId)
                ?? throw ErrorHelper.NotFound("Product item not found.");

            var product = productItem.Product;

            if (product.Status != ProductStatus.Active)
                throw ErrorHelper.BadRequest("Product is not active for sale.");

            if (product.Organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Conflict("Organizer is currently suspended.");

            await EnsureOnlineApprovedAsync(product.OrganizerId);

            var activeStocks = productItem.Stocks.Where(s => s.IsActive).ToList();
            var selectedStock = ResolveProductStock(activeStocks, request.StallId, productItem.Name);
            var resolvedStallId = selectedStock.StallId;

            if (resolvedStallId.HasValue)
            {
                await EnsureActiveStallRentalAsync(product.OrganizerId, resolvedStallId.Value);
            }

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ItemType == OrderItemType.Product
                                        && ci.ProductItemId == productItem.Id
                                        && ci.StallId == resolvedStallId);

            var targetQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
            if (selectedStock.StockQuantity < targetQuantity)
                throw ErrorHelper.Conflict($"Insufficient stock for '{productItem.Name}'. Available: {selectedStock.StockQuantity}.");

            var unitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, targetQuantity);

            if (existingItem is not null)
            {
                existingItem.Quantity = targetQuantity;
                existingItem.UnitPrice = unitPrice;
                await _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ItemType = OrderItemType.Product,
                    ProductItemId = productItem.Id,
                    Quantity = targetQuantity,
                    UnitPrice = unitPrice,
                    StallId = resolvedStallId
                };
                await _unitOfWork.CartItems.AddAsync(newItem);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetMyCartAsync();
        }

        public async Task<CartResponse> AddBoxToCartAsync(AddBoxToCartRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var box = await _unitOfWork.BoxItems.GetQueryable()
                .Include(b => b.Organizer)
                .Include(b => b.PriceTiers)
                .Include(b => b.Stocks)
                .FirstOrDefaultAsync(b => b.Id == request.BoxItemId)
                ?? throw ErrorHelper.NotFound("Box item not found.");

            if (box.Status != ProductStatus.Active)
                throw ErrorHelper.BadRequest("Box item is not active for sale.");

            if (box.Organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Conflict("Organizer is currently suspended.");

            await EnsureOnlineApprovedAsync(box.OrganizerId);

            var activeStocks = box.Stocks.Where(s => s.IsActive).ToList();
            var selectedStock = ResolveBoxStock(activeStocks, request.StallId, box.Name);
            var resolvedStallId = selectedStock.StallId;

            if (resolvedStallId.HasValue)
            {
                await EnsureActiveStallRentalAsync(box.OrganizerId, resolvedStallId.Value);
            }

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ItemType == OrderItemType.Box
                                        && ci.BoxItemId == box.Id
                                        && ci.StallId == resolvedStallId);

            var targetQuantity = (existingItem?.Quantity ?? 0) + request.Quantity;
            if (selectedStock.StockQuantity < targetQuantity)
                throw ErrorHelper.Conflict($"Insufficient stock for box '{box.Name}'. Available: {selectedStock.StockQuantity}.");

            var unitPrice = ResolveUnitPrice(box.Price, box.PriceTiers, targetQuantity);

            if (existingItem is not null)
            {
                existingItem.Quantity = targetQuantity;
                existingItem.UnitPrice = unitPrice;
                await _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ItemType = OrderItemType.Box,
                    BoxItemId = box.Id,
                    Quantity = targetQuantity,
                    UnitPrice = unitPrice,
                    StallId = resolvedStallId
                };
                await _unitOfWork.CartItems.AddAsync(newItem);
            }

            await _unitOfWork.SaveChangesAsync();

            return await GetMyCartAsync();
        }

        public async Task<CartResponse> UpdateCartItemAsync(Guid itemId, UpdateCartItemRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var item = await _unitOfWork.CartItems.GetQueryable()
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId)
                ?? throw ErrorHelper.NotFound("Cart item not found.");

            var targetQuantity = request.Quantity ?? item.Quantity;

            if (item.ItemType == OrderItemType.Box)
            {
                if (request.ProductItemId.HasValue)
                    throw ErrorHelper.BadRequest("Box cart item does not support changing ProductItemId.");

                var box = await _unitOfWork.BoxItems.GetQueryable()
                    .Include(b => b.PriceTiers)
                    .Include(b => b.Stocks)
                    .FirstOrDefaultAsync(b => b.Id == item.BoxItemId)
                    ?? throw ErrorHelper.NotFound("Box item not found.");

                var stock = box.Stocks.FirstOrDefault(s => s.StallId == item.StallId && s.IsActive)
                    ?? throw ErrorHelper.Conflict($"Box '{box.Name}' is no longer available at the selected stall.");

                if (stock.StockQuantity < targetQuantity)
                    throw ErrorHelper.Conflict($"Insufficient stock for box '{box.Name}'. Available: {stock.StockQuantity}.");

                item.Quantity = targetQuantity;
                item.UnitPrice = ResolveUnitPrice(box.Price, box.PriceTiers, targetQuantity);

                await _unitOfWork.CartItems.Update(item);
                await _unitOfWork.SaveChangesAsync();

                return await GetMyCartAsync();
            }

            var targetProductItemId = request.ProductItemId ?? item.ProductItemId!.Value;

            var productItem = await _unitOfWork.ProductItems.GetQueryable()
                .Include(pi => pi.Product)
                .Include(pi => pi.PriceTiers)
                .Include(pi => pi.Stocks)
                .FirstOrDefaultAsync(pi => pi.Id == targetProductItemId)
                ?? throw ErrorHelper.NotFound("Product item not found.");

            if (productItem.Product.Status != ProductStatus.Active)
                throw ErrorHelper.BadRequest("Product is not active for sale.");

            var productStock = productItem.Stocks.FirstOrDefault(s => s.StallId == item.StallId && s.IsActive)
                ?? throw ErrorHelper.Conflict($"Product item '{productItem.Name}' is not available at the selected stall.");

            if (targetProductItemId != item.ProductItemId)
            {
                var duplicateItem = await _unitOfWork.CartItems.GetQueryable()
                    .FirstOrDefaultAsync(ci => ci.Id != item.Id
                                            && ci.CartId == item.CartId
                                            && ci.ItemType == OrderItemType.Product
                                            && ci.ProductItemId == targetProductItemId
                                            && ci.StallId == item.StallId);

                if (duplicateItem is not null)
                {
                    var mergedQuantity = duplicateItem.Quantity + targetQuantity;
                    if (productStock.StockQuantity < mergedQuantity)
                        throw ErrorHelper.Conflict($"Insufficient stock for '{productItem.Name}'. Available: {productStock.StockQuantity}.");

                    duplicateItem.Quantity = mergedQuantity;
                    duplicateItem.UnitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, mergedQuantity);
                    await _unitOfWork.CartItems.Update(duplicateItem);
                    await _unitOfWork.CartItems.SoftRemove(item);
                    await _unitOfWork.SaveChangesAsync();

                    return await GetMyCartAsync();
                }

                item.ProductItemId = targetProductItemId;
            }

            if (productStock.StockQuantity < targetQuantity)
                throw ErrorHelper.Conflict($"Insufficient stock for '{productItem.Name}'. Available: {productStock.StockQuantity}.");

            item.Quantity = targetQuantity;
            item.UnitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, targetQuantity);

            await _unitOfWork.CartItems.Update(item);
            await _unitOfWork.SaveChangesAsync();

            return await GetMyCartAsync();
        }

        public async Task RemoveCartItemAsync(Guid itemId)
        {
            var userId = _claimsService.GetRequiredUserId();

            var item = await _unitOfWork.CartItems.GetQueryable()
                .Include(ci => ci.Cart)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.Cart.UserId == userId)
                ?? throw ErrorHelper.NotFound("Cart item not found.");

            await _unitOfWork.CartItems.SoftRemove(item);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _unitOfWork.Carts.GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is not null)
                return cart;

            cart = new Cart { UserId = userId };
            await _unitOfWork.Carts.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
            return cart;
        }

        private async Task EnsureOnlineApprovedAsync(Guid organizerId)
        {
            var hasOnlineApproved = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizerId
                            && r.Type == RegistrationType.Online
                            && r.Status == RegistrationStatus.Approved);

            if (!hasOnlineApproved)
                throw ErrorHelper.Conflict("Organizer is not authorized for online sales.");
        }

        private async Task EnsureActiveStallRentalAsync(Guid organizerId, Guid stallId)
        {
            var hasActiveRental = await _unitOfWork.StallRentals.GetQueryable()
                .AnyAsync(r => r.OrganizerId == organizerId
                            && r.StallId == stallId
                            && r.Status == RentalStatus.Active);

            if (!hasActiveRental)
                throw ErrorHelper.Conflict("The organizer does not have an active rental at the selected stall.");
        }

        private static StallProductItem ResolveProductStock(
            List<StallProductItem> activeStocks,
            Guid? requestedStallId,
            string itemName)
        {
            if (activeStocks.Count == 0)
                throw ErrorHelper.Conflict($"'{itemName}' is not available for sale.");

            if (requestedStallId.HasValue)
            {
                return activeStocks.FirstOrDefault(s => s.StallId == requestedStallId.Value)
                    ?? throw ErrorHelper.BadRequest($"Selected stall does not sell '{itemName}'.");
            }

            var onlineStock = activeStocks.FirstOrDefault(s => s.StallId == null);
            if (onlineStock is not null)
                return onlineStock;

            if (activeStocks.Count == 1)
                return activeStocks[0];

            throw ErrorHelper.BadRequest($"StallId is required when '{itemName}' is sold at multiple stalls.");
        }

        private static StallBoxItem ResolveBoxStock(
            List<StallBoxItem> activeStocks,
            Guid? requestedStallId,
            string boxName)
        {
            if (activeStocks.Count == 0)
                throw ErrorHelper.Conflict($"Box '{boxName}' is not available for sale.");

            if (requestedStallId.HasValue)
            {
                return activeStocks.FirstOrDefault(s => s.StallId == requestedStallId.Value)
                    ?? throw ErrorHelper.BadRequest($"Selected stall does not sell box '{boxName}'.");
            }

            var onlineStock = activeStocks.FirstOrDefault(s => s.StallId == null);
            if (onlineStock is not null)
                return onlineStock;

            if (activeStocks.Count == 1)
                return activeStocks[0];

            throw ErrorHelper.BadRequest($"StallId is required when box '{boxName}' is sold at multiple stalls.");
        }

        internal static decimal ResolveUnitPrice(decimal basePrice, IEnumerable<PriceTier> priceTiers, int quantity)
        {
            var tierPrice = priceTiers
                .Where(t => t.MinQuantity <= quantity)
                .OrderByDescending(t => t.MinQuantity)
                .Select(t => (decimal?)t.UnitPrice)
                .FirstOrDefault();

            return tierPrice ?? basePrice;
        }
    }
}

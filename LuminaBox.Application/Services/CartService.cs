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

        public CartService(
            IUnitOfWork unitOfWork,
            IClaimsService claimsService)
        {
            _unitOfWork = unitOfWork;
            _claimsService = claimsService;
        }

        public async Task<CartResponse> GetMyCartAsync()
        {
            var userId = _claimsService.GetRequiredUserId();
            var cart = await GetOrCreateCartAsync(userId);
            return await BuildCartResponseAsync(cart.Id, userId);
        }

        public async Task<CartResponse> AddProductToCartAsync(AddProductToCartRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var product = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Include(p => p.Organizer)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId)
                ?? throw ErrorHelper.NotFound("Product not found.");

            if (product.Status != ProductStatus.Active)
                throw ErrorHelper.BadRequest("Product is not active for sale.");

            if (product.Organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Conflict("Organizer is currently suspended.");

            await EnsureOnlineApprovedAsync(product.OrganizerId);

            var productItem = await _unitOfWork.ProductItems.GetQueryable()
                .AsNoTracking()
                .Include(pi => pi.PriceTiers)
                .Include(pi => pi.Stocks)
                .OrderBy(pi => pi.CreatedAt)
                .FirstOrDefaultAsync(pi => pi.ProductId == request.ProductId
                                        && pi.CategoryId == request.CategoryId
                                        && pi.FillingId == request.FillingId)
                ?? throw ErrorHelper.NotFound("Selected category and filling combination is not available for this product.");

            var activeStocks = productItem.Stocks.Where(s => s.IsActive).ToList();
            var selectedStock = ResolveProductStock(activeStocks, request.StallId);
            var resolvedStallId = selectedStock.StallId;

            if (resolvedStallId.HasValue)
                await EnsureActiveStallRentalAsync(product.OrganizerId, resolvedStallId.Value);

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ItemType == OrderItemType.Product
                                        && ci.ProductItemId == productItem.Id
                                        && ci.StallId == resolvedStallId);

            if (existingItem is not null)
            {
                var newQty = existingItem.Quantity + request.Quantity;
                if (newQty > 1000)
                    throw ErrorHelper.BadRequest("Total quantity for this item in cart cannot exceed 1000.");

                if (selectedStock.StockQuantity < newQty)
                    throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                existingItem.Quantity = newQty;
                existingItem.UnitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, newQty);
                await _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                if (selectedStock.StockQuantity < request.Quantity)
                    throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ItemType = OrderItemType.Product,
                    ProductItemId = productItem.Id,
                    BoxItemId = null,
                    StallId = resolvedStallId,
                    Quantity = request.Quantity,
                    UnitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, request.Quantity)
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            return await BuildCartResponseAsync(cart.Id, userId);
        }

        public async Task<CartResponse> AddBoxToCartAsync(AddBoxToCartRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();

            var box = await _unitOfWork.BoxItems.GetQueryable()
                .AsNoTracking()
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
            var selectedStock = ResolveBoxStock(activeStocks, request.StallId);
            var resolvedStallId = selectedStock.StallId;

            if (resolvedStallId.HasValue)
                await EnsureActiveStallRentalAsync(box.OrganizerId, resolvedStallId.Value);

            var cart = await GetOrCreateCartAsync(userId);

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ItemType == OrderItemType.Box
                                        && ci.BoxItemId == box.Id
                                        && ci.StallId == resolvedStallId);

            if (existingItem is not null)
            {
                var newQty = existingItem.Quantity + request.Quantity;
                if (newQty > 1000)
                    throw ErrorHelper.BadRequest("Total quantity for this box in cart cannot exceed 1000.");

                if (selectedStock.StockQuantity < newQty)
                    throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                existingItem.Quantity = newQty;
                existingItem.UnitPrice = ResolveUnitPrice(box.Price, box.PriceTiers, newQty);
                await _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                if (selectedStock.StockQuantity < request.Quantity)
                    throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                var cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ItemType = OrderItemType.Box,
                    ProductItemId = null,
                    BoxItemId = box.Id,
                    StallId = resolvedStallId,
                    Quantity = request.Quantity,
                    UnitPrice = ResolveUnitPrice(box.Price, box.PriceTiers, request.Quantity)
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }

            await _unitOfWork.SaveChangesAsync();
            return await BuildCartResponseAsync(cart.Id, userId);
        }

        public async Task<CartResponse> UpdateCartItemAsync(Guid itemId, UpdateCartItemRequest request)
        {
            var userId = _claimsService.GetRequiredUserId();
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await _unitOfWork.CartItems.GetQueryable()
                .Include(ci => ci.ProductItem)
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.CartId == cart.Id)
                ?? throw ErrorHelper.NotFound("Cart item not found.");

            var targetQuantity = request.Quantity ?? cartItem.Quantity;

            if (cartItem.ItemType == OrderItemType.Product)
            {
                var currentProductItem = cartItem.ProductItem
                    ?? await _unitOfWork.ProductItems.GetByIdAsync(cartItem.ProductItemId!.Value)
                    ?? throw ErrorHelper.NotFound("Product item not found.");

                var targetCategoryId = request.CategoryId ?? currentProductItem.CategoryId;
                var targetFillingId = request.FillingId ?? currentProductItem.FillingId;

                var productItem = await _unitOfWork.ProductItems.GetQueryable()
                    .AsNoTracking()
                    .Include(pi => pi.Product)
                    .Include(pi => pi.PriceTiers)
                    .Include(pi => pi.Stocks)
                    .OrderBy(pi => pi.CreatedAt)
                    .FirstOrDefaultAsync(pi => pi.ProductId == currentProductItem.ProductId
                                            && pi.CategoryId == targetCategoryId
                                            && pi.FillingId == targetFillingId)
                    ?? throw ErrorHelper.NotFound("Selected category and filling combination is not available for this product.");

                if (productItem.Product.Status != ProductStatus.Active)
                    throw ErrorHelper.BadRequest("Product is not active for sale.");

                var stockRow = productItem.Stocks
                    .FirstOrDefault(s => s.StallId == cartItem.StallId && s.IsActive)
                    ?? throw ErrorHelper.BadRequest("Selected product item is not available at the current stall.");

                if (targetCategoryId != currentProductItem.CategoryId || targetFillingId != currentProductItem.FillingId)
                {
                    var duplicate = await _unitOfWork.CartItems.GetQueryable()
                        .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                                && ci.Id != cartItem.Id
                                                && ci.ItemType == OrderItemType.Product
                                                && ci.ProductItemId == productItem.Id
                                                && ci.StallId == cartItem.StallId);

                    if (duplicate is not null)
                    {
                        var mergedQty = duplicate.Quantity + targetQuantity;
                        if (mergedQty > 1000)
                            throw ErrorHelper.BadRequest("Total quantity for this item in cart cannot exceed 1000.");

                        if (stockRow.StockQuantity < mergedQty)
                            throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                        duplicate.Quantity = mergedQty;
                        duplicate.UnitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, mergedQty);
                        await _unitOfWork.CartItems.Update(duplicate);
                        await _unitOfWork.CartItems.SoftRemove(cartItem);
                        await _unitOfWork.SaveChangesAsync();

                        return await BuildCartResponseAsync(cart.Id, userId);
                    }
                }

                if (stockRow.StockQuantity < targetQuantity)
                    throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                cartItem.ProductItemId = productItem.Id;
                cartItem.Quantity = targetQuantity;
                cartItem.UnitPrice = ResolveUnitPrice(productItem.Price, productItem.PriceTiers, targetQuantity);
                await _unitOfWork.CartItems.Update(cartItem);
            }
            else
            {
                if (request.CategoryId.HasValue || request.FillingId.HasValue)
                    throw ErrorHelper.BadRequest("Cannot update CategoryId or FillingId for a Box item in cart.");

                if (request.Quantity.HasValue)
                {
                    var box = await _unitOfWork.BoxItems.GetQueryable()
                        .AsNoTracking()
                        .Include(b => b.PriceTiers)
                        .Include(b => b.Stocks)
                        .FirstOrDefaultAsync(b => b.Id == cartItem.BoxItemId)
                        ?? throw ErrorHelper.NotFound("Box item not found.");

                    var stockRow = box.Stocks
                        .FirstOrDefault(s => s.StallId == cartItem.StallId && s.IsActive)
                        ?? throw ErrorHelper.BadRequest("Selected box item is not available at the current stall.");

                    if (stockRow.StockQuantity < targetQuantity)
                        throw ErrorHelper.Conflict("Requested quantity exceeds available stock.");

                    cartItem.Quantity = targetQuantity;
                    cartItem.UnitPrice = ResolveUnitPrice(box.Price, box.PriceTiers, targetQuantity);
                    await _unitOfWork.CartItems.Update(cartItem);
                }
            }

            await _unitOfWork.SaveChangesAsync();
            return await BuildCartResponseAsync(cart.Id, userId);
        }

        public async Task RemoveCartItemAsync(Guid itemId)
        {
            var userId = _claimsService.GetRequiredUserId();
            var cart = await GetOrCreateCartAsync(userId);

            var cartItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.Id == itemId && ci.CartId == cart.Id)
                ?? throw ErrorHelper.NotFound("Cart item not found.");

            await _unitOfWork.CartItems.SoftRemove(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }

        public static decimal ResolveUnitPrice(decimal basePrice, IEnumerable<PriceTier> tiers, int quantity)
        {
            var applicableTier = tiers
                .Where(t => quantity >= t.MinQuantity)
                .OrderByDescending(t => t.MinQuantity)
                .FirstOrDefault();

            return applicableTier?.UnitPrice ?? basePrice;
        }

        private async Task<Cart> GetOrCreateCartAsync(Guid userId)
        {
            var cart = await _unitOfWork.Carts.GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is not null)
                return cart;

            cart = new Cart
            {
                UserId = userId
            };
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

        private static StallProductItem ResolveProductStock(List<StallProductItem> activeStocks, Guid? requestedStallId)
        {
            if (activeStocks.Count == 0)
                throw ErrorHelper.Conflict("This product item is not available for sale.");

            if (requestedStallId.HasValue)
            {
                return activeStocks.FirstOrDefault(s => s.StallId == requestedStallId.Value)
                    ?? throw ErrorHelper.BadRequest("Selected stall does not sell this product item.");
            }

            var onlineStock = activeStocks.FirstOrDefault(s => s.StallId == null);
            if (onlineStock is not null)
                return onlineStock;

            if (activeStocks.Count == 1)
                return activeStocks[0];

            throw ErrorHelper.BadRequest("StallId is required when the product item is sold at multiple stalls.");
        }

        private static StallBoxItem ResolveBoxStock(List<StallBoxItem> activeStocks, Guid? requestedStallId)
        {
            if (activeStocks.Count == 0)
                throw ErrorHelper.Conflict("This box item is not available for sale.");

            if (requestedStallId.HasValue)
            {
                return activeStocks.FirstOrDefault(s => s.StallId == requestedStallId.Value)
                    ?? throw ErrorHelper.BadRequest("Selected stall does not sell this box item.");
            }

            var onlineStock = activeStocks.FirstOrDefault(s => s.StallId == null);
            if (onlineStock is not null)
                return onlineStock;

            if (activeStocks.Count == 1)
                return activeStocks[0];

            throw ErrorHelper.BadRequest("StallId is required when the box item is sold at multiple stalls.");
        }

        private async Task<CartResponse> BuildCartResponseAsync(Guid cartId, Guid userId)
        {
            var items = await _unitOfWork.CartItems.GetQueryable()
                .AsNoTracking()
                .Where(ci => ci.CartId == cartId)
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
                    ProductItemName = ci.ProductItem != null ? ci.ProductItem.Name : null,
                    EggCount = ci.ProductItem != null ? ci.ProductItem.EggCount : null,
                    Weight = ci.ProductItem != null ? ci.ProductItem.Weight : null,
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
                Id = cartId,
                UserId = userId,
                TotalItems = items.Count,
                Groups = groups
            };
        }
    }
}

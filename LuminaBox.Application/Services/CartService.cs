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
                        ? ci.Product!.OrganizerId
                        : ci.BoxItem!.OrganizerId,
                    OrganizerBrandName = ci.ItemType == OrderItemType.Product
                        ? ci.Product!.Organizer.BrandName
                        : ci.BoxItem!.Organizer.BrandName,
                    StallId = ci.StallId,
                    StallCode = ci.Stall != null ? ci.Stall.Code : null,
                    StallName = ci.Stall != null ? ci.Stall.Name : null,
                    ProductId = ci.ProductId,
                    ProductName = ci.Product != null ? ci.Product.Name : null,
                    BoxItemId = ci.BoxItemId,
                    BoxItemName = ci.BoxItem != null ? ci.BoxItem.Name : null,
                    BoxCapacity = ci.BoxItem != null ? ci.BoxItem.Capacity : null,
                    ImageUrl = ci.ItemType == OrderItemType.Product
                        ? ci.Product!.Images
                            .OrderByDescending(img => img.IsPrimary)
                            .ThenBy(img => img.SortOrder)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault()
                        : ci.BoxItem!.Images
                            .OrderByDescending(img => img.IsPrimary)
                            .ThenBy(img => img.SortOrder)
                            .Select(img => img.ImageUrl)
                            .FirstOrDefault(),
                    CategoryId = ci.CategoryId,
                    CategoryName = ci.Category != null ? ci.Category.Name : null,
                    FillingId = ci.FillingId,
                    FillingName = ci.Filling != null ? ci.Filling.Name : null,
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

            var product = await _unitOfWork.Products.GetQueryable()
                .Include(p => p.Organizer)
                .Include(p => p.Stalls)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId)
                ?? throw ErrorHelper.NotFound("Product not found.");

            if (product.Status != ProductStatus.Active)
                throw ErrorHelper.BadRequest("Product is not active for sale.");

            if (product.Organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Conflict("Organizer is currently suspended.");

            var hasOnlineApproved = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == product.OrganizerId
                            && r.Type == RegistrationType.Online
                            && r.Status == RegistrationStatus.Approved);

            if (!hasOnlineApproved)
                throw ErrorHelper.Conflict("Organizer is not authorized for online sales.");

            Guid? resolvedStallId;
            if (product.Stalls.Count == 0)
            {
                if (request.StallId.HasValue)
                    throw ErrorHelper.BadRequest("This product is sold online only and does not belong to any stall.");

                resolvedStallId = null;
            }
            else if (product.Stalls.Count == 1)
            {
                var singleStall = product.Stalls.First();
                if (request.StallId.HasValue && request.StallId.Value != singleStall.Id)
                    throw ErrorHelper.BadRequest("Selected stall does not belong to this product.");

                resolvedStallId = singleStall.Id;
            }
            else
            {
                if (!request.StallId.HasValue)
                    throw ErrorHelper.BadRequest("StallId is required when product is sold at multiple stalls.");

                if (!product.Stalls.Any(s => s.Id == request.StallId.Value))
                    throw ErrorHelper.BadRequest("Selected stall does not belong to this product.");

                resolvedStallId = request.StallId.Value;
            }

            var variant = await _unitOfWork.ProductVariants.GetQueryable()
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.ProductId == product.Id
                                       && v.CategoryId == request.CategoryId
                                       && v.FillingId == request.FillingId)
                ?? throw ErrorHelper.NotFound("Product variant with the selected category and filling not found.");

            var cart = await _unitOfWork.Carts.GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ItemType == OrderItemType.Product
                                        && ci.ProductId == product.Id
                                        && ci.CategoryId == request.CategoryId
                                        && ci.FillingId == request.FillingId
                                        && ci.StallId == resolvedStallId);

            if (existingItem is not null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UnitPrice = variant.Price;
                await _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ItemType = OrderItemType.Product,
                    ProductId = product.Id,
                    CategoryId = request.CategoryId,
                    FillingId = request.FillingId,
                    Quantity = request.Quantity,
                    UnitPrice = variant.Price,
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
                .Include(b => b.Stalls)
                .FirstOrDefaultAsync(b => b.Id == request.BoxItemId)
                ?? throw ErrorHelper.NotFound("Box item not found.");

            if (box.Status != ProductStatus.Active)
                throw ErrorHelper.BadRequest("Box item is not active for sale.");

            if (box.Organizer.Status != OrganizerStatus.Active)
                throw ErrorHelper.Conflict("Organizer is currently suspended.");

            var hasOnlineApproved = await _unitOfWork.OrganizerRegistrations.GetQueryable()
                .AnyAsync(r => r.OrganizerId == box.OrganizerId
                            && r.Type == RegistrationType.Online
                            && r.Status == RegistrationStatus.Approved);

            if (!hasOnlineApproved)
                throw ErrorHelper.Conflict("Organizer is not authorized for online sales.");

            Guid? resolvedStallId;
            if (box.Stalls.Count == 0)
            {
                if (request.StallId.HasValue)
                    throw ErrorHelper.BadRequest("This box item is sold online only and does not belong to any stall.");

                resolvedStallId = null;
            }
            else if (box.Stalls.Count == 1)
            {
                var singleStall = box.Stalls.First();
                if (request.StallId.HasValue && request.StallId.Value != singleStall.Id)
                    throw ErrorHelper.BadRequest("Selected stall does not belong to this box item.");

                resolvedStallId = singleStall.Id;
            }
            else
            {
                if (!request.StallId.HasValue)
                    throw ErrorHelper.BadRequest("StallId is required when box item is sold at multiple stalls.");

                if (!box.Stalls.Any(s => s.Id == request.StallId.Value))
                    throw ErrorHelper.BadRequest("Selected stall does not belong to this box item.");

                resolvedStallId = request.StallId.Value;
            }

            var cart = await _unitOfWork.Carts.GetQueryable()
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart is null)
            {
                cart = new Cart { UserId = userId };
                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();
            }

            var existingItem = await _unitOfWork.CartItems.GetQueryable()
                .FirstOrDefaultAsync(ci => ci.CartId == cart.Id
                                        && ci.ItemType == OrderItemType.Box
                                        && ci.BoxItemId == box.Id
                                        && ci.StallId == resolvedStallId);

            if (existingItem is not null)
            {
                existingItem.Quantity += request.Quantity;
                existingItem.UnitPrice = box.Price;
                await _unitOfWork.CartItems.Update(existingItem);
            }
            else
            {
                var newItem = new CartItem
                {
                    CartId = cart.Id,
                    ItemType = OrderItemType.Box,
                    BoxItemId = box.Id,
                    Quantity = request.Quantity,
                    UnitPrice = box.Price,
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

            if (item.ItemType == OrderItemType.Box && (request.CategoryId.HasValue || request.FillingId.HasValue))
                throw ErrorHelper.BadRequest("Box cart item does not support changing CategoryId or FillingId.");

            var targetQuantity = request.Quantity.HasValue ? request.Quantity.Value : item.Quantity;

            if (item.ItemType == OrderItemType.Product && (request.CategoryId.HasValue || request.FillingId.HasValue))
            {
                var newCategoryId = request.CategoryId ?? item.CategoryId!.Value;
                var newFillingId = request.FillingId ?? item.FillingId!.Value;

                if (newCategoryId != item.CategoryId || newFillingId != item.FillingId)
                {
                    var variant = await _unitOfWork.ProductVariants.GetQueryable()
                        .AsNoTracking()
                        .FirstOrDefaultAsync(v => v.ProductId == item.ProductId
                                               && v.CategoryId == newCategoryId
                                               && v.FillingId == newFillingId)
                        ?? throw ErrorHelper.NotFound("Product variant with the selected category and filling not found.");

                    var duplicateItem = await _unitOfWork.CartItems.GetQueryable()
                        .FirstOrDefaultAsync(ci => ci.Id != item.Id
                                                && ci.CartId == item.CartId
                                                && ci.ItemType == OrderItemType.Product
                                                && ci.ProductId == item.ProductId
                                                && ci.CategoryId == newCategoryId
                                                && ci.FillingId == newFillingId
                                                && ci.StallId == item.StallId);

                    if (duplicateItem is not null)
                    {
                        duplicateItem.Quantity += targetQuantity;
                        duplicateItem.UnitPrice = variant.Price;
                        await _unitOfWork.CartItems.Update(duplicateItem);
                        await _unitOfWork.CartItems.SoftRemove(item);
                        await _unitOfWork.SaveChangesAsync();

                        return await GetMyCartAsync();
                    }

                    item.CategoryId = newCategoryId;
                    item.FillingId = newFillingId;
                    item.UnitPrice = variant.Price;
                }
            }

            if (request.Quantity.HasValue && item.Quantity != request.Quantity.Value)
                item.Quantity = request.Quantity.Value;

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
    }
}


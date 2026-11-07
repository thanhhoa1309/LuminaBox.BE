using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Box;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class BoxItemService : IBoxItemService
    {
        private const int MaxImagesPerBox = 10;

        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrganizerAccessService _organizerAccess;
        private readonly IMapper _mapper;

        public BoxItemService(IUnitOfWork unitOfWork, IOrganizerAccessService organizerAccess, IMapper mapper)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _organizerAccess = organizerAccess;
        }

        public async Task<Pagination<BoxItemResponse>> GetAllAsync(
            string? search,
            int page,
            int pageSize,
            Guid? stallId = null,
            Guid? organizerId = null)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.BoxItems.GetQueryable()
                .AsNoTracking()
                .Where(b => b.Status == ProductStatus.Active
                    && b.Organizer.Status == OrganizerStatus.Active
                    && b.Stocks.Any(s => s.IsActive
                        && s.StockQuantity > 0
                        && (stallId == null || s.StallId == stallId)));

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(keyword));
            }

            if (organizerId.HasValue)
                query = query.Where(b => b.OrganizerId == organizerId.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<BoxItemResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new Pagination<BoxItemResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<BoxItemResponse> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.BoxItems.GetQueryable()
                .AsNoTracking()
                .Where(b => b.Id == id && b.Status == ProductStatus.Active && b.Organizer.Status == OrganizerStatus.Active)
                .ProjectTo<BoxItemResponse>(_mapper.ConfigurationProvider)
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Box item not found.", "E9.4");
        }

        public async Task<Pagination<BoxItemResponse>> GetMineAsync(string? search, int page, int pageSize, ProductStatus? status = null)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.BoxItems.GetQueryable()
                .AsNoTracking()
                .Where(b => b.OrganizerId == organizer.Id);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(b => b.Name.ToLower().Contains(keyword));
            }

            if (status.HasValue)
                query = query.Where(b => b.Status == status.Value);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(b => b.CreatedAt)
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ProjectTo<BoxItemResponse>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new Pagination<BoxItemResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<BoxItemResponse> CreateAsync(CreateBoxItemRequest request)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            var box = _mapper.Map<BoxItem>(request);
            box.OrganizerId = organizer.Id;

            await _unitOfWork.BoxItems.AddAsync(box);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(box.Id);
        }

        public async Task<BoxItemResponse> UpdateAsync(Guid id, UpdateBoxItemRequest request)
        {
            var box = await GetOwnedAsync(id);

            if (request.Price.HasValue)
            {
                var highestTier = await _unitOfWork.PriceTiers.GetQueryable()
                    .Where(t => t.BoxItemId == id)
                    .MaxAsync(t => (decimal?)t.UnitPrice);

                if (highestTier.HasValue && highestTier.Value >= request.Price.Value)
                    throw ErrorHelper.BadRequest("Price must stay above every wholesale tier price.", "E5.8");
            }

            _mapper.Map(request, box);

            await _unitOfWork.BoxItems.Update(box);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task<BoxItemResponse> SetStatusAsync(Guid id, ProductStatus status)
        {
            var box = await GetOwnedAsync(id);

            box.Status = status;

            await _unitOfWork.BoxItems.Update(box);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var box = await GetOwnedAsync(id);

            await _unitOfWork.BoxItems.SoftRemove(box);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<BoxItemResponse> AddImageAsync(Guid id, AddProductItemImageRequest request)
        {
            await GetOwnedAsync(id);

            var existing = await _unitOfWork.BoxItemImages.GetQueryable()
                .Where(im => im.BoxItemId == id)
                .Select(im => im.SortOrder)
                .ToListAsync();

            if (existing.Count >= MaxImagesPerBox)
                throw ErrorHelper.BadRequest($"A box item can have at most {MaxImagesPerBox} images.");

            await _unitOfWork.BoxItemImages.AddAsync(new BoxItemImage
            {
                BoxItemId = id,
                Url = request.Url.Trim(),
                SortOrder = request.SortOrder ?? (existing.Count == 0 ? 0 : existing.Max() + 1)
            });
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task<BoxItemResponse> RemoveImageAsync(Guid id, Guid imageId)
        {
            await GetOwnedAsync(id);

            var images = await _unitOfWork.BoxItemImages.GetAllAsync(im => im.BoxItemId == id);
            var image = images.FirstOrDefault(im => im.Id == imageId)
                ?? throw ErrorHelper.NotFound("Image not found.", "E5.5");

            if (images.Count == 1)
                throw ErrorHelper.BadRequest("A box item must keep at least one image.", "E5.7");

            await _unitOfWork.BoxItemImages.SoftRemove(image);
            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        public async Task<BoxItemResponse> SetPriceTiersAsync(Guid id, List<PriceTierRequest> tiers)
        {
            var box = await GetOwnedAsync(id);

            CatalogRules.ValidateTiers(tiers, box.Price);

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var existing = await _unitOfWork.PriceTiers.GetAllAsync(t => t.BoxItemId == id);

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
                            BoxItemId = id,
                            ItemType = OrderItemType.Box,
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

        public async Task<BoxItemResponse> SetStocksAsync(Guid id, List<StockRequest> stocks)
        {
            var box = await GetOwnedAsync(id);

            await CatalogRules.ValidateStocksAsync(_unitOfWork, box.OrganizerId, stocks);

            var existing = await _unitOfWork.StallBoxItems.GetAllAsync(s => s.BoxItemId == id);
            var toAdd = new List<StallBoxItem>();
            var toUpdate = new List<StallBoxItem>();

            foreach (var request in stocks)
            {
                var row = existing.FirstOrDefault(s => s.StallId == request.StallId);

                if (row is null)
                {
                    toAdd.Add(new StallBoxItem
                    {
                        BoxItemId = id,
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
                await _unitOfWork.StallBoxItems.AddRangeAsync(toAdd);

            if (toUpdate.Count > 0)
                await _unitOfWork.StallBoxItems.UpdateRange(toUpdate);

            await _unitOfWork.SaveChangesAsync();

            return await GetResponseAsync(id);
        }

        private async Task<BoxItem> GetOwnedAsync(Guid id)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            var box = await _unitOfWork.BoxItems.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Box item not found.", "E5.5");

            if (box.OrganizerId != organizer.Id)
                throw ErrorHelper.Forbidden("You cannot modify this box item.", "E5.4");

            return box;
        }

        private async Task<BoxItemResponse> GetResponseAsync(Guid id)
        {
            return await _unitOfWork.BoxItems.GetQueryable()
                .AsNoTracking()
                .Where(b => b.Id == id)
                .ProjectTo<BoxItemResponse>(_mapper.ConfigurationProvider)
                .FirstAsync();
        }
    }
}

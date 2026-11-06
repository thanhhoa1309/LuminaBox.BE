using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Product;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Enums;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrganizerAccessService _organizerAccess;

        public ProductService(IUnitOfWork unitOfWork, IOrganizerAccessService organizerAccess)
        {
            _unitOfWork = unitOfWork;
            _organizerAccess = organizerAccess;
        }

        public async Task<Pagination<ProductSummaryResponse>> GetAllAsync(ProductListQuery filter)
        {
            var pagination = new PaginationParameter { PageIndex = filter.Page, PageSize = filter.PageSize };

            var query = _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Where(p => p.Status == ProductStatus.Active
                    && p.Organizer.Status == OrganizerStatus.Active
                    && p.Items.Any());

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(keyword));
            }

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.Items.Any(i => i.CategoryId == filter.CategoryId.Value));

            if (filter.FillingId.HasValue)
                query = query.Where(p => p.Items.Any(i => i.FillingId == filter.FillingId.Value));

            if (filter.OrganizerId.HasValue)
                query = query.Where(p => p.OrganizerId == filter.OrganizerId.Value);

            if (filter.StallId.HasValue)
                query = query.Where(p => p.Items.Any(i => i.Stocks.Any(s =>
                    s.StallId == filter.StallId.Value && s.IsActive && s.StockQuantity > 0)));

            var totalCount = await query.CountAsync();

            query = ApplySort(query, filter.SortBy, filter.IsDescending);

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(ProductProjections.Summary)
                .ToListAsync();

            return new Pagination<ProductSummaryResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<ProductResponse> GetByIdAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Where(p => p.Id == id && p.Status == ProductStatus.Active && p.Organizer.Status == OrganizerStatus.Active)
                .Select(ProductProjections.Detail)
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Product not found.", "E5.5");

            await AttachItemsAsync(new List<ProductResponse> { product });
            return product;
        }

        public async Task<Pagination<ProductResponse>> GetMineAsync(MyProductListQuery filter)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();
            var pagination = new PaginationParameter { PageIndex = filter.Page, PageSize = filter.PageSize };

            var query = _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Where(p => p.OrganizerId == organizer.Id);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var keyword = filter.Search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(keyword));
            }

            if (filter.Status.HasValue)
                query = query.Where(p => p.Status == filter.Status.Value);

            if (filter.CategoryId.HasValue)
                query = query.Where(p => p.Items.Any(i => i.CategoryId == filter.CategoryId.Value));

            var totalCount = await query.CountAsync();

            query = ApplySort(query, filter.SortBy, filter.IsDescending);

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(ProductProjections.Detail)
                .ToListAsync();

            await AttachItemsAsync(items);

            return new Pagination<ProductResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<ProductResponse> CreateAsync(CreateProductRequest request)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            var product = new Product
            {
                OrganizerId = organizer.Id,
                Name = request.Name.Trim(),
                Description = Normalize(request.Description)
            };

            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            return await GetOwnedResponseAsync(product.Id);
        }

        public async Task<ProductResponse> UpdateAsync(Guid id, UpdateProductRequest request)
        {
            var product = await GetOwnedAsync(id);

            if (request.Name is not null)
                product.Name = request.Name.Trim();

            if (request.Description is not null)
                product.Description = Normalize(request.Description);

            await _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return await GetOwnedResponseAsync(id);
        }

        public async Task<ProductResponse> SetStatusAsync(Guid id, UpdateProductStatusRequest request)
        {
            var product = await GetOwnedAsync(id);

            if (request.Status == ProductStatus.Active
                && !await _unitOfWork.ProductItems.GetQueryable().AnyAsync(i => i.ProductId == id))
            {
                throw ErrorHelper.BadRequest("A product needs at least one item before it can be activated.");
            }

            product.Status = request.Status;

            await _unitOfWork.Products.Update(product);
            await _unitOfWork.SaveChangesAsync();

            return await GetOwnedResponseAsync(id);
        }

        public async Task DeleteAsync(Guid id)
        {
            var product = await GetOwnedAsync(id);

            await _unitOfWork.ExecuteTransactionAsync(async () =>
            {
                var items = await _unitOfWork.ProductItems.GetAllAsync(i => i.ProductId == id);

                if (items.Count > 0)
                    await _unitOfWork.ProductItems.SoftRemoveRange(items);

                await _unitOfWork.Products.SoftRemove(product);
                await _unitOfWork.SaveChangesAsync();
                return true;
            });
        }

        private async Task<Product> GetOwnedAsync(Guid id)
        {
            var organizer = await _organizerAccess.GetSellingOrganizerAsync();

            var product = await _unitOfWork.Products.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Product not found.", "E5.5");

            if (product.OrganizerId != organizer.Id)
                throw ErrorHelper.Forbidden("You cannot modify this product.", "E5.4");

            return product;
        }

        private async Task<ProductResponse> GetOwnedResponseAsync(Guid id)
        {
            var product = await _unitOfWork.Products.GetQueryable()
                .AsNoTracking()
                .Where(p => p.Id == id)
                .Select(ProductProjections.Detail)
                .FirstAsync();

            await AttachItemsAsync(new List<ProductResponse> { product });
            return product;
        }

        private async Task AttachItemsAsync(List<ProductResponse> products)
        {
            if (products.Count == 0)
                return;

            var ids = products.Select(p => p.Id).ToList();

            var items = await _unitOfWork.ProductItems.GetQueryable()
                .AsNoTracking()
                .Where(i => ids.Contains(i.ProductId))
                .OrderBy(i => i.CreatedAt)
                .Select(ProductProjections.Item)
                .ToListAsync();

            var byProduct = items.ToLookup(i => i.ProductId);

            foreach (var product in products)
                product.Items = byProduct[product.Id].ToList();
        }

        private static IQueryable<Product> ApplySort(IQueryable<Product> query, string? sortBy, bool isDescending)
        {
            return sortBy?.Trim().ToLowerInvariant() switch
            {
                "name" => isDescending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                "price" => isDescending
                    ? query.OrderByDescending(p => p.Items.Min(i => (decimal?)i.Price))
                    : query.OrderBy(p => p.Items.Min(i => (decimal?)i.Price)),
                _ => isDescending ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt)
            };
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

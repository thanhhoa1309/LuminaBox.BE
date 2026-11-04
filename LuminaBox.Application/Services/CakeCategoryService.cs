using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Catalog;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class CakeCategoryService : ICakeCategoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public CakeCategoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Pagination<LookupResponse>> GetAllAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.CakeCategories.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(c => c.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "createdat" => isDescending ? query.OrderByDescending(c => c.CreatedAt) : query.OrderBy(c => c.CreatedAt),
                _ => isDescending ? query.OrderByDescending(c => c.Name) : query.OrderBy(c => c.Name)
            };

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(c => new LookupResponse { Id = c.Id, Name = c.Name, Description = c.Description })
                .ToListAsync();

            return new Pagination<LookupResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<LookupResponse> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.CakeCategories.GetQueryable()
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new LookupResponse { Id = c.Id, Name = c.Name, Description = c.Description })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Cake category not found.", "E5.5");
        }

        public async Task<LookupResponse> CreateAsync(LookupRequest request)
        {
            var name = request.Name.Trim();
            await EnsureNameAvailableAsync(name, null);

            var category = new CakeCategory { Name = name, Description = Normalize(request.Description) };
            await _unitOfWork.CakeCategories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            return new LookupResponse { Id = category.Id, Name = category.Name, Description = category.Description };
        }

        public async Task<LookupResponse> UpdateAsync(Guid id, LookupRequest request)
        {
            var category = await _unitOfWork.CakeCategories.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Cake category not found.", "E5.5");

            var name = request.Name.Trim();
            await EnsureNameAvailableAsync(name, id);

            category.Name = name;
            category.Description = Normalize(request.Description);

            await _unitOfWork.CakeCategories.Update(category);
            await _unitOfWork.SaveChangesAsync();

            return new LookupResponse { Id = category.Id, Name = category.Name, Description = category.Description };
        }

        public async Task DeleteAsync(Guid id)
        {
            var category = await _unitOfWork.CakeCategories.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Cake category not found.", "E5.5");

            if (await _unitOfWork.ProductItems.GetQueryable().AnyAsync(i => i.CategoryId == id))
                throw ErrorHelper.Conflict("Cake category is in use by product items.");

            await _unitOfWork.CakeCategories.SoftRemove(category);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureNameAvailableAsync(string name, Guid? exceptId)
        {
            var lowered = name.ToLower();
            var taken = await _unitOfWork.CakeCategories.GetQueryable()
                .AnyAsync(c => c.Name.ToLower() == lowered && (exceptId == null || c.Id != exceptId));

            if (taken)
                throw ErrorHelper.Conflict("Cake category name already exists.");
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

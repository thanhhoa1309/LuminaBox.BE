using Microsoft.EntityFrameworkCore;
using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Catalog;
using LuminaBox.Application.Interface;
using LuminaBox.Application.Utils;
using LuminaBox.Domain.Entities;
using LuminaBox.Domain.Interfaces;

namespace LuminaBox.Application.Services
{
    public class FillingService : IFillingService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FillingService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Pagination<LookupResponse>> GetAllAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize)
        {
            var pagination = new PaginationParameter { PageIndex = page, PageSize = pageSize };

            var query = _unitOfWork.Fillings.GetQueryable().AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var keyword = search.Trim().ToLower();
                query = query.Where(f => f.Name.ToLower().Contains(keyword));
            }

            var totalCount = await query.CountAsync();

            query = sortBy?.Trim().ToLowerInvariant() switch
            {
                "createdat" => isDescending ? query.OrderByDescending(f => f.CreatedAt) : query.OrderBy(f => f.CreatedAt),
                _ => isDescending ? query.OrderByDescending(f => f.Name) : query.OrderBy(f => f.Name)
            };

            var items = await query
                .Skip((pagination.PageIndex - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .Select(f => new LookupResponse { Id = f.Id, Name = f.Name, Description = f.Description })
                .ToListAsync();

            return new Pagination<LookupResponse>(items, totalCount, pagination.PageIndex, pagination.PageSize);
        }

        public async Task<LookupResponse> GetByIdAsync(Guid id)
        {
            return await _unitOfWork.Fillings.GetQueryable()
                .AsNoTracking()
                .Where(f => f.Id == id)
                .Select(f => new LookupResponse { Id = f.Id, Name = f.Name, Description = f.Description })
                .FirstOrDefaultAsync()
                ?? throw ErrorHelper.NotFound("Filling not found.", "E5.5");
        }

        public async Task<LookupResponse> CreateAsync(LookupRequest request)
        {
            var name = request.Name.Trim();
            await EnsureNameAvailableAsync(name, null);

            var filling = new Filling { Name = name, Description = Normalize(request.Description) };
            await _unitOfWork.Fillings.AddAsync(filling);
            await _unitOfWork.SaveChangesAsync();

            return new LookupResponse { Id = filling.Id, Name = filling.Name, Description = filling.Description };
        }

        public async Task<LookupResponse> UpdateAsync(Guid id, LookupRequest request)
        {
            var filling = await _unitOfWork.Fillings.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Filling not found.", "E5.5");

            var name = request.Name.Trim();
            await EnsureNameAvailableAsync(name, id);

            filling.Name = name;
            filling.Description = Normalize(request.Description);

            await _unitOfWork.Fillings.Update(filling);
            await _unitOfWork.SaveChangesAsync();

            return new LookupResponse { Id = filling.Id, Name = filling.Name, Description = filling.Description };
        }

        public async Task DeleteAsync(Guid id)
        {
            var filling = await _unitOfWork.Fillings.GetByIdAsync(id)
                ?? throw ErrorHelper.NotFound("Filling not found.", "E5.5");

            if (await _unitOfWork.ProductItems.GetQueryable().AnyAsync(i => i.FillingId == id))
                throw ErrorHelper.Conflict("Filling is in use by product items.");

            await _unitOfWork.Fillings.SoftRemove(filling);
            await _unitOfWork.SaveChangesAsync();
        }

        private async Task EnsureNameAvailableAsync(string name, Guid? exceptId)
        {
            var lowered = name.ToLower();
            var taken = await _unitOfWork.Fillings.GetQueryable()
                .AnyAsync(f => f.Name.ToLower() == lowered && (exceptId == null || f.Id != exceptId));

            if (taken)
                throw ErrorHelper.Conflict("Filling name already exists.");
        }

        private static string? Normalize(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}

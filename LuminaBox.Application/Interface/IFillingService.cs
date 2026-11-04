using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Catalog;

namespace LuminaBox.Application.Interface
{
    public interface IFillingService
    {
        Task<Pagination<LookupResponse>> GetAllAsync(string? search, string? sortBy, bool isDescending, int page, int pageSize);

        Task<LookupResponse> GetByIdAsync(Guid id);

        Task<LookupResponse> CreateAsync(LookupRequest request);

        Task<LookupResponse> UpdateAsync(Guid id, LookupRequest request);

        Task DeleteAsync(Guid id);
    }
}

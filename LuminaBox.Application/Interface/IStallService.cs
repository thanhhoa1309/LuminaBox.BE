using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Stall;
using LuminaBox.Domain.Enums;

namespace LuminaBox.Application.Interface
{
    public interface IStallService
    {
        Task<Pagination<StallResponse>> GetAllAsync(
            string? search,
            int page,
            int pageSize,
            StallStatus? status = null,
            Guid? provinceId = null,
            Guid? districtId = null,
            Guid? wardId = null);

        Task<StallResponse> GetByIdAsync(Guid id);

        Task<List<StallResponse>> GetMineAsync();

        Task<StallResponse> CreateAsync(CreateStallRequest request);

        Task<StallResponse> UpdateAsync(Guid id, UpdateStallRequest request);

        Task DeleteAsync(Guid id);
    }
}

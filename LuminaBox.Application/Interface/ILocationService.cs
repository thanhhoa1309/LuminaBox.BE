using LuminaBox.Application.Commons;
using LuminaBox.Application.Dtos.Geography;

namespace LuminaBox.Application.Interface
{
    public interface ILocationService
    {
        Task<Pagination<ProvinceResponse>> GetProvincesAsync(string? search, int page, int pageSize);

        Task<Pagination<DistrictResponse>> GetDistrictsAsync(Guid provinceId, string? search, int page, int pageSize);

        Task<Pagination<WardResponse>> GetWardsAsync(Guid districtId, string? search, int page, int pageSize);

        /// <summary>Throws NotFound when the ward does not exist.</summary>
        Task EnsureWardExistsAsync(Guid wardId);
    }
}
